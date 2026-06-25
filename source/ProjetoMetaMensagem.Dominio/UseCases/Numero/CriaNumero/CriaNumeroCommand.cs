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
        public Guid UsuarioId { get; set; }
        public Guid IdEmpresa { get; set; }

        public string NumeroTelefone { get; set; }

        public string? NomeEmpresa { get; set; }
    }
}
