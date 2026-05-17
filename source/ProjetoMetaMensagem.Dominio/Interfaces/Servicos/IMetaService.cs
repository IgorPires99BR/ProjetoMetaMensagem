using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.CriaNumeroMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.ObtemNumerosMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplate;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.ObtemTemplateMeta;
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

        Task<string?> BuscarWabaIdDaMetaAsync(string accessToken);

        Task<ObtemNumerosMetaResposta> ObterNumerosMetaAsync();
        Task<CriaNumeroMetaResposta> CriarNumeroMetaAsync(CriaNumeroMetaRequisicao requisicao);
        Task<ObtemTemplatesMetaResposta> ObterTemplatesMetaAsync();
    }
}
