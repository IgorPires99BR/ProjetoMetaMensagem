using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Entidades;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Cobranca.ListaAssinaturas
{
    public class ListaAssinaturasHandler : IRequestHandler<ListaAssinaturasCommand, Response<ListaAssinaturasResult>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ListaAssinaturasHandler> _logger;

        public ListaAssinaturasHandler(IUnitOfWork unitOfWork, ILogger<ListaAssinaturasHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<ListaAssinaturasResult>> Handle(ListaAssinaturasCommand command)
        {
            var response = new Response<ListaAssinaturasResult>();

            try
            {
                var assinaturas = new List<Assinatura>();

                if (command.EhAdminPlataforma)
                {
                    assinaturas.AddRange(await _unitOfWork.Assinatura.Listar());
                }
                else if (command.EmpresaIdSolicitante.HasValue)
                {
                    var minha = await _unitOfWork.Assinatura.ObterPorEmpresa(command.EmpresaIdSolicitante.Value);
                    if (minha != null) assinaturas.Add(minha);
                }

                // Uma consulta de empresas só, em vez de uma por assinatura.
                var empresas = (await _unitOfWork.Empresa.Obter()).ToDictionary(e => e.Id, e => e.Nome);

                var resultado = new ListaAssinaturasResult
                {
                    Assinaturas = assinaturas
                        .Select(a => new AssinaturaResumo(a, empresas.TryGetValue(a.EmpresaId, out var nome) ? nome : null))
                        .ToList()
                };

                resultado.TotalAtivas = assinaturas.Count(a => a.Status == StatusAssinatura.Ativa);
                resultado.ReceitaMensalEstimada = assinaturas
                    .Where(a => a.Status == StatusAssinatura.Ativa && a.ValorCentavos.HasValue)
                    .Sum(a => a.ValorCentavos!.Value) / 100m;

                response.AddValue(resultado);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ListaAssinaturasHandler));
            }

            return response;
        }
    }
}
