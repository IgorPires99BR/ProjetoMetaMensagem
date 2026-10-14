using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Helpers;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemDadosBancarios;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaSubcontaAsaas
{
    public class CriaSubcontaAsaasHandler : IRequestHandler<CriaSubcontaAsaasCommand, Response<CriaSubcontaAsaasResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPixService _pix;
        private readonly ICriptografiaService _criptografia;
        private readonly ILogger<CriaSubcontaAsaasHandler> _logger;

        public CriaSubcontaAsaasHandler(IUnitOfWork unitOfWork, IPixService pix, ICriptografiaService criptografia,
            ILogger<CriaSubcontaAsaasHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _pix = pix;
            _criptografia = criptografia;
            _logger = logger;
        }

        public async Task<Response<CriaSubcontaAsaasResult>> Handle(CriaSubcontaAsaasCommand command)
        {
            var response = new Response<CriaSubcontaAsaasResult>();

            try
            {
                Normalizar(command);

                var validateResult = new CriaSubcontaAsaasValidator().Validate(command);
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

                // Cada criacao abre uma conta nova no Asaas (com possivel tarifa); trocar de
                // subconta e decisao do suporte, direto no banco.
                if (dados.TemSubconta)
                {
                    response.AddErro("Esta empresa já tem uma subconta Asaas. Para trocar, fale com o suporte da Contact Solution.");
                    return response;
                }

                // A apiKey so vem nesta resposta: sem a chave de criptografia nao ha como guarda-la,
                // e criar a conta mesmo assim perderia o acesso a ela para sempre.
                if (!_criptografia.Configurada)
                {
                    response.AddErro("A criptografia do servidor não está configurada (Criptografia:Chave). Avise o suporte antes de criar a subconta.");
                    return response;
                }

                SubcontaCriada subconta;
                try
                {
                    subconta = await _pix.CriarSubcontaAsync(new NovaSubconta(
                        command.Nome!, command.Email!, command.CpfCnpj!,
                        DocumentoHelper.EhCpf(command.CpfCnpj) ? command.DataNascimento : null,
                        DocumentoHelper.EhCnpj(command.CpfCnpj) ? command.TipoEmpresa : null,
                        command.Celular!, command.FaturamentoMensal!.Value, command.Endereco!, command.Numero!,
                        command.Complemento, command.Bairro!, command.Cep!));
                }
                catch (PixException ex)
                {
                    // A mensagem do PixException ja diz que foi o Asaas e em qual operacao.
                    response.AddErro(ex.Message);
                    return response;
                }

                dados.AsaasContaId = subconta.Id;
                dados.AsaasWalletId = subconta.WalletId;
                dados.AsaasApiKeyCriptografada = string.IsNullOrWhiteSpace(subconta.ApiKey) ? null : _criptografia.Criptografar(subconta.ApiKey);
                dados.AsaasContaCriadaEm = DateTime.Now;

                _unitOfWork.BeginTransaction();
                try
                {
                    await _unitOfWork.DadosBancariosEmpresa.Salvar(dados);
                }
                catch (Exception ex)
                {
                    _unitOfWork.Rollback();
                    // A conta ja existe no Asaas: sem esses ids no log, so daria para religar
                    // pelo painel do Asaas.
                    _logger.LogError(ex, "AsaasPix: subconta {ContaId} (wallet {WalletId}) criada para a empresa {EmpresaId}, mas nao foi gravada no banco",
                        subconta.Id, subconta.WalletId, command.EmpresaId);
                    throw;
                }
                _unitOfWork.Commit();

                dados.DataAtualizacao = DateTime.Now;
                response.AddValue(new CriaSubcontaAsaasResult
                {
                    DadosBancarios = new ObtemDadosBancariosResult(command.EmpresaId, dados)
                });
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(CriaSubcontaAsaasHandler));
            }

            return response;
        }

        private static void Normalizar(CriaSubcontaAsaasCommand command)
        {
            static string? Limpo(string? valor) => string.IsNullOrWhiteSpace(valor) ? null : valor.Trim();

            command.Nome = Limpo(command.Nome);
            command.Email = Limpo(command.Email)?.ToLowerInvariant();
            command.CpfCnpj = DocumentoHelper.SoDigitos(command.CpfCnpj);
            command.TipoEmpresa = Limpo(command.TipoEmpresa)?.ToUpperInvariant();
            command.Endereco = Limpo(command.Endereco);
            command.Numero = Limpo(command.Numero);
            command.Complemento = Limpo(command.Complemento);
            command.Bairro = Limpo(command.Bairro);
            command.Cep = DocumentoHelper.SoDigitos(command.Cep);

            // Quem cola "+55 (11) 99999-8888" quer dizer 11999998888.
            var celular = DocumentoHelper.SoDigitos(command.Celular);
            if (celular != null && celular.Length is 12 or 13 && celular.StartsWith("55"))
                celular = celular[2..];
            command.Celular = celular;
        }
    }
}
