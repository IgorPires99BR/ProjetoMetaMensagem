using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.Common;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.ObtemPerfilNumero
{
    public class ObtemPerfilNumeroHandler : IRequestHandler<ObtemPerfilNumeroCommand, Response<ObtemPerfilNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly ILogger<ObtemPerfilNumeroHandler> _logger;

        public ObtemPerfilNumeroHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<ObtemPerfilNumeroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<ObtemPerfilNumeroResult>> Handle(ObtemPerfilNumeroCommand command)
        {
            var response = new Response<ObtemPerfilNumeroResult>();

            var validateResult = new ObtemPerfilNumeroValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var (numero, erro) = await NumeroMetaAcesso.CarregarAsync(_unitOfWork, command.NumeroId, command.EmpresaIdSolicitante);
                if (numero == null)
                {
                    response.AddErro(erro!);
                    return response;
                }

                var token = await NumeroMetaAcesso.ObterTokenAsync(_unitOfWork, numero);
                var perfil = await _metaService.ObterPerfilNumeroAsync(numero.InstanciaId!, token!);

                response.AddValue(new ObtemPerfilNumeroResult(numero, perfil));
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemPerfilNumeroHandler));
            }

            return response;
        }
    }
}
