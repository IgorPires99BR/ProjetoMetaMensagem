using ProjetoMetaMensagem.Dominio.Common;
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
        private readonly int tamanhoSenha = 8;
        public EsqueceuASenhaHandler(IEmailService whatsappService)
        {
            _emailService = whatsappService;
        }

        public async Task<Response<EsqueceuASenhaResult>> Handle(EsqueceuASenhaCommand command)
        {
            var response = new Response<EsqueceuASenhaResult>();


            //var validator = new CriaClienteValidator();
            //var validateResult = validator.Validate(request);

            //if (!validateResult.IsValid)
            //{
            //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
            //    return response;
            //}

            const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var senha = new string(Enumerable.Repeat(caracteres, tamanhoSenha)
                    .Select(s => s[random.Next(s.Length)]).ToArray());

            var email = await _emailService.EnviarEmailAsync(command.Email, senha);
            return response;
        }
    }
}
