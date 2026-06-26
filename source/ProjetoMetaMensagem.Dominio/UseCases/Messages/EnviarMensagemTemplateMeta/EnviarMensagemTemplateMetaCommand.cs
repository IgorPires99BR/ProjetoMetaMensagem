using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemTemplateMeta
{
    public class EnviarMensagemTemplateMetaCommand : IRequest<Response<EnviarMensagemTemplateMetaResult>>
    {
        public Guid IdEmpresa { get; set; }
        public string Telefone { get; set; } // Formato: 5511999998888
        public string NomeTemplate { get; set; }
        public string Idioma { get; set; } = "pt_BR";

        // Lista de strings para preencher {{1}}, {{2}}... na ordem
        public List<string> ParametrosBody { get; set; } = new List<string>();

        // Se o botão for dinâmico (URL com final variável)
        public List<string> ParametrosButton { get; set; } = new List<string>();
    }
}
