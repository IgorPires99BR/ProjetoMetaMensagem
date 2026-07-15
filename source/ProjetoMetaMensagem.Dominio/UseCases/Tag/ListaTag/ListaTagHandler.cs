using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.ListaTag
{
    public class ListaTagHandler : IRequestHandler<ListaTagCommand, Response<List<ListaTagResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListaTagHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<List<ListaTagResult>>> Handle(ListaTagCommand command)
        {
            var response = new Response<List<ListaTagResult>>();

            try
            {
                var listaResultados = new List<ListaTagResult>();

                var tags = await _unitOfWork.Tag.ListarPorEmpresa(command.EmpresaId);

                foreach (var tag in tags)
                {
                    listaResultados.Add(new ListaTagResult(tag));
                }

                response.AddValue(listaResultados);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
