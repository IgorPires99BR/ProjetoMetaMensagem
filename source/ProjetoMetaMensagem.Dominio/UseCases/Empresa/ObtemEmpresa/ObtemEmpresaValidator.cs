using FluentValidation;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa
{
    // Sem regras de proposito: os dois campos do command vem das claims do JWT, e o proprio
    // handler ja trata o caso de EmpresaIdSolicitante nulo (admin enxerga todas as empresas,
    // nao-admin sem empresa recebe erro). Exigir o id aqui derrubaria a listagem do admin.
    public class ObtemEmpresaValidator : AbstractValidator<ObtemEmpresaCommand>
    {
    }
}
