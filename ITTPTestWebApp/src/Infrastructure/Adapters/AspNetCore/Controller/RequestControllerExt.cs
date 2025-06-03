using System.Net;
using System.Security.Claims;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Network.RestBodyModels;

namespace ITTPTestWebApp.Adapters.AspNetCore.Controller
{
    public static class RequestControllerExt
    {
        public static async Task<IActionResult> ExecuteAsync
        (
            ControllerBase controller,
            bool needAuthorization,
            string contentType,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?)>> requestCheck,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>> requestHandle,
            List<string>? allowedHeaders = null,
            List<string>? permittedAddresses = null
        )
        {
            try
            {
                var (code, res) =  await RequestExecutorCore.Execute
                (
                    controller.Request,
                    controller.Response,
                    needAuthorization,
                    controller.Request.Method,
                    contentType,
                    requestCheck,
                    requestHandle,
                    allowedHeaders: allowedHeaders,
                    permittedAddresses: permittedAddresses
                );

                if (!string.IsNullOrEmpty(res)) 
                { return new ContentResult() { StatusCode = (int)code, ContentType = controller.Response.ContentType, Content = res }; } 
                else { return controller.StatusCode((int)code); }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                return controller.StatusCode(StatusCodes.Status500InternalServerError, new ResponseErrorBody() { error = ex.Message });
            }
        }
    }
}
