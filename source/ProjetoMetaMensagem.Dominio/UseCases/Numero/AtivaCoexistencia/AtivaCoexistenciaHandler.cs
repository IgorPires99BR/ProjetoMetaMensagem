using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Enums;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AtivaCoexistencia
{
    public class AtivaCoexistenciaHandler : IRequestHandler<AtivaCoexistenciaCommand, Response<AtivaCoexistenciaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        private readonly ILogger<AtivaCoexistenciaHandler> _logger;

        public AtivaCoexistenciaHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<AtivaCoexistenciaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<AtivaCoexistenciaResult>> Handle(AtivaCoexistenciaCommand command)
        {
            var response = new Response<AtivaCoexistenciaResult>();

            var validator = new AtivaCoexistenciaValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var numero = await _unitOfWork.Numero.ObterPorId(command.NumeroId);

                if (numero == null)
                {
                    response.AddErro("Número não encontrado.");
                    return response;
                }

                if (string.IsNullOrEmpty(numero.InstanciaId))
                {
                    response.AddErro("O número ainda não possui um identificador (InstanciaId) válido na Meta.");
                    return response;
                }

                var accessToken = !string.IsNullOrEmpty(numero.SystemUserToken)
                    ? numero.SystemUserToken
                    : await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                var resultadoMeta = await _metaService.AtivarCoexistenciaAsync(numero.InstanciaId, accessToken, command.Pin);

                if (!resultadoMeta.Sucesso)
                {
                    response.AddErro($"Falha ao ativar coexistência na Meta: {resultadoMeta.Erro}");
                    return response;
                }

                numero.TipoConexao = TipoConexaoNumero.Coexistencia;
                numero.StatusConexao = resultadoMeta.StatusConexao;
                numero.DataUltimaSincronizacao = DateTime.Now;
                numero.DataAtualizacao = DateTime.Now;

                var linhasAfetadas = await _unitOfWork.Numero.Alterar(numero, command.IdEmpresa);

                // Zero linhas: numero inexistente ou de outra empresa. O NumeroId chega pela
                // query string, entao sem esse recorte daria pra ativar coexistencia em numero
                // alheio. Mesma mensagem do "nao existe", pra nao confirmar o id ao atacante.
                if (linhasAfetadas == 0)
                {
                    response.AddErro("Número não encontrado.");
                    return response;
                }

                response.AddValue(new AtivaCoexistenciaResult
                {
                    NumeroId = numero.Id,
                    StatusConexao = numero.StatusConexao
                });
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(AtivaCoexistenciaHandler)));
            }

            return response;
        }
    }
}
