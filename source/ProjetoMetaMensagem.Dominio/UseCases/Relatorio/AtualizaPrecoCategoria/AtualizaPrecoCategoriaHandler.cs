using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemPrecoCategoria;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.AtualizaPrecoCategoria
{
    public class AtualizaPrecoCategoriaHandler : IRequestHandler<AtualizaPrecoCategoriaCommand, Response<ObtemPrecoCategoriaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AtualizaPrecoCategoriaHandler> _logger;

        public AtualizaPrecoCategoriaHandler(IUnitOfWork unitOfWork, ILogger<AtualizaPrecoCategoriaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ObtemPrecoCategoriaResult>> Handle(AtualizaPrecoCategoriaCommand command)
        {
            var response = new Response<ObtemPrecoCategoriaResult>();

            var validator = new AtualizaPrecoCategoriaValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            if (!command.SolicitanteEhAdmin)
            {
                response.AddErro("Apenas administradores podem alterar o preço por categoria.");
                return response;
            }

            try
            {
                await _unitOfWork.Relatorio.AtualizarPrecoCategoria(command.Categoria, command.PrecoUnitario);
                var precos = await _unitOfWork.Relatorio.ListarPrecosCategoria();
                response.AddValue(new ObtemPrecoCategoriaResult { Precos = precos });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AtualizaPrecoCategoriaHandler));
            }

            return response;
        }
    }
}
