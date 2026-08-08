using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.ListaCampanha
{
    public class ListaCampanhaHandler : IRequestHandler<ListaCampanhaCommand, Response<List<ListaCampanhaResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ListaCampanhaHandler> _logger;

        public ListaCampanhaHandler(IUnitOfWork unitOfWork, ILogger<ListaCampanhaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ListaCampanhaResult>>> Handle(ListaCampanhaCommand command)
        {
            var response = new Response<List<ListaCampanhaResult>>();

            try
            {
                var campanhas = await _unitOfWork.Campanha.Listar(command.EmpresaId);

                var listaResult = campanhas.Select(c => new ListaCampanhaResult
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    DataAgendamento = c.DataAgendamento,
                    Status = c.Status,
                    TotalContatos = c.TotalContatos,
                    Processados = c.Processados
                }).ToList();

                response.AddValue(listaResult);
            }
            catch (Exception ex)
            {
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(ListaCampanhaHandler)));
            }

            return response;
        }
    }
}
