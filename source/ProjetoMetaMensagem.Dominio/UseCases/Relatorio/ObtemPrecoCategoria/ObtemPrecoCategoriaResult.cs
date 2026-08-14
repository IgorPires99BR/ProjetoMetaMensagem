using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Relatorio.ObtemPrecoCategoria
{
    public class ObtemPrecoCategoriaResult
    {
        public List<PrecoCategoriaDto> Precos { get; set; } = new();
    }
}
