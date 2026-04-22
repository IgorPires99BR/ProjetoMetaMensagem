using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.AlteraNumero
{
    public class AlteraNumeroCommand : IRequest<Response<AlteraNumeroResult>>
    {
        public string Id { get; set; }
        public string UsuarioId { get; set; }

        public string NumeroTelefone { get; set; }

        public string? Descricao { get; set; }

        public string? InstanciaId { get; set; }

    }
}
