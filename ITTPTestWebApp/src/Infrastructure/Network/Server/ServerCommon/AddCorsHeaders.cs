using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace ITTPTestWebApp.Network
{
    static partial class ServerCommon
    {
        public static void AddCorsHeaders(this HttpResponse response, HttpRequest request, bool needAuthorization, string method, List<string>? allowedHeaders = null)
        {
            response.Headers["Access-Control-Allow-Origin"] = request.Headers["Origin"].FirstOrDefault() ?? $"{request.Scheme}://{request.Host}" ?? "*";
            response.Headers["Access-Control-Allow-Methods"] = method + ", OPTIONS";
            response.Headers["Access-Control-Allow-Credentials"] = "true";

            var headers = "Content-Type";
            if (needAuthorization) { headers += ", Authorization"; }
            if (allowedHeaders != null && allowedHeaders.Any()) { headers += ", " + string.Join(",", allowedHeaders); }

            response.Headers["Access-Control-Allow-Headers"] = headers;
        }
    }
}
