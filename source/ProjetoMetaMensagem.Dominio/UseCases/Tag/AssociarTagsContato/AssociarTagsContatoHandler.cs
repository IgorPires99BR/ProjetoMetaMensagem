using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.AssociarTagsContato
{
    public class AssociarTagsContatoHandler : IRequestHandler<AssociarTagsContatoCommand, Response<AssociarTagsContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public AssociarTagsContatoHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<AssociarTagsContatoResult>> Handle(AssociarTagsContatoCommand command)
        {
            var response = new Response<AssociarTagsContatoResult>();

            try
            {
                await _unitOfWork.Tag.AssociarTagsContato(command.ContatoId, command.TagIds);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
