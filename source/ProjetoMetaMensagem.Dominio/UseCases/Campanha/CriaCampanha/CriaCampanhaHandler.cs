using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.CriaCampanha
{
    public class CriaCampanhaHandler : IRequestHandler<CriaCampanhaCommand, Response<CriaCampanhaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CriaCampanhaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<CriaCampanhaResult>> Handle(CriaCampanhaCommand command)
        {
            var response = new Response<CriaCampanhaResult>();

            try
            {
                var campanha = new Entidades.Campanha
                {
                    Nome = command.Nome,
                    TemplateId = command.TemplateId,
                    ConteudoLivre = command.ConteudoLivre,
                    DataAgendamento = command.DataAgendamento,
                    EmpresaId = command.EmpresaId,
                    TotalContatos = command.ContatoIds?.Count ?? 0
                };

                var campanhaId = await _unitOfWork.Campanha.Incluir(campanha);

                var campanhaContatos = command.ContatoIds
                    .Select(contatoId => new Entidades.CampanhaContato
                    {
                        Id = Guid.NewGuid(),
                        CampanhaId = campanhaId,
                        ContatoId = contatoId,
                        Processado = false
                    })
                    .ToList();

                await _unitOfWork.Campanha.IncluirContatos(campanhaContatos);

                response.AddValue(new CriaCampanhaResult { Id = campanhaId });
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro ao criar campanha: {ex.Message}");
            }

            return response;
        }
    }
}
