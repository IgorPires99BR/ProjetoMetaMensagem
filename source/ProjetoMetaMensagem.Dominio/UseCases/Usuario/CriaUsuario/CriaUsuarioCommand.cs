using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Usuario.CriaUsuario
{
    public class CriaUsuarioCommand : IRequest<Response<CriaUsuarioResult>>
    {
        public Guid EmpresaId { get; set; }

        public string Nome { get; set; }

        public string? Email { get; set; }

        public string? SenhaHash { get; set; }

        // A tela de Usuarios sempre mandou "perfil", mas o comando nao tinha o campo: o valor
        // era descartado no bind e todo mundo virava operador, inclusive quem foi criado como
        // "Administrador (Total)". O dono da conta (criado pelo pagamento) nao conseguia dar
        // acesso de admin pra um socio -- e a tela dizia que tinha dado.
        public string? Perfil { get; set; }

        // Escopo do token, nunca do corpo: senao o proprio atacante escolheria a empresa
        // (e se promoveria a admin) na requisicao.
        public Guid? EmpresaIdSolicitante { get; set; }

        public bool SolicitanteEhAdmin { get; set; }
    }
}
