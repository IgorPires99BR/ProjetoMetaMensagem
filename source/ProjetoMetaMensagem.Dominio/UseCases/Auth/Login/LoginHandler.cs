using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Auth.Login
{
    public class LoginHandler : IRequestHandler<LoginCommand, Response<LoginResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        public LoginHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<LoginResult>> Handle(LoginCommand request)
        {
            var response = new Response<LoginResult>();

            //var validator = new CriaClienteValidator();
            //var validateResult = validator.Validate(request);

            //if (!validateResult.IsValid)
            //{
            //    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
            //    return response;
            //}

            var login = await _unitOfWork.CompaniesRepository.Login(request.email, request.password);

            if (login == null)
                throw new Exception("Usuario e senha não encontrados no banco de dados.");


            response.AddValue(new LoginResult(login));

            return response;
        }
    }
}
