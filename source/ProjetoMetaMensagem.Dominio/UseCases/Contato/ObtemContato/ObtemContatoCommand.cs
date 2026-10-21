using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato
{
    public class ObtemContatoCommand : IRequest<Response<List<ObtemContatoResult>>>
    {
        public ObtemContatoCommand(Guid? empresaIdSolicitante, bool apenasInativos = false)
        {
            EmpresaIdSolicitante = empresaIdSolicitante;
            ApenasInativos = apenasInativos;
        }

        // Escopo vem do token (null = administrador, ve todas as empresas), nunca da rota.
        public Guid? EmpresaIdSolicitante { get; set; }

        // Lista de inativos da tela de Contatos (pra reativar); o padrao continua so os ativos.
        public bool ApenasInativos { get; set; }
    }
}
