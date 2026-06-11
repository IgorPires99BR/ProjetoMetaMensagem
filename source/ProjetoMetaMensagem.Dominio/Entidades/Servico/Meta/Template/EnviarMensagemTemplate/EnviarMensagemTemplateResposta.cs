using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplate
{
    public class EnviarMensagemTemplateResposta
    {
        public bool Sucesso { get; set; }
        public string WamidMeta { get; set; }
        public string Erro { get; set; }
    }
}
