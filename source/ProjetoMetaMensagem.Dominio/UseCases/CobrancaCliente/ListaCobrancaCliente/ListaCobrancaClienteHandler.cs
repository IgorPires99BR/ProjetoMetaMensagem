using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.ListaCobrancaCliente
{
    public class ListaCobrancaClienteHandler : IRequestHandler<ListaCobrancaClienteCommand, Response<ListaCobrancaClienteResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListaCobrancaClienteHandler> _logger;

        public ListaCobrancaClienteHandler(IUnitOfWork unitOfWork, ILogger<ListaCobrancaClienteHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ListaCobrancaClienteResult>> Handle(ListaCobrancaClienteCommand command)
        {
            var response = new Response<ListaCobrancaClienteResult>();

            var validator = new ListaCobrancaClienteValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var cobrancas = (await _unitOfWork.CobrancaCliente.ObterPorEmpresa(command.EmpresaIdSolicitante, command.Status, command.DataInicio, command.DataFim)).ToList();

                // IContatoRepository.ObterPorIds e escopado a UMA empresa (protecao contra
                // vazamento entre empresas); admin da plataforma pode listar cobrancas de varias
                // empresas de uma vez, entao busca contato por grupo de empresa, nao tudo junto.
                // Contato precisa vir qualificado (Entidades.Contato): dentro deste namespace,
                // "Contato" bare resolveria pro namespace UseCases.Contato (irmao deste),
                // nao pro tipo -- mesmo motivo que EnviarMensagemTemplateMetaLoteHandler ja
                // qualifica todo uso de Contato.
                var contatoPorId = new Dictionary<Guid, Entidades.Contato>();
                foreach (var grupo in cobrancas.GroupBy(c => c.EmpresaId))
                {
                    var contatos = await _unitOfWork.Contato.ObterPorIds(grupo.Key, grupo.Select(c => c.ContatoId).Distinct());
                    foreach (var contato in contatos)
                        contatoPorId[contato.Id] = contato;
                }

                var resultado = new ListaCobrancaClienteResult
                {
                    Cobrancas = cobrancas
                        .Select(c =>
                        {
                            contatoPorId.TryGetValue(c.ContatoId, out var contato);
                            return new CobrancaClienteResumo(c, contato?.NomeCliente, contato?.NomeContato, contato?.Telefone);
                        })
                        .ToList()
                };

                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaCobrancaClienteHandler));
            }

            return response;
        }
    }
}
