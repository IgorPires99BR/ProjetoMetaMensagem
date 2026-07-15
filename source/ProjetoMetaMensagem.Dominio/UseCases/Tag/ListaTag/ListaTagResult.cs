using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Tag.ListaTag
{
    public class ListaTagResult
    {
        public ListaTagResult(Entidades.Tag tag)
        {
            Id = tag.Id;
            Nome = tag.Nome;
            Cor = tag.Cor;
        }

        public Guid Id { get; set; }
        public string Nome { get; set; }
        public string Cor { get; set; }
    }
}
