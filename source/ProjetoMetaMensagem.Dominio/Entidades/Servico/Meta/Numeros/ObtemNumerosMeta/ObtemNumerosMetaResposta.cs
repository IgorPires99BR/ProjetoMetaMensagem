using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.ObtemNumerosMeta
{
    public class ObtemNumerosMetaResposta
    {
        public List<NumeroMetaDto> Numeros { get; set; } = new();

    }

    public class NumeroMetaDto
    {
        public string Id { get; set; }
        public string NumeroFormatado { get; set; }
        public string NomeVerificado { get; set; }
        public string Status { get; set; }
        public string Qualidade { get; set; }
        public string CodigoPais { get; set; }
        public bool EhContaOficial { get; set; }
    }

}
