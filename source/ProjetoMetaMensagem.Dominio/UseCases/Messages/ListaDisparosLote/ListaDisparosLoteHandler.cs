using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.ListaDisparosLote
{
    public class ListaDisparosLoteHandler : IRequestHandler<ListaDisparosLoteCommand, Response<ListaDisparosLoteResult>>
    {
        private const int QuantidadeRecentes = 5;

        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListaDisparosLoteHandler> _logger;

        public ListaDisparosLoteHandler(IUnitOfWork unitOfWork, ILogger<ListaDisparosLoteHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ListaDisparosLoteResult>> Handle(ListaDisparosLoteCommand command)
        {
            var response = new Response<ListaDisparosLoteResult>();

            if (command.EmpresaIdSolicitante.HasValue)
                command.EmpresaId = command.EmpresaIdSolicitante.Value;

            var validateResult = new ListaDisparosLoteValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var resultado = new ListaDisparosLoteResult();

                if (command.LoteId.HasValue)
                {
                    var lote = await _unitOfWork.DisparoLote.ObterResumo(command.LoteId.Value, command.EmpresaIdSolicitante);
                    if (lote == null)
                    {
                        response.AddErro("Envio não encontrado.", 404);
                        return response;
                    }

                    lote.Pendencias = (await _unitOfWork.DisparoLote.ObterPendencias(lote.Id)).ToList();
                    resultado.Lotes.Add(lote);
                }
                else
                {
                    resultado.Lotes = (await _unitOfWork.DisparoLote.ListarRecentes(command.EmpresaId, QuantidadeRecentes)).ToList();
                }

                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaDisparosLoteHandler));
            }

            return response;
        }
    }
}
