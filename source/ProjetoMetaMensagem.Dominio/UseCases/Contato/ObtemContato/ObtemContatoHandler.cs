using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato
{
    public class ObtemContatoHandler : IRequestHandler<ObtemContatoCommand, Response<List<ObtemContatoResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ObtemContatoHandler> _logger;

        public ObtemContatoHandler(IUnitOfWork unitOfWork, ILogger<ObtemContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ObtemContatoResult>>> Handle(ObtemContatoCommand command)
        {
            var response = new Response<List<ObtemContatoResult>>();

            try
            {
                var listaContato = new List<ObtemContatoResult>();

                var validator = new ObtemContatoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var contatos = (await _unitOfWork.Contato.ObterPorEmpresa(command.EmpresaIdSolicitante)).ToList();

                // usuarioParaEmpresa resolve a empresa de cada contato pelo dono dele (Contato
                // nao guarda EmpresaId direto -- mesmo padrao do resto do dominio). So precisa
                // ser montado no caminho de plataforma (EmpresaIdSolicitante nulo): no caminho
                // comum, todo contato retornado ja e da mesma empresa conhecida.
                Dictionary<Guid, Guid> usuarioParaEmpresa = command.EmpresaIdSolicitante.HasValue
                    ? new Dictionary<Guid, Guid>()
                    : (await _unitOfWork.Usuario.Obter()).ToDictionary(u => u.Id, u => u.EmpresaId);

                Guid? EmpresaDoContato(Entidades.Contato c) =>
                    command.EmpresaIdSolicitante ?? (usuarioParaEmpresa.TryGetValue(c.UsuarioId, out var e) ? e : null);

                // ObterPorEmpresa(null) e o caminho da conta de plataforma: devolve contato de
                // TODAS as empresas de uma vez -- e o unico jeito de mostrar origem pra quem
                // realmente usa a tela de Contatos no dia a dia aqui, ja que os dois usuarios
                // reais da empresa que roda a campanha (Contact Solution) sao contas de
                // plataforma. Busca a origem empresa por empresa (nunca o conjunto inteiro sem
                // filtro), pra um telefone de uma empresa nunca casar com o de outra que usa o
                // mesmo numero.
                var empresasAlvo = contatos.Select(EmpresaDoContato).Where(e => e.HasValue).Select(e => e!.Value).Distinct();

                var origens = new List<Entidades.OrigemLead>();
                foreach (var empresaId in empresasAlvo)
                {
                    origens.AddRange(await _unitOfWork.OrigemLead.ListarPorEmpresa(empresaId));
                }

                var origensPorEmpresaETelefone = AgruparOrigemMaisAntigaPorEmpresaETelefone(origens);

                foreach (var contato in contatos)
                {
                    var empresaId = EmpresaDoContato(contato);
                    Entidades.OrigemLead? origem = null;
                    if (empresaId.HasValue)
                    {
                        origensPorEmpresaETelefone.TryGetValue((empresaId.Value, contato.Telefone), out origem);
                    }

                    listaContato.Add(new ObtemContatoResult(contato, origem));
                }

                response.AddValue(listaContato);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemContatoHandler));
            }

            return response;
        }

        // Separado do Handle pra poder ser testado sem banco. Chave composta (EmpresaId +
        // Telefone), nunca so o telefone -- o caminho de plataforma junta origem de varias
        // empresas na mesma lista, e so o telefone colidiria se duas empresas diferentes
        // tivessem, por coincidencia, um lead com o mesmo numero.
        //
        // Um telefone pode ter mais de um registro de origem na mesma empresa (reapareceu por
        // um segundo anuncio depois que a conversa reiniciou) -- a primeira mensagem com
        // anuncio e a que teve o merito de trazer o lead, entao ela e a que fica mostrada.
        public static Dictionary<(Guid EmpresaId, string Telefone), Entidades.OrigemLead> AgruparOrigemMaisAntigaPorEmpresaETelefone(IEnumerable<Entidades.OrigemLead> origens)
        {
            return origens
                .GroupBy(o => (o.EmpresaId, o.Telefone))
                .ToDictionary(g => g.Key, g => g.OrderBy(o => o.DataPrimeiroContato).First());
        }
    }
}
