using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnfileiraDisparoLote
{
    // So valida e grava: nenhum Pix nem mensagem sai daqui, e por isso o request nao tem mais como
    // estourar o tempo limite no meio de um envio. Quem processa e o DisparoLoteWorker.
    public class EnfileiraDisparoLoteHandler : IRequestHandler<EnfileiraDisparoLoteCommand, Response<EnfileiraDisparoLoteResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EnfileiraDisparoLoteHandler> _logger;

        public EnfileiraDisparoLoteHandler(IUnitOfWork unitOfWork, ILogger<EnfileiraDisparoLoteHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<EnfileiraDisparoLoteResult>> Handle(EnfileiraDisparoLoteCommand command)
        {
            var response = new Response<EnfileiraDisparoLoteResult>();

            if (command.EmpresaIdSolicitante.HasValue)
                command.IdEmpresa = command.EmpresaIdSolicitante.Value;

            var validateResult = new EnfileiraDisparoLoteValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var existente = await _unitOfWork.DisparoLote.ObterPorChave(command.IdEmpresa, command.ChaveIdempotencia);
                if (existente != null)
                {
                    response.AddValue(new EnfileiraDisparoLoteResult { LoteId = existente.Id });
                    return response;
                }

                var erro = await ValidarAsync(command);
                if (erro != null)
                {
                    response.AddErro(erro);
                    return response;
                }

                var lote = new Entidades.DisparoLote
                {
                    EmpresaId = command.IdEmpresa,
                    TemplateId = command.TemplateId,
                    ChaveIdempotencia = command.ChaveIdempotencia,
                    VariaveisJson = JsonConvert.SerializeObject(command.Variaveis ?? new()),
                    ParametroHeaderMediaUrl = string.IsNullOrWhiteSpace(command.ParametroHeaderMediaUrl) ? null : command.ParametroHeaderMediaUrl,
                    ParametrosButtonJson = JsonConvert.SerializeObject(command.ParametrosButton ?? new()),
                    DataCriacao = HorarioBrasilia.Agora()
                };

                var itens = command.ContatosIds.Distinct()
                    .Select((contatoId, ordem) => new DisparoLoteItem { LoteId = lote.Id, ContatoId = contatoId, Ordem = ordem })
                    .ToList();

                _unitOfWork.BeginTransaction();
                try
                {
                    await _unitOfWork.DisparoLote.Incluir(lote, itens);
                    _unitOfWork.Commit();
                }
                catch
                {
                    _unitOfWork.Rollback();
                    throw;
                }

                response.AddValue(new EnfileiraDisparoLoteResult { LoteId = lote.Id });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(EnfileiraDisparoLoteHandler));
            }

            return response;
        }

        // Tudo que da para recusar antes de aceitar o lote: depois daqui o erro vira falha por contato.
        private async Task<string?> ValidarAsync(EnfileiraDisparoLoteCommand command)
        {
            var template = await _unitOfWork.Template.ObterPorIdEEmpresa(command.TemplateId, command.IdEmpresa);
            if (template == null) return "Modelo de mensagem não encontrado.";
            if (!template.Ativo) return "Este modelo foi inativado. Reative em Templates para enviar.";
            if (template.Status == null || !template.Status.StartsWith("APPROVED", StringComparison.OrdinalIgnoreCase))
                return "Este modelo ainda não foi aprovado pela Meta.";

            var variaveis = command.Variaveis ?? new();
            var origemInvalida = variaveis.FirstOrDefault(v => !ResolvedorDeVariaveis.OrigemValida(v.Origem));
            if (origemInvalida != null) return $"Origem de variável inválida: {origemInvalida.Origem}.";

            if (variaveis.Any(v => v.Origem == ResolvedorDeVariaveis.ParametroCadastrado))
            {
                if (variaveis.Any(v => v.Origem == ResolvedorDeVariaveis.ParametroCadastrado && !v.ParametroId.HasValue))
                    return "Selecione o parâmetro de cada variável que usa um parâmetro cadastrado.";

                var parametros = (await _unitOfWork.Parametro.ObterPorEmpresa(command.IdEmpresa)).Select(p => p.Id).ToHashSet();
                if (variaveis.Any(v => v.Origem == ResolvedorDeVariaveis.ParametroCadastrado && !parametros.Contains(v.ParametroId!.Value)))
                    return "Um dos parâmetros escolhidos não existe mais. Atualize a tela e selecione novamente.";
            }

            var ids = command.ContatosIds.Distinct().ToList();
            var encontrados = new HashSet<Guid>();
            foreach (var lote in ids.Chunk(1000))
            {
                foreach (var contato in await _unitOfWork.Contato.ObterPorIds(command.IdEmpresa, lote))
                    encontrados.Add(contato.Id);
            }
            if (ids.Any(id => !encontrados.Contains(id)))
                return "Há contato selecionado que não pertence a esta empresa.";

            var naFila = (await _unitOfWork.DisparoLote.ObterContatosJaNaFila(command.TemplateId, ids)).Count();
            if (naFila > 0)
                return $"{naFila} contato(s) selecionado(s) já estão na fila com este modelo. Aguarde esse envio terminar ou desmarque-os.";

            return null;
        }
    }
}
