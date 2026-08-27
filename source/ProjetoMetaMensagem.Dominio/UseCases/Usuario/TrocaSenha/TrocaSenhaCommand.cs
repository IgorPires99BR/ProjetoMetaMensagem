using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.TrocaSenha
{
    public class TrocaSenhaCommand : IRequest<Response<TrocaSenhaResult>>
    {
        public string SenhaAtual { get; set; } = string.Empty;

        public string SenhaNova { get; set; } = string.Empty;

        public string ConfirmacaoSenhaNova { get; set; } = string.Empty;

        // Preenchido pelo controller a partir das claims do token, nunca pelo corpo: se viesse
        // do JSON, qualquer usuario logado trocaria a senha de outra pessoa so mandando o id
        // dela. Por isso nao tem setter publico exposto na tela -- a tela nem sabe que existe.
        public Guid? UsuarioIdDoToken { get; set; }
    }
}
