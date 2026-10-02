using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.Common;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraFotoNumero
{
    public class AlteraFotoNumeroHandler : IRequestHandler<AlteraFotoNumeroCommand, Response<AlteraFotoNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly ILogger<AlteraFotoNumeroHandler> _logger;

        public AlteraFotoNumeroHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<AlteraFotoNumeroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<AlteraFotoNumeroResult>> Handle(AlteraFotoNumeroCommand command)
        {
            var response = new Response<AlteraFotoNumeroResult>();

            var validateResult = new AlteraFotoNumeroValidator().Validate(command);
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

                var empresaId = await NumeroMetaAcesso.EmpresaDoNumeroAsync(_unitOfWork, numero);
                var appId = empresaId.HasValue ? await _unitOfWork.Empresa.ObterAppIdMeta(empresaId.Value) : null;

                if (string.IsNullOrEmpty(appId))
                {
                    response.AddErro("A empresa deste número não tem o App ID da Meta cadastrado. Preencha-o na tela de Empresas antes de trocar a foto.");
                    return response;
                }

                var token = await NumeroMetaAcesso.ObterTokenAsync(_unitOfWork, numero);

                // A Meta não recebe a imagem direto no perfil: primeiro sobe pela Resumable Upload
                // API (a mesma da mídia de exemplo dos templates) e o perfil recebe só o handle.
                var handle = await _metaService.UploadMidiaExemploAsync(appId, token!, command.Arquivo, command.MimeType);

                await _metaService.AtualizarPerfilNumeroAsync(numero.InstanciaId!, token!, new PerfilNumeroEnvio { FotoHandle = handle });

                response.AddValue(new AlteraFotoNumeroResult { NumeroId = numero.Id });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AlteraFotoNumeroHandler));
            }

            return response;
        }
    }
}
