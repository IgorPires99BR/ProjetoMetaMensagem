using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AtualizaNumeroMeta
{
    public class AtualizaNumeroMetaHandler : IRequestHandler<AtualizaNumeroMetaCommand, Response<List<AtualizaNumeroMetaResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        private readonly ILogger<AtualizaNumeroMetaHandler> _logger;

        public AtualizaNumeroMetaHandler(IUnitOfWork unitOfWork, IMetaService metaService, ILogger<AtualizaNumeroMetaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
            _logger = logger;
        }


        public async Task<Response<List<AtualizaNumeroMetaResult>>> Handle(AtualizaNumeroMetaCommand command)
        {
            var response = new Response<List<AtualizaNumeroMetaResult>>();

            try
            {
                var validator = new AtualizaNumeroMetaValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var wabaId = await _unitOfWork.Empresa.ObterWabaId(command.IdEmpresa);
                var token = await _unitOfWork.Empresa.ObterMetaAccessToken(command.IdEmpresa);

                var numerosMeta = await _metaService.ObterNumerosMetaAsync(wabaId, token);

                if (numerosMeta == null || !numerosMeta.Any())
                {
                    response.AddErro("Nenhum número encontrado na API da Meta.");
                    return response;
                }

                // E por WABA, nao por numero -- chamar uma vez aqui (nao dentro do foreach de
                // numeros abaixo). Tambem serve de retry pro caso da assinatura ter falhado no
                // Embedded Signup original: o usuario clicando em "Sincronizar Meta" tenta de novo.
                var appAssinado = await _metaService.AssinarAppNoWabaAsync(wabaId, token);
                if (!appAssinado)
                {
                    _logger.LogWarning("Falha ao assinar o app no WABA {WabaId} durante sincronizacao manual. Empresa {IdEmpresa}", wabaId, command.IdEmpresa);
                }

                var numerosNoBanco = await _unitOfWork.Numero
                    .ObterPorUsuario(command.IdUsuario);

                var idsVindosDaMeta = numerosMeta.Select(n => n.Id).ToList();
                var numerosParaRemover = numerosNoBanco
                    .Where(b => !idsVindosDaMeta.Contains(b.InstanciaId))
                    .ToList();

                foreach (var numeroExcluir in numerosParaRemover)
                {
                    // Sincronizacao sempre restrita a empresa do comando, mesmo que a lista de
                    // numeros ja tenha vindo do usuario dela: assim um id de outra empresa que
                    // escape pra ca nao chega a ser apagado.
                    await _unitOfWork.Numero.Excluir(numeroExcluir.Id, command.IdEmpresa);
                }
                foreach (var numeroApi in numerosMeta)
                {
                    var numeroExistente = numerosNoBanco.FirstOrDefault(x => x.InstanciaId == numeroApi.Id);

                    if (numeroExistente != null)
                    {
                        // --- ATUALIZAÇÃO ---
                        numeroExistente.Telefone = numeroApi.NumeroFormatado;
                        numeroExistente.StatusMeta = numeroApi.Status;
                        numeroExistente.QualidadeMeta = numeroApi.Qualidade;
                        numeroExistente.Descricao = string.IsNullOrEmpty(numeroExistente.Descricao)
                            ? numeroApi.NomeVerificado
                            : numeroExistente.Descricao;
                        numeroExistente.DataAtualizacao = DateTime.Now;
                        numeroExistente.DataUltimaSincronizacao = DateTime.Now;

                        await _unitOfWork.Numero.Alterar(numeroExistente, command.IdEmpresa);
                    }
                    else
                    {
                        // --- INSERÇÃO ---
                        var novoNumero = new Entidades.Numero
                        {
                            Id = Guid.NewGuid(),
                            UsuarioId = command.IdUsuario,
                            Telefone = numeroApi.NumeroFormatado,
                            InstanciaId = numeroApi.Id,
                            StatusMeta = numeroApi.Status,
                            QualidadeMeta = numeroApi.Qualidade,
                            Descricao = numeroApi.NomeVerificado,
                            DataCriacao = DateTime.Now
                        };

                        await _unitOfWork.Numero.Incluir(novoNumero);
                    }
                }
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(AtualizaNumeroMetaHandler));
            }

            return response;
        }
    }
}
