using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraPerfilNumero
{
    public class AlteraPerfilNumeroHandler : IRequestHandler<AlteraPerfilNumeroCommand, Response<AlteraPerfilNumeroResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;
        private readonly ILogger<AlteraPerfilNumeroHandler> _logger;

        public AlteraPerfilNumeroHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<AlteraPerfilNumeroHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }

        public async Task<Response<AlteraPerfilNumeroResult>> Handle(AlteraPerfilNumeroCommand command)
        {
            var response = new Response<AlteraPerfilNumeroResult>();

            var validateResult = new AlteraPerfilNumeroValidator().Validate(command);
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

                var token = await NumeroMetaAcesso.ObterTokenAsync(_unitOfWork, numero);

                // Texto vazio vai como "" (limpa o campo na Meta); só o segmento vazio fica de fora,
                // porque a Meta não aceita vertical em branco.
                await _metaService.AtualizarPerfilNumeroAsync(numero.InstanciaId!, token!, new PerfilNumeroEnvio
                {
                    Sobre = command.Sobre?.Trim() ?? string.Empty,
                    Descricao = command.Descricao?.Trim() ?? string.Empty,
                    Endereco = command.Endereco?.Trim() ?? string.Empty,
                    Email = command.Email?.Trim() ?? string.Empty,
                    Sites = (command.Sites ?? new List<string>()).Where(s => !string.IsNullOrWhiteSpace(s)).Select(s => s.Trim()).ToList(),
                    Segmento = string.IsNullOrEmpty(command.Segmento) ? null : command.Segmento
                });

                response.AddValue(new AlteraPerfilNumeroResult { NumeroId = numero.Id });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AlteraPerfilNumeroHandler));
            }

            return response;
        }
    }
}
