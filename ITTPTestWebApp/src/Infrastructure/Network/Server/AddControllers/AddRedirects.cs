using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using static ITTPTestWebApp.Network.ServerCommon;
using static ITTPTestWebApp.Adapters.AspNetCore.EndpointRouteBuilder.MapExt;

namespace ITTPTestWebApp.Network
{
    partial class Server
    {
        public void AddRedirects
        (
            bool needAuthorization,
            List<(string Method, string From, string To)> redirectInfos,
            List<string>? corsAllowedHeaders = null, 
            List<string>? permittedAddresses = null
        ) =>
        AddControllers
        (
            redirectInfos.Select(info =>
            (IRequestController) 
            new RedirectController
            (
                needAuthorization,
                info.Method, info.From, info.To,
                corsAllowedHeaders, permittedAddresses
            )).ToList()
        );
    }

    class RedirectController : IRequestController
    {
        #region fields 
        private readonly bool _NeedAuthorization = false;
        private readonly string _Method = string.Empty;
        private readonly string _From = string.Empty;
        private readonly string _To = string.Empty;
        private readonly List<string>? _CorsAllowedHeaders = null;
        private readonly List<string>? _PermittedAddresses = null;
        #endregion

        public RedirectController
        (
            bool needAuthorization,
            string method, string from, string to,
            List<string>? corsAllowedHeaders = null,
            List<string>? permittedAddresses = null
        ) 
        {
            _NeedAuthorization = needAuthorization;
            _Method = method.ToUpper(); _From = from; _To = to; 
            _CorsAllowedHeaders = corsAllowedHeaders; 
            _PermittedAddresses = permittedAddresses; 
        }
        ~RedirectController() { }

        public void Register(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapOptionsExt(_From, _NeedAuthorization, _Method, _CorsAllowedHeaders);

            var handler = async (HttpContext context) =>
            {
                try
                {
                    if (_NeedAuthorization && context.User?.Identity?.IsAuthenticated != true)
                    { context.Response.StatusCode = StatusCodes.Status401Unauthorized; return; }

                    if (!CheckRemoteAddress(context, _PermittedAddresses))
                    { context.Response.StatusCode = StatusCodes.Status403Forbidden; return; }
                    context.Response.AddCorsHeaders(context.Request, _NeedAuthorization, _Method, _CorsAllowedHeaders);

                    context.Response.StatusCode = StatusCodes.Status307TemporaryRedirect;
                    context.Response.Headers["Location"] = _To;
                }
                catch { context.Response.StatusCode = StatusCodes.Status500InternalServerError; }
                finally { await Task.CompletedTask; }
            };

            _ = _Method switch
            {
                "GET" => endpoints.MapGet(_From, handler),
                "POST" => endpoints.MapPost(_From, handler),
                "PUT" => endpoints.MapPut(_From, handler),
                "DELETE" => endpoints.MapDelete(_From, handler),
                _ => null
            };
        }
    }
}