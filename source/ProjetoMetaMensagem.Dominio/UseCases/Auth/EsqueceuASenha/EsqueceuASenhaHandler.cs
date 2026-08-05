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
        public EsqueceuASenhaHandler(IEmailService whatsappService, IUnitOfWork unitOfWork)
        {
            _emailService = whatsappService;
            _unitOfWork = unitOfWork;
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
                var random = new Random();
                var senha = new string(Enumerable.Repeat(caracteres, tamanhoSenha)
                        .Select(s => s[random.Next(s.Length)]).ToArray());

                // Antes a senha era gerada e enviada por email, mas nunca era salva no banco --
                // o usuario recebia uma senha nova que nunca funcionava pra logar.
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha);
                await _unitOfWork.Usuario.Alterar(usuario);

                await _emailService.EnviarEmailAsync(command.Email, senha);

                response.AddValue(new EsqueceuASenhaResult());
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
