using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.Interfaces.Servicos;
using ProjetoMetaMensagem.Dominio.UseCases.Numero.ListarNumeros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Template.ListaTemplate
{
    public class ListaTemplateHandler : IRequestHandler<ListaTemplateCommand, Response<List<ListaTemplateResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMetaService _metaService;

        public ListaTemplateHandler(IUnitOfWork unitOfWork, IMetaService metaService)
        {
            _unitOfWork = unitOfWork;
            _metaService = metaService;
        }

        public async Task<Response<List<ListaTemplateResult>>> Handle(ListaTemplateCommand command)
        {
            var response = new Response<List<ListaTemplateResult>>();
            var listaTemplates = new List<ListaTemplateResult>();

            var validator = new ListaTemplateValidator();
            var validateResult = validator.Validate(command);

            if (!validateResult.IsValid)
            {
                response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                return response;
            }


            var templateBanco = await _unitOfWork.Template.ObterPorEmpresa(command.IdEmpresa);

            if (templateBanco == null)
            {
                response.AddErro("Não foram encontrados números no banco de dados");
                return response;
            }

            foreach (var template in templateBanco)
            {
                listaTemplates.Add(new ListaTemplateResult(template));
            }


            response.AddValue(listaTemplates);

            return response;

        }
    }
}
