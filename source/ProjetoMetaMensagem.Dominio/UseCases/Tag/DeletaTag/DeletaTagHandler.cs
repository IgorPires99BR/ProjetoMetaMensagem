using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.DeletaTag
{
    public class DeletaTagHandler : IRequestHandler<DeletaTagCommand, Response<DeletaTagResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeletaTagHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<DeletaTagResult>> Handle(DeletaTagCommand command)
        {
            var response = new Response<DeletaTagResult>();

            try
            {
                await _unitOfWork.Tag.Excluir(command.Id);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
