using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemSituacaoSubconta
{
    public class ObtemSituacaoSubcontaHandler : IRequestHandler<ObtemSituacaoSubcontaCommand, Response<ObtemSituacaoSubcontaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPixService _pix;
        private readonly ICriptografiaService _criptografia;
        private readonly ILogger<ObtemSituacaoSubcontaHandler> _logger;

        public ObtemSituacaoSubcontaHandler(IUnitOfWork unitOfWork, IPixService pix, ICriptografiaService criptografia,
            ILogger<ObtemSituacaoSubcontaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _pix = pix;
            _criptografia = criptografia;
            _logger = logger;
        }

        public async Task<Response<ObtemSituacaoSubcontaResult>> Handle(ObtemSituacaoSubcontaCommand command)
        {
            var response = new Response<ObtemSituacaoSubcontaResult>();

            try
            {
                var validateResult = new ObtemSituacaoSubcontaValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(command.EmpresaId);
                if (dados == null || !dados.TemSubconta)
                {
                    response.AddErro("A empresa ainda não tem subconta Asaas.");
                    return response;
                }

                if (string.IsNullOrWhiteSpace(dados.AsaasApiKeyCriptografada))
                {
                    response.AddValue(new ObtemSituacaoSubcontaResult { ConsultaDisponivel = false, CarteiraExterna = dados.CarteiraExterna });
                    return response;
                }

                if (!_criptografia.Configurada)
                {
                    response.AddErro("A criptografia do servidor não está configurada (Criptografia:Chave). Avise o suporte.");
                    return response;
                }

                SituacaoSubconta situacao;
                try
                {
                    situacao = await _pix.ConsultarSituacaoSubcontaAsync(_criptografia.Descriptografar(dados.AsaasApiKeyCriptografada));
                }
                catch (PixException ex)
                {
                    response.AddErro(ex.Message);
                    return response;
                }

                response.AddValue(new ObtemSituacaoSubcontaResult
                {
                    ConsultaDisponivel = true,
                    Geral = situacao.Geral,
                    DadosComerciais = situacao.DadosComerciais,
                    DadosBancarios = situacao.DadosBancarios,
                    Documentacao = situacao.Documentacao,
                    MotivoRecusa = situacao.MotivoRecusa,
                    Documentos = situacao.Documentos
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemSituacaoSubcontaHandler));
            }

            return response;
        }
    }
}
