using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemSituacaoSubconta
{
    public class ObtemSituacaoSubcontaResult
    {
        // False quando a subconta foi vinculada pelo painel (ou e conta propria da empresa): a
        // apiKey dela nunca passou pela plataforma, entao a situacao so pode ser vista no Asaas.
        public bool ConsultaDisponivel { get; set; }
        public bool CarteiraExterna { get; set; }

        public string? Geral { get; set; }
        public string? DadosComerciais { get; set; }
        public string? DadosBancarios { get; set; }
        public string? Documentacao { get; set; }
        public string? MotivoRecusa { get; set; }
        public IReadOnlyList<DocumentoSubconta> Documentos { get; set; } = new List<DocumentoSubconta>();

        public bool Aprovada => Geral == "APPROVED";
    }
}
