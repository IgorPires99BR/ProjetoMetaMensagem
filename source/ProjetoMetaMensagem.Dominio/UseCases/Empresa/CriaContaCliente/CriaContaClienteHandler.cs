using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.CriaContaCliente
{
    // Ate agora a conta de um cliente so nascia inteira por um caminho: o webhook de pagamento
    // da Cakto. Cadastrar um cliente que fechou por fora exigia criar a empresa numa tela, e o
    // usuario admin dela nao dava nem para criar pela interface -- a tela de Usuarios sempre
    // usa a empresa de quem esta logado, entao nao havia como apontar para a empresa recem
    // criada. Na pratica, sobrava mexer no banco a mao.
    //
    // Este caso de uso faz a conta inteira de uma vez, pela mesma sequencia do pagamento.
    public class CriaContaClienteHandler : IRequestHandler<CriaContaClienteCommand, Response<CriaContaClienteResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICriacaoDeContaDeCliente _criacaoDeConta;
        private readonly ILogger<CriaContaClienteHandler> _logger;

        public CriaContaClienteHandler(
            IUnitOfWork unitOfWork,
            ICriacaoDeContaDeCliente criacaoDeConta,
            ILogger<CriaContaClienteHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _criacaoDeConta = criacaoDeConta;
            _logger = logger;
        }

        public async Task<Response<CriaContaClienteResult>> Handle(CriaContaClienteCommand command)
        {
            var response = new Response<CriaContaClienteResult>();

            // Criar conta de cliente cria um tenant novo. Um admin de empresa cliente nao pode
            // fazer isso -- so a conta de operacao da propria Contact Solution.
            if (!command.SolicitanteEhAdminDaPlataforma)
            {
                response.AddErro("Apenas a conta de operação da Contact Solution pode cadastrar clientes.");
                return response;
            }

            var validateResult = new CriaContaClienteValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            var email = command.Email.Trim();

            try
            {
                // O login busca usuario por e-mail com QueryFirstOrDefault: dois usuarios com o
                // mesmo e-mail tornam o login nao-deterministico e o cliente cai numa das duas
                // contas sem criterio. Barrar aqui e mais claro do que descobrir depois.
                var jaExiste = await _unitOfWork.Usuario.ObterPorEmail(email);
                if (jaExiste != null)
                {
                    response.AddErro("Já existe uma conta com esse e-mail. Use outro e-mail ou abra a conta existente.");
                    return response;
                }

                var dados = new DadosDaContaDeCliente
                {
                    Nome = command.Nome.Trim(),
                    Email = email,
                    Telefone = command.Telefone,
                    Cnpj = command.Cnpj,
                    Plano = command.Plano,
                    PagamentoJaConfirmado = command.PagamentoJaConfirmado
                };

                // Dar acesso a uma empresa existente so vale enquanto ela nao tem ninguem. Se
                // ja tem, criar "o dono" de novo produziria dois admins disputando a mesma
                // conta -- para adicionar gente ao time existe a tela de Usuarios.
                if (command.EmpresaId.HasValue)
                {
                    var empresa = await _unitOfWork.Empresa.ObterPorId(command.EmpresaId.Value);
                    if (empresa == null)
                    {
                        response.AddErro("Empresa não encontrada.");
                        return response;
                    }

                    var jaTemUsuario = await _unitOfWork.Usuario.ObterPorEmpresa(command.EmpresaId.Value);
                    if (jaTemUsuario.Any())
                    {
                        response.AddErro("Esta empresa já tem acesso criado. Para adicionar mais pessoas, use a tela de Usuários.");
                        return response;
                    }
                }

                _unitOfWork.BeginTransaction();

                var conta = command.EmpresaId.HasValue
                    ? await _criacaoDeConta.CriarAcessoParaEmpresaExistenteAsync(command.EmpresaId.Value, dados)
                    : await _criacaoDeConta.CriarAsync(dados);

                _unitOfWork.Commit();

                _logger.LogInformation("Conta de cliente criada internamente para {Email} (empresa {EmpresaId})", email, conta.EmpresaId);

                response.AddValue(new CriaContaClienteResult
                {
                    EmpresaId = conta.EmpresaId,
                    Email = email,
                    SenhaProvisoria = conta.SenhaProvisoria,
                    Mensagem = "Conta criada. Anote a senha provisória: ela não aparece de novo."
                });
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(CriaContaClienteHandler));
            }

            return response;
        }
    }
}
