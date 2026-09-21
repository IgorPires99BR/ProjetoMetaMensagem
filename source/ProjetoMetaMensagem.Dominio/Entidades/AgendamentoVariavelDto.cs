using System;

namespace ProjetoMetaMensagem.Dominio.Entidades
{
    // Definicao de UMA variavel do corpo do template (posicao = ordem da lista). Usado pelos
    // Agendamentos (gravado em VariaveisJson) e pelo disparo em lote.
    public class AgendamentoVariavelDto
    {
        // "fixo", "parametro" ou um campo do contato -- ver ResolvedorDeVariaveis.
        public string Origem { get; set; }
        public string ValorFixo { get; set; }

        // So quando Origem = "parametro": o Parametro cadastrado da empresa que fornece o valor.
        public Guid? ParametroId { get; set; }
    }
}
