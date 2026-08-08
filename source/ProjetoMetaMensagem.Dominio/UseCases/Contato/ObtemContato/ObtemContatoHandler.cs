using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato
{
    public class ObtemContatoHandler : IRequestHandler<ObtemContatoCommand, Response<List<ObtemContatoResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ObtemContatoHandler> _logger;

        public ObtemContatoHandler(IUnitOfWork unitOfWork, ILogger<ObtemContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ObtemContatoResult>>> Handle(ObtemContatoCommand command)
        {
            var response = new Response<List<ObtemContatoResult>>();

            try
            {
                var listaContato = new List<ObtemContatoResult>();

                var validator = new ObtemContatoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var contatos = await _unitOfWork.Contato.ObterPorUsuario(command.IdEmpresa);

                foreach (var contato in contatos)
                {
                    listaContato.Add(new ObtemContatoResult(contato));
                }

                response.AddValue(listaContato);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemContatoHandler));
            }

            return response;
        }
    }
}
