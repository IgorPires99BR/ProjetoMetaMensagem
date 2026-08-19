using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Flows.CriaFlow
{
    public class CriaFlowHandler : IRequestHandler<CriaFlowCommand, Response<CriaFlowResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CriaFlowHandler> _logger;

        public CriaFlowHandler(IUnitOfWork unitOfWork, ILogger<CriaFlowHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<CriaFlowResult>> Handle(CriaFlowCommand command)
        {
            var response = new Response<CriaFlowResult>();

            var validator = new CriaFlowValidator();
            var validateResult = validator.Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            Flow flow = new Flow(command);
            var etapasParaSalvar = new List<FlowEtapa>();
            FlowEtapa etapaAnterior = null;

            // Ordena os passos vindos da tela para garantir a lógica sequencial
            var passosOrdenados = command.Etapas.OrderBy(e => e.Ordem).ToList();

            for (int i = 0; i < passosOrdenados.Count; i++)
            {
                var dto = passosOrdenados[i];

                var novaEtapa = new FlowEtapa
                {
                    // Sem isso, toda etapa nascia com Guid.Empty -- a segunda etapa de qualquer
                    // flow violava a PK ao tentar salvar (mesmo Id que a primeira).
                    Id = Guid.NewGuid(),
                    FlowId = flow.Id,
                    NomeEtapa = dto.TipoStep, // "Mensagem" ou "Capturar Input"
                    ConteudoLivre = dto.MensagemPergunta, // Texto digitado na caixa
                    EhEtapaInicial = (i == 0), // O primeiro item da lista da tela vira a etapa inicial
                    GatilhoResposta = dto.TipoStep == "Capturar Input" ? "Qualquer_Resposta" : "Avancar",
                    // A tela sempre pediu a variavel de saida e o valor chegava aqui, mas nao era
                    // gravado -- entao nenhuma etapa de captura guardava a resposta do cliente.
                    VariavelSaida = dto.VariavelSaida,
                    TemplateId = dto.TemplateId,
                    Botao1 = dto.Botao1,
                    Botao2 = dto.Botao2
                };

                // Se houver uma etapa anterior no loop, atualizamos o ponteiro dela para esta nova
                if (etapaAnterior != null)
                {
                    etapaAnterior.ProximaEtapaId = novaEtapa.Id;
                }

                etapasParaSalvar.Add(novaEtapa);
                etapaAnterior = novaEtapa; // Atualiza a referência para o próximo loop
            }

            // Resolve a ramificacao depois de todas as etapas existirem: OrdemDestinoB aponta
            // pra uma etapa que vem DEPOIS na lista, entao o Id dela so existe agora.
            for (int i = 0; i < passosOrdenados.Count; i++)
            {
                var destinoB = passosOrdenados[i].OrdemDestinoB;
                if (destinoB == null) continue;

                var indiceDestino = passosOrdenados.FindIndex(e => e.Ordem == destinoB.Value);
                if (indiceDestino < 0)
                {
                    response.AddErro($"A etapa {passosOrdenados[i].Ordem} aponta o segundo caminho para a etapa {destinoB.Value}, que não existe neste fluxo.");
                    return response;
                }

                etapasParaSalvar[i].ProximaEtapaIdB = etapasParaSalvar[indiceDestino].Id;
            }

            try
            {
                _unitOfWork.BeginTransaction();
                // 3. Salva o Flow Pai
                await _unitOfWork.Flow.Incluir(flow);

                // 4. Salva os filhos (Etapas) já encadeadas -- de tras pra frente, porque
                // ProximaEtapaId e uma FK auto-referenciada: a etapa N aponta pra etapa N+1,
                // entao a etapa N+1 precisa existir no banco antes da etapa N ser inserida.
                for (int i = etapasParaSalvar.Count - 1; i >= 0; i--)
                {
                    await _unitOfWork.Flow.IncluirEtapa(etapasParaSalvar[i]);
                }

                // 7. Retorno de sucesso
                var result = new CriaFlowResult();

                response.AddValue(result);
                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                // Caso ocorra qualquer erro no processo do banco ou da Meta, a transação sofre rollback
                // e o banco não fica com dados fragmentados.
                response.AddErroServico(ex, _logger, nameof(CriaFlowHandler));
            }

            return response;
        }
    }
}


