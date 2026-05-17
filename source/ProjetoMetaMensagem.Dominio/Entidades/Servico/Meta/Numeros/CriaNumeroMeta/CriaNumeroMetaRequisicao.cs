using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.CriaNumeroMeta
{
    public class CriaNumeroMetaRequisicao
    {
        public string Telefone { get; set; }        // Ex: "16315551000"
        public string NomeVerificado { get; set; }  // Ex: "My Business Name"
        public string CodigoPais { get; set; }      // Ex: "1" ou "55"
    }
}
