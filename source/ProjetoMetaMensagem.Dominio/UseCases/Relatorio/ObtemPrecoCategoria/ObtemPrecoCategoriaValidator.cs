using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemPrecoCategoria
{
    // Sem regras de proposito: o unico campo do command vem da claim do JWT, e o handler
    // trata a autorizacao.
    public class ObtemPrecoCategoriaValidator : AbstractValidator<ObtemPrecoCategoriaCommand>
    {
    }
}
