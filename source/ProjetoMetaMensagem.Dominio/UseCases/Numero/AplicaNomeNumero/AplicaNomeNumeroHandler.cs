using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.Common;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AplicaNomeNumero
{
    public class AplicaNomeNumeroHandler : IRequestHandler<AplicaNomeNumeroCommand, Response<AplicaNomeNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly ILogger<AplicaNomeNumeroHandler> _logger;

        public AplicaNomeNumeroHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<AplicaNomeNumeroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<AplicaNomeNumeroResult>> Handle(AplicaNomeNumeroCommand command)
        {
            var response = new Response<AplicaNomeNumeroResult>();

            var validateResult = new AplicaNomeNumeroValidator().Validate(command);
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

                // Registrar de novo antes da aprovação não tem efeito nenhum (regra da Meta) --
                // confere o status antes pra não gastar a chamada e explicar o motivo.
                var perfil = await _metaService.ObterPerfilNumeroAsync(numero.InstanciaId!, token!);
                if (!string.Equals(perfil.StatusNovoNome, "APPROVED", StringComparison.OrdinalIgnoreCase)
                    || string.IsNullOrEmpty(perfil.NovoNomeSolicitado))
                {
                    response.AddErro("Ainda não há um novo nome aprovado pela Meta para este número. Aguarde a análise e tente de novo.");
                    return response;
                }

                await _metaService.RegistrarNumeroAsync(numero.InstanciaId!, token!, command.Pin);

                // Descricao é o rótulo do número na lista e nasceu do nome verificado; acompanha a troca.
                numero.Descricao = perfil.NovoNomeSolicitado;
                numero.DataAtualizacao = DateTime.Now;
                await _unitOfWork.Numero.Alterar(numero, command.EmpresaIdSolicitante);

                response.AddValue(new AplicaNomeNumeroResult { NumeroId = numero.Id, NomeAplicado = perfil.NovoNomeSolicitado });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AplicaNomeNumeroHandler));
            }

            return response;
        }
    }
}
