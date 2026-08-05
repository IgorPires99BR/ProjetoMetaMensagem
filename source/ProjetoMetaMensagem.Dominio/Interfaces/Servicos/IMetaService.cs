using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.AtivaCoexistencia;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.CriaNumeroMeta;
using ProjetoMetaMensagem.Dominio.Entidades.Servico.Meta.Numeros.EmbeddedSignup;
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
    public interface    IMetaService
    {
        Task<EnviarMensagemTemplateResposta> EnviarTemplateAsync(EnviarMensagemTemplateRequisicao requisicao, string phoneNumberId, string accessToken);
        Task<bool> EnviarTextoLivreAsync(string celular, string mensagem, string accessToken);

        // Retorno em Dictionary mapeando a resposta detalhada por telefone
        Task<Dictionary<string, EnviarMensagemTemplateResposta>> EnviarTemplatesEmLoteAsync(EnviarMensagemTemplateLoteRequisicao requisicaoLote, string phoneNumberId, string accessToken);

        // Ajustado: Incluído os parâmetros de wabaId e accessToken para criação dinâmica de templates da empresa
        Task<string> CriarTemplateMetaAsync(CreateTemplateRequisicao novoTemplate, string wabaId, string accessToken);

        Task<string?> BuscarWabaIdDaMetaAsync(string accessToken);

        // Ajustados: Inclusão do parâmetro accessToken para isolamento do cabeçalho Authorization local
        Task<ObtemNumerosMetaResposta> ObterNumerosMetaAsync(string wabaId, string accessToken);
        Task<CriaNumeroMetaResposta> CriarNumeroMetaAsync(CriaNumeroMetaRequisicao requisicao, string wabaId, string accessToken);
        Task<ObtemTemplatesMetaResposta> ObterTemplatesMetaAsync(string wabaId, string accessToken);

        // Embedded Signup: troca o "code" de autorização retornado pelo SDK JS da Meta por um token de sistema
        Task<TrocaCodeMetaResposta> TrocarCodeEmbeddedSignupAsync(string code);

        // CoEx: habilita a coexistência entre o app WhatsApp Business e a Cloud API para o phone_number_id informado
        Task<AtivaCoexistenciaMetaResposta> AtivarCoexistenciaAsync(string phoneNumberId, string accessToken, string pin);
    }
}