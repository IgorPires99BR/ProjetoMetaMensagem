using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using ProjetoMetaMensagem.Servico.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Servico.Email
{
    public class EmailService
    {
        // Configurações vindas do seu Gmail
        private readonly GmailConfiguration _config;

        public EmailService(IOptions<GmailConfiguration> options)
        {
            _config = options.Value;
        }

        public async Task<bool> EnviarEmailAsync(string emailDestino, string novaSenha)
        {
            try
            {
                // No .NET, para a porta 465 (SSL implícito) o SmtpClient nativo pode oscilar.
                // A recomendação para o Gmail é porta 587 com EnableSsl = true.
                using (var client = new SmtpClient(_config.SmtpServer, _config.SmtpPort))
                {
                    client.Credentials = new NetworkCredential(_config.EmailRemetente, _config.SenhaApp);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        // Equivale ao msg['From'] do Python
                        From = new MailAddress(_config.EmailRemetente, "Contact Solution"),
                        Subject = "Sua Nova Senha Temporária - Contact Solution",
                        IsBodyHtml = false // Define como plain text igual ao seu Python
                    };

                    // Montagem do corpo (String Raw)
                    mailMessage.Body = $@"
                     Olá,
                     
                     Recebemos uma solicitação de recuperação de senha para sua conta no Contact Solution.
                     Sua nova senha temporária é: {novaSenha}
                     
                     Por favor, acesse o sistema com esta senha e altere-a imediatamente no seu perfil.
                     
                     Atenciosamente,
                     Equipe Contact Solution.";

                    mailMessage.To.Add(emailDestino);

                    await client.SendMailAsync(mailMessage);

                    // Log de sucesso (simulando o seu print)
                    Console.WriteLine($"✅ E-mail enviado com sucesso para {emailDestino}");
                    return true;
                }
            }
            catch (Exception ex)
            {
                // Log de erro (simulando o seu print do exception)
                Console.WriteLine($"❌ Erro ao enviar e-mail: {ex.Message}");
                return false;
            }
        }
    }
}
