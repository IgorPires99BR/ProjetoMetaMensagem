using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.CobrancaCliente.AlteraAtivoCobrancaCliente
{
    public class AlteraAtivoCobrancaClienteHandler : IRequestHandler<AlteraAtivoCobrancaClienteCommand, Response<AlteraAtivoCobrancaClienteResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPreparadorCobrancaPix _preparadorPix;
        private readonly ILogger<AlteraAtivoCobrancaClienteHandler> _logger;

        public AlteraAtivoCobrancaClienteHandler(IUnitOfWork unitOfWork, IPreparadorCobrancaPix preparadorPix,
                                                 ILogger<AlteraAtivoCobrancaClienteHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _preparadorPix = preparadorPix;
            _logger = logger;
        }

        public async Task<Response<AlteraAtivoCobrancaClienteResult>> Handle(AlteraAtivoCobrancaClienteCommand command)
        {
            var response = new Response<AlteraAtivoCobrancaClienteResult>();

            var validateResult = new AlteraAtivoCobrancaClienteValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var cobranca = await _unitOfWork.CobrancaCliente.ObterPorId(command.CobrancaClienteId, command.EmpresaIdSolicitante);
                if (cobranca == null)
                {
                    response.AddErro("Cobrança não encontrada.", 404);
                    return response;
                }

                if (cobranca.Ativo == command.Ativo)
                {
                    response.AddErro(command.Ativo ? "Esta cobrança já está ativa." : "Esta cobrança já está inativa.");
                    return response;
                }

                if (!command.Ativo && StatusCobrancaCliente.EmAberto(cobranca.Status) && !string.IsNullOrEmpty(cobranca.PixIdExterno))
                {
                    // Sem isso o cliente continuaria podendo pagar, pelo link que ja recebeu, uma
                    // cobranca que saiu da tela de quem a acompanha.
                    var erro = await _preparadorPix.EncerrarPixAsync(cobranca);
                    if (erro != null)
                    {
                        response.AddErro(erro);
                        return response;
                    }
                }

                var linhasAfetadas = command.Ativo
                    ? await _unitOfWork.CobrancaCliente.Reativar(cobranca.Id, command.EmpresaIdSolicitante)
                    : await _unitOfWork.CobrancaCliente.Inativar(cobranca.Id, command.EmpresaIdSolicitante, HorarioBrasilia.Agora());

                if (linhasAfetadas == 0)
                {
                    response.AddErro("Cobrança não encontrada.", 404);
                    return response;
                }

                response.AddValue(new AlteraAtivoCobrancaClienteResult { CobrancaClienteId = cobranca.Id, Ativo = command.Ativo });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AlteraAtivoCobrancaClienteHandler));
            }

            return response;
        }
    }
}
