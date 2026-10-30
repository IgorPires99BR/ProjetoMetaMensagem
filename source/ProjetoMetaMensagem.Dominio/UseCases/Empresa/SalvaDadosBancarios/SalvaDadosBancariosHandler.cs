using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.SalvaDadosBancarios
{
    public class SalvaDadosBancariosHandler : IRequestHandler<SalvaDadosBancariosCommand, Response<SalvaDadosBancariosResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProvedoresCobranca _provedores;
        private readonly ILogger<SalvaDadosBancariosHandler> _logger;

        public SalvaDadosBancariosHandler(IUnitOfWork unitOfWork, IProvedoresCobranca provedores, ILogger<SalvaDadosBancariosHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _provedores = provedores;
            _logger = logger;
        }

        public async Task<Response<SalvaDadosBancariosResult>> Handle(SalvaDadosBancariosCommand command)
        {
            var response = new Response<SalvaDadosBancariosResult>();

            try
            {
                command.ComissaoTipo = string.IsNullOrWhiteSpace(command.ComissaoTipo) ? null : command.ComissaoTipo.Trim().ToUpperInvariant();

                var validateResult = new SalvaDadosBancariosValidator().Validate(command);
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

                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(command.EmpresaId)
                            ?? new DadosBancariosEmpresa { EmpresaId = command.EmpresaId };

                dados.CobrancaPixAtiva = command.CobrancaPixAtiva;

                if (command.SolicitanteEhAdminDaPlataforma)
                {
                    dados.ComissaoTipo = command.ComissaoTipo;
                    dados.ComissaoValor = command.ComissaoValor;
                    if (command.RecebimentoNaPlataforma.HasValue)
                        dados.RecebimentoNaPlataforma = command.RecebimentoNaPlataforma.Value;
                }

                // Sem split (empresa recebendo na conta da plataforma, ou provedor ativo sem split,
                // como o Mercado Pago) subconta e comissao nao entram.
                var exigeSubconta = dados.CobrancaPixAtiva && !dados.RecebimentoNaPlataforma && ProvedorAtivoFazSplit();

                if (exigeSubconta && !dados.TemSubconta)
                {
                    response.AddErro("Crie a subconta Asaas da empresa antes de ativar a cobrança via Pix: é nela que o valor das cobranças é depositado.");
                    return response;
                }

                if (exigeSubconta && !dados.ComissaoDefinida)
                {
                    response.AddErro(command.SolicitanteEhAdminDaPlataforma
                        ? "Defina a comissão da plataforma para ativar a cobrança via Pix."
                        : "A comissão da plataforma para esta empresa ainda não foi definida. Fale com o suporte da Contact Solution para ativar a cobrança via Pix.");
                    return response;
                }

                _unitOfWork.BeginTransaction();
                try
                {
                    await _unitOfWork.DadosBancariosEmpresa.Salvar(dados);
                }
                catch
                {
                    _unitOfWork.Rollback();
                    throw;
                }
                _unitOfWork.Commit();

                // Monta a resposta do que esta em memoria: depois do Commit a transacao da sessao
                // fica descartada e uma nova consulta com ela estouraria.
                dados.DataAtualizacao = DateTime.Now;
                var resultado = new SalvaDadosBancariosResult
                {
                    DadosBancarios = new ObtemDadosBancariosResult(command.EmpresaId, dados)
                };

                if (dados.CobrancaPixAtiva)
                    await ConferirComProvedor(resultado);

                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(SalvaDadosBancariosHandler));
            }

            return response;
        }

        // Provedor mal configurado: na duvida exige a subconta, o erro de configuracao aparece na
        // conferencia logo depois.
        private bool ProvedorAtivoFazSplit()
        {
            try { return _provedores.Ativo.SuportaSplit; }
            catch (PixException) { return true; }
        }

        // Credencial da plataforma errada so apareceria no primeiro disparo, com cada cliente
        // da lista falhando. Os dados ja foram gravados: falha aqui vira aviso, nunca erro.
        private async Task ConferirComProvedor(SalvaDadosBancariosResult resultado)
        {
            try
            {
                await _provedores.Ativo.TestarCredenciaisAsync();
                resultado.CredenciaisConferidas = true;
            }
            catch (PixException ex)
            {
                resultado.AvisoIntegracao = $"Dados salvos, mas a conexão com o provedor de pagamento falhou: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Pix: erro inesperado ao conferir credenciais do provedor ativo");
                resultado.AvisoIntegracao = "Dados salvos, mas a conexão com o provedor de pagamento falhou: erro inesperado.";
            }
        }
    }
}
