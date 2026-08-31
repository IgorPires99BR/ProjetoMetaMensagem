using Microsoft.Extensions.Logging;
using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProjetoMetaMensagem.Dominio.Help.Error;

namespace ProjetoMetaMensagem.Dominio.UseCases.Contato.ObtemContato
{
    public class ObtemContatoHandler : IRequestHandler<ObtemContatoCommand, Response<List<ObtemContatoResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        private readonly ILogger<ObtemContatoHandler> _logger;

        public ObtemContatoHandler(IUnitOfWork unitOfWork, ILogger<ObtemContatoHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Response<List<ObtemContatoResult>>> Handle(ObtemContatoCommand command)
        {
            var response = new Response<List<ObtemContatoResult>>();

            try
            {
                var listaContato = new List<ObtemContatoResult>();

                var validator = new ObtemContatoValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var contatos = await _unitOfWork.Contato.ObterPorEmpresa(command.EmpresaIdSolicitante);

                // So busca origem se a empresa e conhecida (ObterPorEmpresa aceita null pra
                // "todas as empresas", caminho de admin de plataforma -- ali nao ha um unico
                // EmpresaId pra filtrar OrigemLead, e cruzar por telefone sem esse recorte
                // misturaria lead de uma empresa com contato de outra que usa o mesmo numero).
                var origens = command.EmpresaIdSolicitante.HasValue
                    ? await _unitOfWork.OrigemLead.ListarPorEmpresa(command.EmpresaIdSolicitante.Value)
                    : Enumerable.Empty<Entidades.OrigemLead>();

                var origensPorTelefone = AgruparOrigemMaisAntigaPorTelefone(origens);

                foreach (var contato in contatos)
                {
                    origensPorTelefone.TryGetValue(contato.Telefone, out var origem);
                    listaContato.Add(new ObtemContatoResult(contato, origem));
                }

                response.AddValue(listaContato);
            }
            catch (Exception ex)
            {
                response.AddErroServico(ex, _logger, nameof(ObtemContatoHandler));
            }

            return response;
        }

        // Separado do Handle pra poder ser testado sem banco. Um telefone pode ter mais de um
        // registro de origem (reapareceu por um segundo anuncio depois de a conversa reiniciar)
        // -- a primeira mensagem com anuncio e a que teve o merito de trazer o lead, entao ela
        // e a que fica mostrada.
        public static Dictionary<string, Entidades.OrigemLead> AgruparOrigemMaisAntigaPorTelefone(IEnumerable<Entidades.OrigemLead> origens)
        {
            return origens
                .GroupBy(o => o.Telefone)
                .ToDictionary(g => g.Key, g => g.OrderBy(o => o.DataPrimeiroContato).First());
        }
    }
}
