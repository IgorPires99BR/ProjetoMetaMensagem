using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Common
{
    // Classe base para todas as respostas. Implementa IResponse.
    // Classe base para todas as respostas. Implementa IResponse.
    public class Response : IResponse
    {
        public bool HasValidations => _listaErrros.Any();
        private readonly List<string> _listaErrros = new List<string>();

        public IReadOnlyCollection<string> Erros { get { return _listaErrros; }  }

        public void AddErro(string erro)
        {
            _listaErrros.Add(erro);
        }

        public void AddErros(List<string> erros)
        {
            _listaErrros.AddRange(erros);
        }
    }

    public class Response<T> : Response
    {
        public T Value { get; private set; }

        public Response()
        {
        }

        public Response(T value) : this() // Construtor para sucesso com dados
        {
            Value = value;
        }

        public void AddValue(T value)
        {
            Value = value;
        }
    }
}

