using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.CriaNumeroMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.ObtemNumerosMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplate;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Template.EnviarMensagemTemplateLote;
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
        Task<EnviarMensagemTemplateResposta> EnviarTemplateAsync(EnviarMensagemTemplateRequisicao requisicao);
        Task<bool> EnviarTextoLivreAsync(string celular, string mensagem);

        // Retorno em Dictionary mapeando a resposta detalhada por telefone
        Task<Dictionary<string, EnviarMensagemTemplateResposta>> EnviarTemplatesEmLoteAsync(EnviarMensagemTemplateLoteRequisicao requisicaoLote);

        // Ajustado: Incluído os parâmetros de wabaId e accessToken para criação dinâmica de templates da empresa
        Task<string> CriarTemplateMetaAsync(CreateTemplateRequisicao novoTemplate, string wabaId, string accessToken);

        Task<string?> BuscarWabaIdDaMetaAsync(string accessToken);

        // Ajustados: Inclusão do parâmetro accessToken para isolamento do cabeçalho Authorization local
        Task<ObtemNumerosMetaResposta> ObterNumerosMetaAsync(string wabaId, string accessToken);
        Task<CriaNumeroMetaResposta> CriarNumeroMetaAsync(CriaNumeroMetaRequisicao requisicao, string wabaId, string accessToken);
        Task<ObtemTemplatesMetaResposta> ObterTemplatesMetaAsync(string wabaId, string accessToken);
    }
}