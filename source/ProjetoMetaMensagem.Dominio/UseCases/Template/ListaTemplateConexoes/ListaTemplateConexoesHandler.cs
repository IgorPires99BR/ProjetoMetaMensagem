using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Repositorios;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplateConexoes
{
    public class ListaTemplateConexoesHandler : IRequestHandler<ListaTemplateConexoesCommand, Response<List<ListaTemplateConexoesResult>>>
    {
        private readonly ITemplateConexaoRepository _templateConexaoRepository;

        public ListaTemplateConexoesHandler(ITemplateConexaoRepository templateConexaoRepository)
        {
            _templateConexaoRepository = templateConexaoRepository;
        }

        public async Task<Response<List<ListaTemplateConexoesResult>>> Handle(ListaTemplateConexoesCommand command)
        {
            var response = new Response<List<ListaTemplateConexoesResult>>();

            try
            {
                var validator = new ListaTemplateConexoesValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var conexoesBanco = await _templateConexaoRepository.ListarPorEmpresa(command.EmpresaId);

                var lista = conexoesBanco.Select(c => new ListaTemplateConexoesResult(c)).ToList();

                response.AddValue(lista);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
