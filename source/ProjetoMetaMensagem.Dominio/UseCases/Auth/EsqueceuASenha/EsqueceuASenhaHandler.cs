using ProjetoMetaMensagem.Dominio.Help.Error;
using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Auth.EsqueceuASenha
{
    public class EsqueceuASenhaHandler : IRequestHandler<EsqueceuASenhaCommand, Response<EsqueceuASenhaResult>>
    {
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly int tamanhoSenha = 8;
        private readonly ILogger<EsqueceuASenhaHandler> _logger;

        public EsqueceuASenhaHandler(IEmailService whatsappService, IUnitOfWork unitOfWork, ILogger<EsqueceuASenhaHandler> logger)
        {
            _emailService = whatsappService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<EsqueceuASenhaResult>> Handle(EsqueceuASenhaCommand command)
        {
            var response = new Response<EsqueceuASenhaResult>();

            try
            {
                var usuario = await _unitOfWork.Usuario.ObterPorEmail(command.Email);
                if (usuario == null)
                {
                    // Nao revela se o email existe ou nao, pra nao dar pista pra enumeracao de usuarios
                    response.AddValue(new EsqueceuASenhaResult());
                    return response;
                }

                const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                // Gerador criptografico em vez de System.Random: essa string vira a senha de
                // acesso do usuario, e o Random comum e previsivel a partir da semente.
                var senha = new string(Enumerable.Range(0, tamanhoSenha)
                    .Select(_ => caracteres[System.Security.Cryptography.RandomNumberGenerator.GetInt32(caracteres.Length)])
                    .ToArray());

                // A troca da senha e o envio do email andam juntos dentro da mesma transacao.
                // Antes a senha era gravada e comitada ANTES do envio, e o envio falhando so
                // devolvia false (o EmailService engole a excecao) -- resultado: a senha antiga
                // parava de funcionar, a nova nunca chegava por email e o usuario ficava
                // trancado fora da conta, com a tela dizendo que o email tinha sido enviado.
                _unitOfWork.BeginTransaction();

                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
                // null = sem recorte por empresa: a recuperacao de senha roda sem usuario
                // logado, e o usuario alvo ja foi resolvido pelo proprio email informado.
                await _unitOfWork.Usuario.Alterar(usuario, null);

                var emailEnviado = await _emailService.EnviarEmailAsync(command.Email, senha);
                if (!emailEnviado)
                {
                    _unitOfWork.Rollback();
                    _logger.LogError("Falha ao enviar o e-mail de recuperacao de senha para {Email}", command.Email);
                    response.AddErro("Não foi possível enviar o e-mail de recuperação agora. Tente novamente em alguns minutos.");
                    return response;
                }

                _unitOfWork.Commit();

                response.AddValue(new EsqueceuASenhaResult());
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(EsqueceuASenhaHandler));
            }

            return response;
        }
    }
}
