using ProjetoMetaMensagem.Dominio.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Leads.ListaConversas
{
    public class ListaConversaPorIdResult
    {
        public ListaConversaPorIdResult() { }

        // Mapeamento De/Para no construtor de objeto único
        public ListaConversaPorIdResult(Conversations con)
        {
            Id = con.id.ToString();
            CompanyId = con.company_id;
            Telefone = con.phone;
            Status = con.status;
            StatusFunil = con.status_funil;
            Nome = con.nome;
        }

        // Método utilitário para converter a lista inteira (o "De/Para" da coleção)
        public static List<ListaConversaPorIdResult> MapearListaLeads(List<Conversations> conversations)
        {
            return conversations.Select(con => new ListaConversaPorIdResult(con)).ToList();
        }

        public string Id { get; set; }
        public string CompanyId { get; set; }
        public string Telefone { get; set; }
        public string Status { get; set; }
        public string StatusFunil { get; set; }
        public string Nome { get; set; }
    }
}
