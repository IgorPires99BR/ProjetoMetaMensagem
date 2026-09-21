using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote
{
    public class EnviarMensagemTemplateMetaLoteResult
    {
        // Por telefone (contrato com o front): um telefone que aparece em mais de um contato
        // vira uma entrada so.
        public Dictionary<string, bool> RelatorioDisparos { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, string> RelatorioErros { get; set; } = new Dictionary<string, string>();

        // Por destinatario. Nao derivam mais de RelatorioDisparos, porque o dicionario por
        // telefone subconta quando dois contatos dividem o mesmo numero.
        public int TotalProcessado { get; set; }
        public int TotalSucesso { get; set; }
        public int TotalFalha { get; set; }
    }
}
