using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Campanha.CriaCampanha
{
    public class CriaCampanhaHandler : IRequestHandler<CriaCampanhaCommand, Response<CriaCampanhaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<CriaCampanhaHandler> _logger;

        public CriaCampanhaHandler(IUnitOfWork unitOfWork, ILogger<CriaCampanhaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaCampanhaResult>> Handle(CriaCampanhaCommand command)
        {
            var response = new Response<CriaCampanhaResult>();

            var validator = new CriaCampanhaValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                // ContatoIds vem do corpo da requisicao: sem conferir que cada um pertence a
                // EmpresaId, um usuario autenticado de uma empresa conseguia mandar mensagem de
                // verdade (via CampanhaWorker) pra contatos de OUTRA empresa, so sabendo os ids.
                var contatosValidos = await _unitOfWork.Contato.ObterPorIds(command.EmpresaId, command.ContatoIds ?? new List<Guid>());
                if (contatosValidos.Count() != (command.ContatoIds?.Count ?? 0))
                {
                    response.AddErro("Um ou mais contatos informados não pertencem a esta empresa.");
                    return response;
                }

                _unitOfWork.BeginTransaction();
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
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(CriaCampanhaHandler));
            }

            return response;
        }
    }
}


