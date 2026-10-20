using System;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.CriaContato
{
    public class CriaContatoResult
    {
        // Usados pelo controller para enfileirar o gatilho "ao cadastrar contato".
        public Guid ContatoId { get; set; }
        public Guid EmpresaId { get; set; }
    }
}
