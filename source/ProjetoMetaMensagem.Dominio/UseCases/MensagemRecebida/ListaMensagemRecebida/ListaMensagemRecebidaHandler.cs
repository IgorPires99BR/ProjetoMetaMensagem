using Newtonsoft.Json.Linq;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Helpers.HTMLHelper;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.MensagemRecebida.ListaMensagemRecebida
{
    public class ListaMensagemRecebidaHandler : IRequestHandler<ListaMensagemRecebidaCommand, Response<ListaMensagemRecebidaResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private HTMLHelper _htmlHelper;

        public ListaMensagemRecebidaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _htmlHelper = new HTMLHelper();
        }

        public async Task<Response<ListaMensagemRecebidaResult>> Handle(ListaMensagemRecebidaCommand command)
        {
            var response = new Response<ListaMensagemRecebidaResult>();

            var mensagensRecebidasTask = await _unitOfWork.MensagemRecebida
                 .ListarPorContato(command.EmpresaId, command.ContatoId);

            var historicoEnviadasTask = await _unitOfWork.HistoricoDisparo
                .ListarPorContato(command.EmpresaId, command.ContatoId);

            // 2. Mapeia as recebidas (cliente -> user)
            var listaRecebidas = mensagensRecebidasTask.Select(m => new
            {
                m.Id,
                From = "user",
                Text = m.Conteudo,
                Data = m.DataRecebimento
            });

            // 3. Mapeia as enviadas tratando o JSON de disparo de template
            var listaEnviadas = historicoEnviadasTask.Select(h => new
            {
                h.Id,
                From = "me", // ou "bot", dependendo da UI
                Text = FormatarConteudoHistorico(h.Conteudo),
                Data = h.DataEnvio
            });

            // 4. Une as duas listas e ordena pela data/hora
            var resultadoDto = listaRecebidas
                .Concat(listaEnviadas)
                .OrderBy(x => x.Data)
                .Select(x => new ItemMensagemChatDto
                {
                    Id = x.Id,
                    From = x.From,
                    Text = x.Text,
                    Time = x.Data.ToString("HH:mm")
                })
                .ToList();

            var resultFinal = new ListaMensagemRecebidaResult { Mensagens = resultadoDto };
            response.AddValue(resultFinal);

            return response;
        }

        private string FormatarConteudoHistorico(string conteudo)
        {
            if (string.IsNullOrWhiteSpace(conteudo))
                return string.Empty;

            // Se não for um JSON, retorna limpo pelo HTMLHelper
            if (!conteudo.TrimStart().StartsWith("{"))
                return _htmlHelper.LimparHtml(conteudo);

            try
            {
                var jsonObj = JObject.Parse(conteudo);

                // Tenta extrair o PayloadEnvio para pegar o nome do template
                if (jsonObj["PayloadEnvio"] != null)
                {
                    var payloadString = jsonObj["PayloadEnvio"].ToString();
                    var payloadObj = JObject.Parse(payloadString);

                    var templateName = payloadObj["template"]?["name"]?.ToString();

                    if (!string.IsNullOrEmpty(templateName))
                    {
                        // Exemplo de retorno: "📄 [Template: hello_world]"
                        return $"📄 [Template: {templateName}]";
                    }
                }
            }
            catch
            {
                // Fallback caso a desserialização falhe
            }

            return _htmlHelper.LimparHtml(conteudo);
        }
    }
}
