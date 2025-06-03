using System.Net;
using System.Security.Claims;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using static ITTPTestWebApp.Network.ServerCommon;

namespace ITTPTestWebApp.Adapters.AspNetCore.EndpointRouteBuilder
{
    static partial class MapExt
    {
        public static void MapOptionsExt(this IEndpointRouteBuilder endpoints, string path, bool needAuthorization, string method, List<string>? allowedHeaders = null) =>
            endpoints.MapMethods(path, ["OPTIONS"], async context =>
            {
                try
                {
                    context.Response.AddCorsHeaders(context.Request, needAuthorization, method, allowedHeaders);
                    context.Response.StatusCode = 204;
                }
                finally { await Task.CompletedTask; }
            }).ExcludeFromDescription();

        public static void MapPostExt
        (
            this IEndpointRouteBuilder endpoints,
            string path,
            bool needAuthorization,
            string contentType,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?)>> requestCheck,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>> requestProcessing,
            List<string>? allowedHeaders = null,
            List<string>? permittedAddresses = null
        ) =>
            endpoints.MapMethodsExt
            (
                path,
                "POST",
                needAuthorization,
                contentType,
                requestCheck,
                requestProcessing,
                allowedHeaders,
                permittedAddresses
            );

        public static void MapGetExt
        (
            this IEndpointRouteBuilder endpoints,
            string path,
            bool needAuthorization,
            string contentType,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?)>> requestCheck,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>> requestProcessing,
            List<string>? allowedHeaders = null,
            List<string>? permittedAddresses = null
        ) =>
            endpoints.MapMethodsExt(
                path,
                "GET",
                needAuthorization,
                contentType,
                requestCheck,
                requestProcessing,
                allowedHeaders,
                permittedAddresses
            );

        public static void MapPutExt
        (
            this IEndpointRouteBuilder endpoints,
            string path,
            bool needAuthorization,
            string contentType,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?)>> requestCheck,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>> requestProcessing,
            List<string>? allowedHeaders = null,
            List<string>? permittedAddresses = null
        ) =>
            endpoints.MapMethodsExt(
                path,
                "PUT",
                needAuthorization,
                contentType,
                requestCheck,
                requestProcessing,
                allowedHeaders,
                permittedAddresses
            );

        public static void MapDeleteExt
        (
            this IEndpointRouteBuilder endpoints,
            string path,
            bool needAuthorization,
            string contentType,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?)>> requestCheck,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>> requestProcessing,
            List<string>? allowedHeaders = null,
            List<string>? permittedAddresses = null
        ) =>
            endpoints.MapMethodsExt(
                path,
                "DELETE",
                needAuthorization,
                contentType,
                requestCheck,
                requestProcessing,
                allowedHeaders,
                permittedAddresses
            );

        public static void MapPatchExt
        (
            this IEndpointRouteBuilder endpoints,
            string path,
            bool needAuthorization,
            string contentType,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?)>> requestCheck,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>> requestProcessing,
            List<string>? allowedHeaders = null,
            List<string>? permittedAddresses = null
        ) =>
            endpoints.MapMethodsExt(
                path,
                "PATCH",
                needAuthorization,
                contentType,
                requestCheck,
                requestProcessing,
                allowedHeaders,
                permittedAddresses
            );

        public static void MapHeadExt
        (
            this IEndpointRouteBuilder endpoints,
            string path,
            bool needAuthorization,
            string contentType,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?)>> requestCheck,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>> requestProcessing,
            List<string>? allowedHeaders = null,
            List<string>? permittedAddresses = null
        ) =>
            endpoints.MapMethodsExt(
                path,
                "HEAD",
                needAuthorization,
                contentType,
                requestCheck,
                requestProcessing,
                allowedHeaders,
                permittedAddresses
            );
    }
}
