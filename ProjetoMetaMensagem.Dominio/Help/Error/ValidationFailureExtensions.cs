using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Help.Error
{
    public static class ValidationFailureExtensions
    {
        public static List<string> ToCustomValidationFailure(this IList<ValidationFailure> failures)
        {
            return failures.Select(x => x.ErrorMessage).ToList();
        }
    }
}
