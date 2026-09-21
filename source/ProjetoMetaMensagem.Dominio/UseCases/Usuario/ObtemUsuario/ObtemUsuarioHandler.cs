using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario
{
    public class ObtemUsuarioHandler : IRequestHandler<ObtemUsuarioCommand, Response<List<ObtemUsuarioResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ObtemUsuarioHandler> _logger;

        public ObtemUsuarioHandler(IUnitOfWork unitOfWork, ILogger<ObtemUsuarioHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ObtemUsuarioResult>>> Handle(ObtemUsuarioCommand command)
        {
            var response = new Response<List<ObtemUsuarioResult>>();

            try
            {
                var validator = new ObtemUsuarioValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                if (command.IdEmpresa.HasValue)
                {
                    var usuariosDaEmpresa = await _unitOfWork.Usuario.ObterPorEmpresa(command.IdEmpresa.Value);
                    response.AddValue(usuariosDaEmpresa.Select(u => new ObtemUsuarioResult(u)).ToList());
                    return response;
                }

                // IdEmpresa nulo = todas as empresas (so a conta de plataforma chega aqui --
                // UsuariosController.ObterTodos garante isso antes de mandar o command).
                var todosOsUsuarios = (await _unitOfWork.Usuario.Obter()).ToList();
                var nomeDaEmpresa = (await _unitOfWork.Empresa.Obter()).ToDictionary(e => e.Id, e => e.Nome);

                var listaComEmpresa = todosOsUsuarios.Select(u =>
                {
                    var resultado = new ObtemUsuarioResult(u);
                    resultado.NomeEmpresa = nomeDaEmpresa.TryGetValue(u.EmpresaId, out var nome) ? nome : null;
                    return resultado;
                }).ToList();

                response.AddValue(listaComEmpresa);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemUsuarioHandler));
            }

            return response;
        }
    }
}
