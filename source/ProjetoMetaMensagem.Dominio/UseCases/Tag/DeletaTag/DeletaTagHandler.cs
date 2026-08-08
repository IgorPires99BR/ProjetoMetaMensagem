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

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.DeletaTag
{
    public class DeletaTagHandler : IRequestHandler<DeletaTagCommand, Response<DeletaTagResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<DeletaTagHandler> _logger;

        public DeletaTagHandler(IUnitOfWork unitOfWork, ILogger<DeletaTagHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<DeletaTagResult>> Handle(DeletaTagCommand command)
        {
            var response = new Response<DeletaTagResult>();

            try
            {
                _unitOfWork.BeginTransaction();
                var linhasAfetadas = await _unitOfWork.Tag.Excluir(command.Id, command.EmpresaIdSolicitante);

                // Zero linhas significa que a tag nao existe OU pertence a outra empresa.
                // As duas situacoes devolvem a mesma mensagem de proposito: dizer "existe, mas
                // nao e sua" ja entregaria ao atacante que aquele id e valido.
                if (linhasAfetadas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Tag não encontrada.");
                    return response;
                }

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                    _unitOfWork.Rollback();
                
                response.AddErro(TratamentoErro.Tratar(ex, _logger, nameof(DeletaTagHandler)));
            }

            return response;
        }
    }
}


