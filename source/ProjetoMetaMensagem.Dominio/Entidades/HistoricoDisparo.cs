using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    public class HistoricoDisparo
    {
        // Construtor para garantir a inicialização do GUID caso não venha do banco
        public HistoricoDisparo()
        {
            Id = Guid.NewGuid();
            DataEnvio = DateTime.Now;
        }

        public Guid Id { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid ContatoId { get; set; }
        public Guid? TemplateId { get; set; } // Nullable, pois o disparo livre não tem template
        public string TipoDisparo { get; set; } // Ex: "Template" ou "Livre"
        public string Conteudo { get; set; }
        public string WamidMeta { get; set; }
        public string? StatusEntrega { get; set; }
        public DateTime DataEnvio { get; set; }
        public string? MidiaId { get; set; }
        public string? TipoMidia { get; set; }
    }

    // Usado apenas em listagens que precisam do telefone do contato sem consulta extra (ex: chats ativos)
    public class HistoricoDisparoComTelefone : HistoricoDisparo
    {
        public string? TelefoneContato { get; set; }
    }
}
