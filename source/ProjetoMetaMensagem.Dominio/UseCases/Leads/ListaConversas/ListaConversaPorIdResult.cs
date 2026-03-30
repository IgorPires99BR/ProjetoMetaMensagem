using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Leads.ListaConversas
{
    public class ListaConversaPorIdResult
    {
        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Telefone { get; set; }
        public string Status { get; set; }
        public string StatusFunil { get; set; }
        public string Nome { get; set; }
    }
}
