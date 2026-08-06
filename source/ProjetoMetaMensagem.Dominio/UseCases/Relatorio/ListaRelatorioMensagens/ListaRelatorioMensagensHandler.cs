using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ListaRelatorioMensagens
{
    public class ListaRelatorioMensagensHandler : IRequestHandler<ListaRelatorioMensagensCommand, Response<ListaRelatorioMensagensResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ListaRelatorioMensagensHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<ListaRelatorioMensagensResult>> Handle(ListaRelatorioMensagensCommand command)
        {
            var response = new Response<ListaRelatorioMensagensResult>();

            try
            {
                var mensagens = await _unitOfWork.Relatorio.ListarMensagens(
                    command.EmpresaId, command.DataInicio, command.DataFim, command.Pagina, command.TamanhoPagina);

                response.AddValue(new ListaRelatorioMensagensResult { Mensagens = mensagens });
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao gerar relatório de mensagens: {ex.Message}");
            }

            return response;
        }
    }
}
