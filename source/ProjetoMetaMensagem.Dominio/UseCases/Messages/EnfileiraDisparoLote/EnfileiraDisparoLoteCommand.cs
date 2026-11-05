using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnfileiraDisparoLote
{
    // A tela so diz quem recebe, qual modelo e de onde sai cada variavel. Telefone, valores e o
    // nome do template sempre vem do banco: nada que da para forjar no corpo da requisicao.
    public class EnfileiraDisparoLoteCommand : IRequest<Response<EnfileiraDisparoLoteResult>>
    {
        public Guid IdEmpresa { get; set; }

        // Vindo do token: quem nao e admin da plataforma so dispara pela propria empresa.
        public Guid? EmpresaIdSolicitante { get; set; }

        public Guid TemplateId { get; set; }
        public List<Guid> ContatosIds { get; set; } = new();
        public List<Entidades.AgendamentoVariavelDto> Variaveis { get; set; } = new();
        public string? ParametroHeaderMediaUrl { get; set; }
        public List<string> ParametrosButton { get; set; } = new();

        // Gerada pela tela a cada pedido de envio; repetir o POST devolve o mesmo lote.
        public Guid ChaveIdempotencia { get; set; }
    }
}
