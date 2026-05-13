using ProjetoMetaMensagem.Dominio.Entidades.Meta.Template;
using ProjetoMetaMensagem.Dominio.Entidades.Meta.Template.EnviarMensagemTemplate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface IMetaService
    {
        Task<bool> EnviarTemplateAsync(EnviarMensagemTemplateRequisicao requisicao);
        Task<bool> EnviarTextoLivreAsync(string celular, string mensagem);

        Task<string> CriarTemplateMetaAsync(CreateTemplateRequisicao novoTemplate);
    }
}
