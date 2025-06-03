using System.Net;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Newtonsoft.Json;

using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Network.RestBodyModels;

namespace ITTPTestWebApp.Adapters.AspNetCore.EndpointRouteBuilder
{
    static partial class MapExt
    {
        public static void MapMethodsExt
        (
            this IEndpointRouteBuilder endpoints,
            string path,
            string method,
            bool needAuthorization,
            string contentType,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?)>> requestCheck,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>> requestProcessing,
            List<string>? allowedHeaders = null,
            List<string>? permittedAddresses = null
        )
        {
            
            endpoints.MapOptionsExt(path, needAuthorization, method, allowedHeaders);

            endpoints.MapMethods(path, [method], async context =>
            {
                if (needAuthorization && context.User?.Identity?.IsAuthenticated != true)
                { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return; }

                try
                {
                    var (code, res) = await RequestExecutorCore.Execute
                    (
                        context.Request,
                        context.Response,
                        needAuthorization,
                        method,
                        contentType,
                        requestCheck,
                        requestProcessing,
                        allowedHeaders: allowedHeaders,
                        permittedAddresses: permittedAddresses
                    );

                    context.Response.StatusCode = (int)code;
                    if (!string.IsNullOrEmpty(res))
                    {
                        var bytes = Encoding.UTF8.GetBytes(res);
                        context.Response.ContentLength = bytes.Length;
                        await context.Response.Body.WriteAsync(bytes);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Instance.Log(ex);
                    var error = JsonConvert.SerializeObject(new ResponseErrorBody()
                    { error = ex.Message }, Formatting.Indented);

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    var bytes = Encoding.UTF8.GetBytes(error);
                    context.Response.ContentLength = bytes.Length;
                    await context.Response.Body.WriteAsync(bytes);
                }
            });
        }
    }
}
