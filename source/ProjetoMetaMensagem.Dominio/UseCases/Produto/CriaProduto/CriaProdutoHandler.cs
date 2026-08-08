using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Help.Error;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Produto.CriaProduto
{
    public class CriaProdutoHandler : IRequestHandler<CriaProdutoCommand, Response<CriaProdutoResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<CriaProdutoHandler> _logger;

        public CriaProdutoHandler(IUnitOfWork unitOfWork, ILogger<CriaProdutoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaProdutoResult>> Handle(CriaProdutoCommand command)
        {
            var response = new Response<CriaProdutoResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var validator = new CriaProdutoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var entity = new Entidades.Produto(command);
                var id = await _unitOfWork.Produto.Incluir(entity);

                response.AddValue(new CriaProdutoResult { Id = id });
                _unitOfWork.Commit();
            }
            catch (System.Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(CriaProdutoHandler)));
            }

            return response;
        }
    }
}


