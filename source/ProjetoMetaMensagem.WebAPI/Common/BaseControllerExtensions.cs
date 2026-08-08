using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Common;
using System.Linq;

namespace ProjetoMetaMensagem.WebAPI.Common
{
    public static class BaseControllerExtensions
    {
        public static IActionResult ValidateResponse(this ControllerBase controllerBase, int statusCode, Response response)
        {
            if (response == null)
                return controllerBase.NoContent();

            var data = ValidateGenericResponse(controllerBase, response);

            if (data != null)
                return data;

            if (statusCode == 0)
                return controllerBase.Ok(response);

            if (statusCode == StatusCodes.Status204NoContent)
                return controllerBase.NoContent();

            return controllerBase.StatusCode(statusCode, response);
        }

        public static IActionResult ValidateResponse<T>(this ControllerBase controllerBase, int statusCode, Response<T> response)
        {
            if (response == null)
                return controllerBase.NoContent();

            var data = ValidateGenericResponse(controllerBase, response);

            if (data != null)
                return data;

            if (statusCode == 0)
                return controllerBase.Ok(response);

            if (statusCode == StatusCodes.Status204NoContent)
                return controllerBase.NoContent();

            return controllerBase.StatusCode(statusCode, response.Value);
        }

        public static IActionResult ValidateGenericResponse(this ControllerBase controllerBase, Response response)
        {
            if (!response.HasValidations)
                return null;

            var corpo = MontarCorpo(response.Erros);

            // Erro de servico (banco, integracao externa) manda no status: mesmo que tambem
            // exista erro de negocio na mesma resposta, o endpoint nao pode fingir que foi
            // so uma validacao (400) quando algo tecnico quebrou.
            var erroServico = response.Erros.FirstOrDefault(e => e.Tipo == TipoErro.Servico);
            if (erroServico != null)
                return controllerBase.StatusCode(erroServico.StatusCode ?? StatusCodes.Status500InternalServerError, corpo);

            // Erro de negocio com status proprio (ex: 404 "nao encontrado", 403 "acesso negado").
            var comStatusProprio = response.Erros.FirstOrDefault(e => e.StatusCode.HasValue);
            if (comStatusProprio != null)
                return controllerBase.StatusCode(comStatusProprio.StatusCode.Value, corpo);

            return controllerBase.BadRequest(corpo);
        }

        // Formato exposto ao front: mensagem (sempre segura pra mostrar) + tipo (Negocio ou
        // Servico, pra decidir COMO mostrar -- direto ou com uma mensagem generica de erro).
        private static object MontarCorpo(System.Collections.Generic.IReadOnlyCollection<Erro> erros)
        {
            return erros.Select(erro => new { mensagem = erro.Mensagem, tipo = erro.Tipo.ToString() });
        }
    }
}
