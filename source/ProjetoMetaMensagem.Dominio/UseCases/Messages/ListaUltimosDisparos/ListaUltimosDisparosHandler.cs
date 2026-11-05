using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Servicos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Messages.ListaUltimosDisparos
{
    public class ListaUltimosDisparosHandler : IRequestHandler<ListaUltimosDisparosCommand, Response<List<ListaUltimosDisparosResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListaUltimosDisparosHandler> _logger;

        public ListaUltimosDisparosHandler(IUnitOfWork unitOfWork, ILogger<ListaUltimosDisparosHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ListaUltimosDisparosResult>>> Handle(ListaUltimosDisparosCommand command)
        {
            var response = new Response<List<ListaUltimosDisparosResult>>();

            if (command.EmpresaIdSolicitante.HasValue)
                command.EmpresaId = command.EmpresaIdSolicitante.Value;

            var validateResult = new ListaUltimosDisparosValidator().Validate(command);
            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }

            try
            {
                var ultimos = await _unitOfWork.HistoricoDisparo.ObterUltimosDisparos(command.EmpresaId);

                response.AddValue(ultimos.Select(u => new ListaUltimosDisparosResult
                {
                    ContatoId = u.ContatoId,
                    DataUltimoDisparo = ParaBrasilia(u.DataUltimoDisparo)
                }).ToList());
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaUltimosDisparosHandler));
            }

            return response;
        }

        // DataEnvio e gravada com o relogio do servidor (UTC em producao). Sem converter, um envio
        // depois das 21h apareceria na tela com a data do dia seguinte e erraria a conta dos 5 dias.
        private static DateTime ParaBrasilia(DateTime horaDoServidor) =>
            HorarioBrasilia.ParaBrasilia(new DateTimeOffset(horaDoServidor, TimeZoneInfo.Local.GetUtcOffset(horaDoServidor)));
    }
}
