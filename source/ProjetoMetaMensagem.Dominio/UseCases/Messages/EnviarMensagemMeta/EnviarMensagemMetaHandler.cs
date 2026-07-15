using Newtonsoft.Json;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.EnviarMensagemMeta
{
    public class EnviarMensagemMetaHandler : IRequestHandler<EnviarMensagemMetaCommand, Response<EnviarMensagemMetaResult>>
    {
        private readonly IMetaService _whatsappService;
        private readonly IUnitOfWork _unitOfWork;

        public EnviarMensagemMetaHandler(IMetaService whatsappService, IUnitOfWork unitOfWork)
        {
            _whatsappService = whatsappService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<EnviarMensagemMetaResult>> Handle(EnviarMensagemMetaCommand command)
        {
            var response = new Response<EnviarMensagemMetaResult>();

            try
            {
                _unitOfWork.BeginTransaction();

                var sucesso = await _whatsappService.EnviarTextoLivreAsync(command.Celular, command.textoMensagem);

                if (sucesso == null)
                {
                    response.AddErro("Erro ao acessar a Meta");
                    return response;
                }

                // Registra historico do disparo
                var historico = new HistoricoDisparo
                {
                    EmpresaId = command.EmpresaId,
                    ContatoId = command.ContatoId,
                    TipoDisparo = "Livre",
                    Conteudo = command.Template,
                    WamidMeta = "",
                    DataEnvio = DateTime.Now
                };


                await _unitOfWork.HistoricoDisparo.Incluir(historico);

                _unitOfWork.Commit();
            }
            catch (Exception ex)
            {
                _unitOfWork.Rollback();
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}

