using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Numero.CriaNumero
{
    public class CriaNumeroCommand : IRequest<Response<CriaNumeroResult>>
    {
        public string UsuarioId { get; set; }

        public string NumeroTelefone { get; set; }

        public string? Descricao { get; set; }

        public string? InstanciaId { get; set; }
    }
}
