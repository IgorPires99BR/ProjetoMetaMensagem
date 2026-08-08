using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
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

        private readonly ILogger<AssociarTagsContatoHandler> _logger;

        public AssociarTagsContatoHandler(IUnitOfWork unitOfWork, ILogger<AssociarTagsContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<AssociarTagsContatoResult>> Handle(AssociarTagsContatoCommand command)
        {
            var response = new Response<AssociarTagsContatoResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                await _unitOfWork.Tag.AssociarTagsContato(
                    command.ContatoId, command.TagIds, command.EmpresaIdSolicitante);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(AssociarTagsContatoHandler));
            }

            return response;
        }
    }
}

