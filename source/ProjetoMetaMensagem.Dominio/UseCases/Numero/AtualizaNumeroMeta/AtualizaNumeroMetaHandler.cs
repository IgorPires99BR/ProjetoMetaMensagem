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

        public AtualizaNumeroMetaHandler(IUnitOfWork unitOfWork, IMetaService metaService)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
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

                if (numerosMeta == null || !numerosMeta.Numeros.Any())
                {
                    response.AddErro("Nenhum número encontrado na API da Meta.");
                    return response;
                }

                var numerosNoBanco = await _unitOfWork.Numero
                    .ObterPorUsuario(command.IdUsuario);

                var idsVindosDaMeta = numerosMeta.Numeros.Select(n => n.Id).ToList();
                var numerosParaRemover = numerosNoBanco
                    .Where(b => !idsVindosDaMeta.Contains(b.InstanciaId))
                    .ToList();

                foreach (var numeroExcluir in numerosParaRemover)
                {
                    await _unitOfWork.Numero.Excluir(numeroExcluir.Id);
                }
                foreach (var numeroApi in numerosMeta.Numeros)
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

                        await _unitOfWork.Numero.Alterar(numeroExistente);
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
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
