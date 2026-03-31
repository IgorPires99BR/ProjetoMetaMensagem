using Microsoft.AspNetCore.Mvc;
using ProjetoMetaMensagem.Dominio.Common;

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
            if (response.HasValidations)
            {
                foreach (var error in response.Erros)
                {
                    if (error.StartsWith("302:"))
                        return controllerBase.Redirect(error.Substring(4));
                    if (error.StartsWith("400:"))
                        return controllerBase.BadRequest(response.Erros);
                    if (error.StartsWith("403:"))
                        return controllerBase.Forbid();
                    if (error.StartsWith("404:"))
                        return controllerBase.NotFound(response.Erros);
                    if (error.StartsWith("500:"))
                        return controllerBase.StatusCode(StatusCodes.Status500InternalServerError, response.Erros);
                }

                return controllerBase.BadRequest(response.Erros);
            }
            return null;
        }
    }
}
