using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.CriaTag
{
    public class CriaTagValidator : AbstractValidator<CriaTagCommand>
    {
        public CriaTagValidator()
        {
            RuleFor(x => x.Nome)
                .NotEmpty().WithMessage("O nome da tag é obrigatório.");
        }
    }
}
