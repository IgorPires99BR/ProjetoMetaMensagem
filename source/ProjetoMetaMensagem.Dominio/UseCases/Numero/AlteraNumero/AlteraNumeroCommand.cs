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
        public Guid Id { get; set; }
        public Guid UsuarioId { get; set; }

        public string NumeroTelefone { get; set; }

        public string? Descricao { get; set; }

        public string? InstanciaId { get; set; }

        // Preenchido pelo controller a partir do JWT (null = administrador). Sem isso o UPDATE
        // casava so pelo Id e permitia alterar numero de outra empresa.
        public Guid? EmpresaIdSolicitante { get; set; }
    }
}
