using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.TrocaSenha
{
    // O e-mail de boas-vindas manda o cliente "trocar essa senha no primeiro acesso", mas nao
    // existia tela para isso: a unica saida era "Esqueci minha senha", que sorteia OUTRA senha
    // e manda por e-mail. Ou seja, o cliente nunca escolhia a propria senha, e a primeira
    // instrucao que recebia da plataforma nao tinha como ser cumprida.
    public class TrocaSenhaHandler : IRequestHandler<TrocaSenhaCommand, Response<TrocaSenhaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<TrocaSenhaHandler> _logger;

        public TrocaSenhaHandler(IUnitOfWork unitOfWork, ILogger<TrocaSenhaHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<TrocaSenhaResult>> Handle(TrocaSenhaCommand command)
        {
            var response = new Response<TrocaSenhaResult>();

            var validateResult = new TrocaSenhaValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            if (!command.UsuarioIdDoToken.HasValue)
            {
                response.AddErro("Não foi possível identificar o usuário logado.");
                return response;
            }

            try
            {
                var usuario = await _unitOfWork.Usuario.ObterPorId(command.UsuarioIdDoToken.Value);

                if (usuario == null || string.IsNullOrEmpty(usuario.SenhaHash))
                {
                    response.AddErro("Não foi possível identificar o usuário logado.");
                    return response;
                }

                // Exigir a senha atual e o que impede que um token roubado (ou uma sessao
                // esquecida aberta) vire a tomada definitiva da conta: sem ela, quem pegasse
                // o token trocaria a senha e trancaria o dono do lado de fora.
                if (!BCrypt.Net.BCrypt.Verify(command.SenhaAtual, usuario.SenhaHash))
                {
                    response.AddErro("A senha atual está incorreta.");
                    return response;
                }

                _unitOfWork.BeginTransaction();

                var hashNovo = BCrypt.Net.BCrypt.HashPassword(command.SenhaNova);
                var linhas = await _unitOfWork.Usuario.AlterarSenha(usuario.Id, hashNovo);

                if (linhas == 0)
                {
                    _unitOfWork.Rollback();
                    response.AddErro("Não foi possível alterar a senha. Tente novamente.");
                    return response;
                }

                _unitOfWork.Commit();
                response.AddValue(new TrocaSenhaResult());
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErroServico(ex, _logger, nameof(TrocaSenhaHandler));
            }

            return response;
        }
    }
}
