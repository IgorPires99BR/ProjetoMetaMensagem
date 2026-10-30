using FluentValidation;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemProvedorCobranca
{
    public class ObtemProvedorCobrancaHandler : IRequestHandler<ObtemProvedorCobrancaCommand, Response<ObtemProvedorCobrancaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProvedoresCobranca _provedores;
        private readonly ILogger<ObtemProvedorCobrancaHandler> _logger;

        public ObtemProvedorCobrancaHandler(IUnitOfWork unitOfWork, IProvedoresCobranca provedores, ILogger<ObtemProvedorCobrancaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _provedores = provedores;
            _logger = logger;
        }

        public async Task<Response<ObtemProvedorCobrancaResult>> Handle(ObtemProvedorCobrancaCommand command)
        {
            var response = new Response<ObtemProvedorCobrancaResult>();
            try
            {
                var validateResult = new ObtemProvedorCobrancaValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(command.EmpresaId);
                var destino = DestinoRecebimento.Resolver(dados, HorarioBrasilia.Agora());

                string? provedor;
                try { provedor = destino.Provedor ?? _provedores.Ativo.Nome; }
                catch (PixException) { provedor = null; }

                response.AddValue(new ObtemProvedorCobrancaResult
                {
                    CobrancaPixAtiva = dados?.CobrancaPixAtiva == true,
                    Provedor = provedor,
                    NomeProvedor = provedor == null ? null : ProvedorCobranca.NomeExibicao(provedor),
                    ContaDaEmpresa = !destino.NaPlataforma
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemProvedorCobrancaHandler));
            }

            return response;
        }
    }
}
