using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato
{
    // Nao valida EmpresaIdSolicitante: null e um valor legitimo (administrador, sem restricao).
    public class ObtemContatoValidator : AbstractValidator<ObtemContatoCommand>
    {
    }
}
