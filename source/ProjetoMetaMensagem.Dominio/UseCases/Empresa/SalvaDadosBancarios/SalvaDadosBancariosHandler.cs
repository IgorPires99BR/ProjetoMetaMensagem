using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.SalvaDadosBancarios
{
    public class SalvaDadosBancariosHandler : IRequestHandler<SalvaDadosBancariosCommand, Response<SalvaDadosBancariosResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICriptografiaService _criptografia;
        private readonly IItauPixService _itau;
        private readonly ILogger<SalvaDadosBancariosHandler> _logger;

        public SalvaDadosBancariosHandler(IUnitOfWork unitOfWork, ICriptografiaService criptografia, IItauPixService itau, ILogger<SalvaDadosBancariosHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _criptografia = criptografia;
            _itau = itau;
            _logger = logger;
        }

        public async Task<Response<SalvaDadosBancariosResult>> Handle(SalvaDadosBancariosCommand command)
        {
            var response = new Response<SalvaDadosBancariosResult>();

            try
            {
                Normalizar(command);

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

                var trocaSecret = !string.IsNullOrWhiteSpace(command.ClientSecret);
                var trocaCertificado = !string.IsNullOrWhiteSpace(command.CertificadoPem);

                if ((trocaSecret || trocaCertificado) && !_criptografia.Configurada)
                {
                    // Gravar o segredo em texto puro "so por enquanto" e exatamente o que nao pode
                    // acontecer: melhor recusar e deixar claro o que falta no servidor.
                    response.AddErro("O servidor ainda não está configurado para guardar credenciais bancárias (Criptografia:Chave). Avise o suporte.", 500);
                    return response;
                }

                var dados = await _unitOfWork.DadosBancariosEmpresa.ObterPorEmpresa(command.EmpresaId)
                            ?? new DadosBancariosEmpresa { EmpresaId = command.EmpresaId };

                dados.Banco = command.Banco;
                dados.Agencia = command.Agencia;
                dados.Conta = command.Conta;
                dados.ContaDigito = command.ContaDigito;
                dados.TitularNome = command.TitularNome;
                dados.TitularDocumento = command.TitularDocumento;
                dados.TipoChavePix = command.TipoChavePix;
                dados.ChavePix = command.ChavePix;
                dados.Ambiente = command.Ambiente;
                dados.ClientId = command.ClientId;
                dados.CobrancaPixAtiva = command.CobrancaPixAtiva;

                if (trocaSecret)
                {
                    dados.ClientSecretCriptografado = _criptografia.Criptografar(command.ClientSecret!);
                }

                if (trocaCertificado)
                {
                    var erroCertificado = AplicarCertificado(dados, command.CertificadoPem!, command.ChavePrivadaPem!);
                    if (erroCertificado != null)
                    {
                        response.AddErro(erroCertificado);
                        return response;
                    }
                }

                if (dados.CobrancaPixAtiva)
                {
                    if (string.IsNullOrWhiteSpace(dados.ClientSecretCriptografado))
                    {
                        response.AddErro("Informe o Client Secret do Itaú para ativar a cobrança via Pix.");
                        return response;
                    }

                    // So producao usa mTLS; o sandbox do Itau autentica so com client id/secret.
                    if (dados.Ambiente == DadosBancariosEmpresa.AmbienteProducao
                        && (string.IsNullOrWhiteSpace(dados.CertificadoPem) || string.IsNullOrWhiteSpace(dados.ChavePrivadaCriptografada)))
                    {
                        response.AddErro("Envie o certificado e a chave privada do Itaú para ativar a cobrança via Pix.");
                        return response;
                    }

                    if (dados.CertificadoValidoAte.HasValue && dados.CertificadoValidoAte.Value < DateTime.Now)
                    {
                        response.AddErro("O certificado do Itaú está vencido. Gere um novo no portal do Itaú e envie aqui antes de ativar a cobrança.");
                        return response;
                    }
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
                    await ConferirComItau(dados, resultado);

                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(SalvaDadosBancariosHandler));
            }

            return response;
        }

        // Credencial errada so apareceria no primeiro disparo, com cada cliente da lista falhando.
        // O webhook e cadastrado de novo a cada ativacao porque a chave Pix pode ter mudado.
        private async Task ConferirComItau(DadosBancariosEmpresa dados, SalvaDadosBancariosResult resultado)
        {
            try
            {
                await _itau.TestarCredenciaisAsync(dados);
                resultado.CredenciaisConferidas = true;
            }
            catch (Exception ex)
            {
                resultado.AvisoIntegracao = $"Dados salvos, mas a conexão com o Itaú falhou: {MensagemDe(ex)}";
                return;
            }

            try
            {
                await _itau.ConfigurarWebhookAsync(dados, _itau.UrlWebhook(dados.EmpresaId));
                resultado.WebhookCadastrado = true;
            }
            catch (Exception ex)
            {
                // Sem webhook a baixa continua acontecendo pela consulta periodica, so mais devagar.
                resultado.AvisoIntegracao = $"Credenciais conferidas, mas o webhook não foi cadastrado ({MensagemDe(ex)}). Os pagamentos serão confirmados pela consulta periódica, em até alguns minutos.";
            }
        }

        // Os dados ja foram gravados: qualquer falha aqui vira aviso, nunca erro do salvamento.
        private string MensagemDe(Exception ex)
        {
            if (ex is ItauPixException) return ex.Message;
            _logger.LogWarning(ex, "ItauPix: erro inesperado ao conferir credenciais");
            return "erro inesperado ao falar com o Itaú";
        }

        // Confere que a chave e mesmo a do certificado ANTES de gravar: um par trocado so
        // apareceria na primeira cobranca, como falha de TLS sem explicacao.
        private string? AplicarCertificado(DadosBancariosEmpresa dados, string certificadoPem, string chavePrivadaPem)
        {
            if (chavePrivadaPem.Contains("ENCRYPTED PRIVATE KEY", StringComparison.Ordinal))
                return "A chave privada está protegida por senha. Envie a chave sem senha (arquivo .key gerado junto com o pedido do certificado).";

            X509Certificate2 certificado;
            try
            {
                certificado = X509Certificate2.CreateFromPem(certificadoPem, chavePrivadaPem);
            }
            catch (CryptographicException)
            {
                return "Certificado ou chave privada inválidos, ou a chave não corresponde ao certificado. Confira se enviou o .crt e o .key do mesmo par.";
            }
            catch (ArgumentException)
            {
                return "Certificado ou chave privada inválidos. Envie os arquivos no formato PEM (.crt e .key).";
            }

            using (certificado)
            {
                if (!certificado.HasPrivateKey)
                    return "A chave privada não corresponde ao certificado.";

                if (certificado.NotAfter < DateTime.Now)
                    return $"O certificado venceu em {certificado.NotAfter:dd/MM/yyyy}. Gere um novo no portal do Itaú.";

                dados.CertificadoPem = certificadoPem;
                dados.ChavePrivadaCriptografada = _criptografia.Criptografar(chavePrivadaPem);
                dados.CertificadoValidoAte = certificado.NotAfter;
                dados.CertificadoTitular = certificado.Subject.Length > 500 ? certificado.Subject[..500] : certificado.Subject;
            }

            return null;
        }

        private static void Normalizar(SalvaDadosBancariosCommand command)
        {
            static string? Limpo(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();
            static string? SoDigitos(string? valor) =>
                string.IsNullOrWhiteSpace(valor) ? null : new string(Array.FindAll(valor.ToCharArray(), char.IsDigit));

            command.Banco = Limpo(command.Banco) ?? DadosBancariosEmpresa.CodigoItau;
            command.Agencia = SoDigitos(command.Agencia);
            command.Conta = SoDigitos(command.Conta);
            command.ContaDigito = Limpo(command.ContaDigito)?.ToUpperInvariant();
            command.TitularNome = Limpo(command.TitularNome);
            command.TitularDocumento = SoDigitos(command.TitularDocumento);
            command.TipoChavePix = Limpo(command.TipoChavePix)?.ToUpperInvariant();
            command.Ambiente = Limpo(command.Ambiente)?.ToUpperInvariant() ?? DadosBancariosEmpresa.AmbienteSandbox;
            command.ClientId = Limpo(command.ClientId);
            command.ClientSecret = Limpo(command.ClientSecret);
            command.CertificadoPem = Limpo(command.CertificadoPem);
            command.ChavePrivadaPem = Limpo(command.ChavePrivadaPem);

            // Chave Pix e comparada pelo banco exatamente como esta no DICT: documento sem
            // mascara, e-mail em minusculas, aleatoria em minusculas.
            var chave = Limpo(command.ChavePix);
            command.ChavePix = command.TipoChavePix switch
            {
                "CNPJ" or "CPF" => SoDigitos(chave),
                "EMAIL" or "ALEATORIA" => chave?.ToLowerInvariant(),
                "TELEFONE" => chave?.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", ""),
                _ => chave
            };
        }
    }
}
