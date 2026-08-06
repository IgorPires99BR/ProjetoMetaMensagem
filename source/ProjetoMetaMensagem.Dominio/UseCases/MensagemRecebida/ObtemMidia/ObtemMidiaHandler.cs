using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ObtemMidia
{
    public class ObtemMidiaHandler : IRequestHandler<ObtemMidiaCommand, Response<ObtemMidiaResult>>
    {
        private readonly IMetaService _metaService;
        private readonly IUnitOfWork _unitOfWork;

        public ObtemMidiaHandler(IMetaService metaService, IUnitOfWork unitOfWork)
        {
            _metaService = metaService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<ObtemMidiaResult>> Handle(ObtemMidiaCommand command)
        {
            var response = new Response<ObtemMidiaResult>();

            try
            {
                if (string.IsNullOrWhiteSpace(command.MidiaId) || command.EmpresaId == Guid.Empty)
                {
                    response.AddErro("MidiaId e EmpresaId são obrigatórios.");
                    return response;
                }

                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.EmpresaId);

                if (string.IsNullOrEmpty(token))
                {
                    response.AddErro("Empresa sem token de acesso à Meta configurado.");
                    return response;
                }

                var (bytes, mimeType) = await _metaService.BaixarMidiaAsync(command.MidiaId, token);

                response.AddValue(new ObtemMidiaResult
                {
                    Bytes = bytes,
                    MimeType = mimeType
                });
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao obter mídia: {ex.Message}");
            }

            return response;
        }
    }
}
