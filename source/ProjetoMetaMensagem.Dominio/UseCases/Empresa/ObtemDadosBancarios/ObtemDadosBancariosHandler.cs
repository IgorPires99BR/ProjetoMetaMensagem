using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios
{
    public class ObtemDadosBancariosHandler : IRequestHandler<ObtemDadosBancariosCommand, Response<ObtemDadosBancariosResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProvedoresCobranca _provedores;
        private readonly ILogger<ObtemDadosBancariosHandler> _logger;

        public ObtemDadosBancariosHandler(IUnitOfWork unitOfWork, IProvedoresCobranca provedores, ILogger<ObtemDadosBancariosHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _provedores = provedores;
            _logger = logger;
        }

        public async Task<Response<ObtemDadosBancariosResult>> Handle(ObtemDadosBancariosCommand command)
        {
            var response = new Response<ObtemDadosBancariosResult>();

            try
            {
                var validateResult = new ObtemDadosBancariosValidator().Validate(command);
                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                if (await _unitOfWork.Empresa.ObterPorId(command.EmpresaId) == null)
                {
                    response.AddErro("Empresa não encontrada.", 404);
                    return response;
                }

                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(command.EmpresaId);
                var resultado = new ObtemDadosBancariosResult(command.EmpresaId, dados);
                try
                {
                    resultado.ProvedorAtivo = _provedores.Ativo.Nome;
                }
                catch (PixException)
                {
                    // Cobranca:Provedor invalido: a tela segue sem o nome e o erro aparece ao salvar.
                }
                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemDadosBancariosHandler));
            }

            return response;
        }
    }
}
