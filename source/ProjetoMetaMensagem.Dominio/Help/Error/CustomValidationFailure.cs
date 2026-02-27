using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Help.Error
{
    public  class CustomValidationFailure
    {
       public List<string> Erros { get; private set; }

        public CustomValidationFailure(List<string> _Erros)
        {
            Erros = _Erros;
        }

        public CustomValidationFailure(string erro)
        {
            if (Erros == null)
                Erros = new List<string>();
            Erros.Add(erro);
        }
    }
}
