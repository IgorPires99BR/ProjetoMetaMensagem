using ProjetoMetaMensagem.Dominio.Common;
using ProjetoMetaMensagem.Dominio.Help.Error;
using ProjetoMetaMensagem.Dominio.Interfaces;
using ProjetoMetaMensagem.Dominio.Interfaces.Mediator;
using ProjetoMetaMensagem.Dominio.UseCases.Empresa.DeletaEmpresa;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjetoMetaMensagem.Dominio.UseCases.Empresa.ObtemEmpresa
{
    public class ObtemEmpresaHandler : IRequestHandler<ObtemEmpresaCommand, Response<List<ObtemEmpresaResult>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ObtemEmpresaHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }


        public async Task<Response<List<ObtemEmpresaResult>>> Handle(ObtemEmpresaCommand command)
        {
            var response = new Response<List<ObtemEmpresaResult>>();

            try
            {
                var listaEmpresa = new List<ObtemEmpresaResult>();

                var validator = new ObtemEmpresaValidator();
                var validateResult = validator.Validate(command);

                if (!validateResult.IsValid)
                {
                    response.AddErros(validateResult.Errors.ToCustomValidationFailure());
                    return response;
                }

                var empresaBanco = await _unitOfWork.Empresa.Obter();

                // Quem nao e admin so pode enxergar a propria empresa. Antes a lista inteira
                // era devolvida pra qualquer autenticado, expondo inclusive o MetaAccessToken
                // das outras empresas.
                if (!command.SolicitanteEhAdmin)
                {
                    if (command.EmpresaIdSolicitante == null)
                    {
                        response.AddErro("Não foi possível identificar a empresa do usuário logado.");
                        return response;
                    }

                    empresaBanco = empresaBanco.Where(e => e.Id == command.EmpresaIdSolicitante.Value).ToList();
                }

                foreach (var empresa in empresaBanco)
                {
                    listaEmpresa.Add(new ObtemEmpresaResult(empresa));
                }

                response.AddValue(listaEmpresa);
            }
            catch (Exception ex)
            {
                response.AddErro($"Erro: {ex.Message}");
            }

            return response;
        }
    }
}
