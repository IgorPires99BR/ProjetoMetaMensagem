using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.DeletaContato
{
    public class DeletaContatoHandler : IRequestHandler<DeletaContatoCommand, Response<DeletaContatoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DeletaContatoHandler> _logger;

        public DeletaContatoHandler(IUnitOfWork unitOfWork, ILogger<DeletaContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaContatoResult>> Handle(DeletaContatoCommand command)
        {
            var response = new Response<DeletaContatoResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new DeletaContatoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var resultado = await _unitOfWork.Contato.Excluir(command.Id, command.EmpresaIdSolicitante);

                // NaoEncontrado cobre o contato que nao existe, o de outra empresa e o ja inativo.
                // Todos devolvem a mesma mensagem de proposito: dizer "existe, mas nao e seu" ja
                // entregaria ao atacante que aquele id e valido.
                if (resultado == ResultadoExclusaoContato.NaoEncontrado)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Contato não encontrado.");
                    return response;
                }

                response.AddValue(new DeletaContatoResult { Inativado = resultado == ResultadoExclusaoContato.Inativado });
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErroServico(ex, _logger, nameof(DeletaContatoHandler));
            }

            return response;
        }
    }
}


