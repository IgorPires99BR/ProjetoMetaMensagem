using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.MarcarComoLida
{
    public class MarcarComoLidaHandler : IRequestHandler<MarcarComoLidaCommand, Response<MarcarComoLidaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarcarComoLidaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<MarcarComoLidaResult>> Handle(MarcarComoLidaCommand command)
        {
            var response = new Response<MarcarComoLidaResult>();

            try
            {
                await _unitOfWork.MensagemRecebida.MarcarTodasComoLidas(command.EmpresaId, command.ContatoId);
                //_unitOfWork.Commit();

                response.AddValue(new MarcarComoLidaResult());
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao marcar mensagens como lidas: {ex.Message}");
            }

            return response;
        }
    }
}
