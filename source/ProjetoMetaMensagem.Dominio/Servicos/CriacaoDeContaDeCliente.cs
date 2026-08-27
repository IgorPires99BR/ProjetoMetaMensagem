using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Servicos
{
    // Nascer uma conta de cliente e sempre a mesma sequencia: empresa, usuario admin, senha
    // provisoria, e-mail de acesso e o cliente virando contato na nossa propria conta.
    //
    // Antes essa sequencia so existia dentro do handler do webhook da Cakto, o que amarrava
    // "criar conta" a "receber pagamento": para cadastrar um cliente que fechou por fora era
    // preciso mexer no banco a mao. Agora os dois caminhos -- o pagamento e a criacao interna
    // -- passam por aqui, entao uma conta criada pela equipe e igual a uma conta comprada.
    public class CriacaoDeContaDeCliente : ICriacaoDeContaDeCliente
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IOnboardingComercialService _onboarding;
        private readonly ILogger<CriacaoDeContaDeCliente> _logger;

        public CriacaoDeContaDeCliente(
            IUnitOfWork unitOfWork,
            IEmailService emailService,
            IOnboardingComercialService onboarding,
            ILogger<CriacaoDeContaDeCliente> logger)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _onboarding = onboarding;
            _logger = logger;
        }

        public async Task<ContaDeClienteCriada> CriarAsync(DadosDaContaDeCliente dados)
        {
            var nome = string.IsNullOrWhiteSpace(dados.Nome) ? dados.Email : dados.Nome;

            var empresa = new Empresa
            {
                Id = Guid.NewGuid(),
                Nome = nome,
                Email = dados.Email,
                Cnpj = dados.Cnpj ?? string.Empty,
                Telefone = dados.Telefone,
                StatusConta = "Ativo",
                PlanoId = dados.Plano,
                DataCriacao = DateTime.Now
            };

            var empresaId = await _unitOfWork.Empresa.Incluir(empresa);
            if (empresaId == Guid.Empty) empresaId = empresa.Id;

            return await CriarAcessoAsync(empresaId, nome, dados);
        }

        // Empresa que ja existe no banco mas nunca teve usuario -- cadastrada numa importacao,
        // num teste, ou antes de a integracao de pagamento existir. Sem usuario ela e peso
        // morto: ninguem entra nela e ela nao aparece para ninguem. Isto da o acesso a ela sem
        // criar uma empresa duplicada ao lado.
        public async Task<ContaDeClienteCriada> CriarAcessoParaEmpresaExistenteAsync(Guid empresaId, DadosDaContaDeCliente dados)
        {
            var nome = string.IsNullOrWhiteSpace(dados.Nome) ? dados.Email : dados.Nome;

            var empresa = await _unitOfWork.Empresa.ObterPorId(empresaId);
            if (empresa != null)
            {
                // A empresa manda no proprio cadastro: so preenche o que estava em branco, e
                // atualiza o plano quando quem cadastrou escolheu um.
                empresa.StatusConta = "Ativo";
                if (!string.IsNullOrWhiteSpace(dados.Plano)) empresa.PlanoId = dados.Plano;
                if (string.IsNullOrWhiteSpace(empresa.Email)) empresa.Email = dados.Email;
                if (string.IsNullOrWhiteSpace(empresa.Telefone)) empresa.Telefone = dados.Telefone;

                await _unitOfWork.Empresa.Alterar(empresa);
            }

            return await CriarAcessoAsync(empresaId, nome, dados);
        }

        // Parte comum aos dois caminhos: o usuario dono, a senha, o e-mail e o WhatsApp.
        private async Task<ContaDeClienteCriada> CriarAcessoAsync(Guid empresaId, string nome, DadosDaContaDeCliente dados)
        {
            var senhaProvisoria = GerarSenhaProvisoria();

            await _unitOfWork.Usuario.Incluir(new Usuario
            {
                Id = Guid.NewGuid(),
                EmpresaId = empresaId,
                Nome = nome,
                Email = dados.Email,
                SenhaHash = BCrypt.Net.BCrypt.HashPassword(senhaProvisoria),
                // Primeiro usuario da conta e o dono: precisa enxergar Chats e Flows, e poder
                // criar o resto da equipe.
                IsAdmin = true,
                DataCriacao = DateTime.Now
            });

            // O e-mail sai fora da transacao de proposito: falha de SMTP nao pode desfazer uma
            // conta ja criada. Se nao chegar, a senha volta na resposta para quem cadastrou.
            await EnviarBoasVindasAsync(dados.Email, nome, senhaProvisoria);

            // Mesmo raciocinio: o servico engole as proprias falhas -- nada aqui pode derrubar
            // a criacao de uma conta que ja existe no banco.
            await _onboarding.ReceberNovoClienteAsync(
                nome, dados.Telefone, dados.Email, empresaId, dados.Plano,
                avisarPagamentoConfirmado: dados.PagamentoJaConfirmado);

            return new ContaDeClienteCriada
            {
                EmpresaId = empresaId,
                SenhaProvisoria = senhaProvisoria
            };
        }

        private async Task EnviarBoasVindasAsync(string email, string nome, string senhaProvisoria)
        {
            try
            {
                await _emailService.EnviarBoasVindasAsync(email, nome, senhaProvisoria);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Conta criada para {Email}, mas o e-mail de acesso falhou", email);
            }
        }

        // Senha so serve para o primeiro acesso; o usuario troca depois na tela de trocar senha.
        // Sem 0/O e 1/l/I porque essa senha costuma ser ditada por telefone.
        private static string GerarSenhaProvisoria()
        {
            const string alfabeto = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
            var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(12);

            return new string(bytes.Select(b => alfabeto[b % alfabeto.Length]).ToArray());
        }
    }
}
