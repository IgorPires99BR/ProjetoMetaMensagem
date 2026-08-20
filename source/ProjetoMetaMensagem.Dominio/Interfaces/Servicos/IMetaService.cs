using ProjetoMetaMensagem.Dominio.Interfaces.Servicos.Meta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta;
using ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMetaLote;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.Interfaces.Servicos
{
    public interface IMetaService
    {
        Task<string?> BuscarWabaIdDaMetaAsync(string accessToken);

        // Ajustados: Inclusão do parâmetro accessToken para isolamento do cabeçalho Authorization local
        Task<List<NumeroMetaDto>> ObterNumerosMetaAsync(string wabaId, string accessToken);
        Task<string> CriarNumeroMetaAsync(string telefone, string nomeVerificado, string codigoPais, string wabaId, string accessToken);
        Task<List<TemplateMetaDto>> ObterTemplatesMetaAsync(string wabaId, string accessToken);

        // Embedded Signup: troca o "code" de autorização retornado pelo SDK JS da Meta por um token de sistema
        Task<string> TrocarCodeEmbeddedSignupAsync(string code);

        // Troca um token short-lived (o que TrocarCodeEmbeddedSignupAsync devolve) por um
        // long-lived (~60 dias) via GET oauth/access_token?grant_type=fb_exchange_token.
        // Sem isso o token do Embedded Signup expira rapido e o numero "desconecta" sozinho.
        Task<(string Token, DateTime? ExpiraEm)> TrocarTokenLongLivedAsync(string shortLivedToken);

        // Assina o nosso App aos webhooks do WABA do cliente (POST /{waba-id}/subscribed_apps).
        // Passo obrigatorio pos-Embedded-Signup: sem ele a Meta nunca entrega eventos (mensagens
        // recebidas, status) desse WABA no nosso webhook, mesmo com o numero "conectado" no banco.
        Task<bool> AssinarAppNoWabaAsync(string wabaId, string accessToken);

        // CoEx: habilita a coexistência entre o app WhatsApp Business e a Cloud API para o phone_number_id informado
        Task<ResultadoCoexistencia> AtivarCoexistenciaAsync(string phoneNumberId, string accessToken, string pin);

        Task<ResultadoEnvioTemplate> EnviarTemplateAsync(EnviarMensagemTemplateMetaCommand command, string phoneNumberId, string accessToken);

        // Retorno em Dictionary mapeando a resposta detalhada por telefone
        Task<Dictionary<string, ResultadoEnvioTemplate>> EnviarTemplatesEmLoteAsync(EnviarMensagemTemplateMetaLoteCommand comandoLote, string phoneNumberId, string accessToken);

        Task<string> CriarTemplateMetaAsync(string nome, string idioma, string categoria, List<ComponenteTemplateEnvio> componentes, string wabaId, string accessToken);

        // Edita um template PENDING/REJECTED (a Meta so aceita edicao nesses status), via POST /{template-id}.
        // Nao aceita alterar nome nem idioma -- so category e components.
        Task<string> AtualizarTemplateMetaAsync(string metaTemplateId, string categoria, List<ComponenteTemplateEnvio> componentes, string accessToken);

        // DELETE /{waba_id}/message_templates?name=...&hsm_id=... -- remove o template (todas as
        // variantes de idioma daquele nome, a menos que hsm_id restrinja a uma variante especifica).
        // 404 da Meta (template ja nao existe la) e tratado como sucesso idempotente.
        Task<bool> ExcluirTemplateMetaAsync(string nomeTemplate, string? metaTemplateId, string wabaId, string accessToken);

        // Resumable Upload API da Meta: sobe um arquivo de exemplo (imagem/vídeo/documento) e devolve
        // o "handle" exigido no campo example.header_handle da criação de template com HEADER de mídia
        Task<string> UploadMidiaExemploAsync(string appId, string accessToken, byte[] arquivo, string mimeType);

        Task<string> EnviarTextoLivreAsync(string celular, string mensagem, string accessToken, string phoneNumberId);

        // Mensagem interativa com ate 3 botoes de resposta rapida. E mensagem de sessao (igual
        // texto livre), nao template -- so entrega dentro da janela de 24h da conversa.
        Task<string> EnviarBotoesAsync(string celular, string corpo, List<string> botoes, string accessToken, string phoneNumberId);

        // Baixa os bytes de uma mídia recebida via webhook (mediaId) usando o token da empresa
        Task<(byte[] Bytes, string MimeType)> BaixarMidiaAsync(string mediaId, string accessToken);

        // Sobe um arquivo (imagem/audio/documento) pra Meta e envia como mensagem pro contato; retorna o wamid e o mediaId gerado
        Task<(string WamidMeta, string MediaId)> EnviarMidiaAsync(string celular, byte[] arquivo, string mimeType, string tipoMidia, string accessToken, string phoneNumberId);
    }
}
