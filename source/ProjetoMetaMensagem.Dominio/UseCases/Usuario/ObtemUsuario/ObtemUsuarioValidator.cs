using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.ObtemUsuario
{
    public class ObtemUsuarioValidator : AbstractValidator<ObtemUsuarioCommand>
    {
        public ObtemUsuarioValidator()
        {
            // IdEmpresa nulo e valido de proposito: e o sinal de "todas as empresas" (uso
            // exclusivo da conta de plataforma, ver UsuariosController.ObterTodos).
        }
    }
}
