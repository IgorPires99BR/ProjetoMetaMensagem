using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.Common;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.SolicitaNomeNumero
{
    public class SolicitaNomeNumeroHandler : IRequestHandler<SolicitaNomeNumeroCommand, Response<SolicitaNomeNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly ILogger<SolicitaNomeNumeroHandler> _logger;

        public SolicitaNomeNumeroHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<SolicitaNomeNumeroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<SolicitaNomeNumeroResult>> Handle(SolicitaNomeNumeroCommand command)
        {
            var response = new Response<SolicitaNomeNumeroResult>();

            var validateResult = new SolicitaNomeNumeroValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var (numero, erro) = await NumeroMetaAcesso.CarregarAsync(_unitOfWork, command.NumeroId, command.EmpresaIdSolicitante);
                if (numero == null)
                {
                    response.AddErro(erro!);
                    return response;
                }

                if (NumeroMetaAcesso.EhCoexistencia(numero))
                {
                    response.AddErro(NumeroMetaAcesso.MensagemCoexistencia);
                    return response;
                }

                var token = await NumeroMetaAcesso.ObterTokenAsync(_unitOfWork, numero);
                var novoNome = command.NovoNome.Trim();

                await _metaService.SolicitarNovoNomeExibicaoAsync(numero.InstanciaId!, token!, novoNome);

                response.AddValue(new SolicitaNomeNumeroResult { NumeroId = numero.Id, NovoNome = novoNome });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(SolicitaNomeNumeroHandler));
            }

            return response;
        }
    }
}
