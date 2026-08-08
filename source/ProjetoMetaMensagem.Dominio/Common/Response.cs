using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjetoMetaMensagem.Dominio.Common
{
    // Distingue erro de NEGOCIO (regra/validacao: id invalido, duplicado, nao encontrado --
    // mensagem sempre segura pra mostrar ao usuario) de erro de SERVICO (falha tecnica: banco,
    // integracao externa como Meta/SMTP -- a mensagem ja vem generica e traduzida pelo
    // TratamentoErro, o detalhe tecnico fica so no log). O front usa isso pra decidir entre
    // mostrar a mensagem direto ou cair num "algo deu errado, tente novamente".
    public enum TipoErro
    {
        Negocio,
        Servico
    }

    public class Erro
    {
        public string Mensagem { get; }
        public TipoErro Tipo { get; }

        // Codigo HTTP que esse erro especifico exige (ex: 404 "nao encontrado", 403 "acesso
        // negado"). Null deixa o controller decidir o codigo padrao do endpoint.
        public int? StatusCode { get; }

        public Erro(string mensagem, TipoErro tipo, int? statusCode = null)
        {
            Mensagem = mensagem;
            Tipo = tipo;
            StatusCode = statusCode;
        }

        // Protege quem ainda trata Erros como texto (ex: string.Join, log) sem precisar
        // atualizar cada ponto de chamada.
        public override string ToString() => Mensagem;
    }

    // Classe base para todas as respostas. Implementa IResponse.
    public class Response : IResponse
    {
        private readonly List<Erro> _erros = new List<Erro>();

        public bool HasValidations => _erros.Any();
        public IReadOnlyCollection<Erro> Erros => _erros;

        // Erro de negocio: regra/validacao que o proprio handler detectou. statusCode
        // opcional força um codigo HTTP especifico (ex: 404, 403) em vez do padrao 400.
        public void AddErro(string mensagem, int? statusCode = null)
        {
            _erros.Add(new Erro(mensagem, TipoErro.Negocio, statusCode));
        }

        public void AddErros(IEnumerable<string> mensagens)
        {
            foreach (var mensagem in mensagens)
                _erros.Add(new Erro(mensagem, TipoErro.Negocio));
        }

        // Erro de servico: traduz a excecao tecnica (SQL, integracao externa) pra uma
        // mensagem generica seguindo o TratamentoErro, loga o detalhe original e marca
        // como Servico -- assim o front sabe distinguir "sua acao foi rejeitada" (Negocio)
        // de "algo quebrou, tente de novo" (Servico) sem depender do texto da mensagem.
        public void AddErroServico(Exception excecao, ILogger? logger, string operacao)
        {
            TratamentoErro.Registrar(logger, excecao, operacao);
            _erros.Add(new Erro(TratamentoErro.Traduzir(excecao), TipoErro.Servico, 500));
        }
    }

    public class Response<T> : Response
    {
        public T Value { get; private set; }

        public Response()
        {
        }

        public Response(T value) : this() // Construtor para sucesso com dados
        {
            Value = value;
        }

        public void AddValue(T value)
        {
            Value = value;
        }
    }
}
