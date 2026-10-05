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

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.AlteraContato
{
    public class AlteraContatoHandler : IRequestHandler<AlteraContatoCommand, Response<AlteraContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<AlteraContatoHandler> _logger;

        public AlteraContatoHandler(IUnitOfWork unitOfWork, ILogger<AlteraContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AlteraContatoResult>> Handle(AlteraContatoCommand command)
        {
            var response = new Response<AlteraContatoResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new AlteraContatoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                // Contato inexistente ou de outra empresa: mesma mensagem nos dois casos, pra nao
                // confirmar ao atacante que o id existe.
                var atual = await _unitOfWork.Contato.ObterPorId(command.Id, command.EmpresaIdSolicitante);
                if (atual == null)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Contato não encontrado.");
                    return response;
                }

                // So valida o numero que mudou: contatos que ja dividem numero (a mesma pessoa
                // responde por varias empresas do cliente, caso da Sebrecon) continuam editaveis,
                // mas a edicao nao cria duplicado novo -- mesma regra do cadastro.
                foreach (var numero in NumerosNovos(atual, command.Telefone, command.Telefone2))
                {
                    if (await _unitOfWork.Contato.ExisteOutroComTelefone(atual.EmpresaId, numero, atual.Id))
                    {
                        _unitOfWork.Rollback();
                        response.AddErro($"Já existe outro contato cadastrado com o telefone {numero}.");
                        return response;
                    }
                }

                var linhasAfetadas = await _unitOfWork.Contato.Alterar(
                    new Entidades.Contato(command), command.EmpresaIdSolicitante);

                // Zero linhas: contato excluido entre a leitura acima e o UPDATE.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Contato não encontrado.");
                    return response;
                }

                response.AddValue(new AlteraContatoResult());
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(AlteraContatoHandler));
            }

            return response;
        }

        // Compara so os digitos: trocar o principal pelo Telefone2 (ou so a formatacao) nao e
        // numero novo.
        public static IEnumerable<string> NumerosNovos(Entidades.Contato atual, string telefone, string? telefone2)
        {
            static string Digitos(string? t) => new string((t ?? "").Where(char.IsDigit).ToArray());

            var jaDoContato = new[] { Digitos(atual.Telefone), Digitos(atual.Telefone2) };

            return new[] { telefone, telefone2 }
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t!.Trim())
                .Where(t => Digitos(t).Length > 0 && !jaDoContato.Contains(Digitos(t)))
                .ToList();
        }
    }
}


