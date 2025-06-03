using System.Net;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Http;

using Newtonsoft.Json;

using static ITTPTestWebApp.Network.ServerCommon;

namespace ITTPTestWebApp.Adapters.AspNetCore
{
    public static class RequestExecutorCore
    {
        public static async Task<(HttpStatusCode,  string?)>
        Execute
        (
            HttpRequest request,
            HttpResponse response,
            bool needAuthorization,
            string method,
            string contentType,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?)>> requestCheck,
            Func<string?, string, List<Claim>, Dictionary<string, string>, Dictionary<string, Cookie>,
                Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>> requestProcessing,
            List<string>? allowedHeaders = null,
            List<string>? permittedAddresses = null
        )
        {
            if (!CheckRemoteAddress(request.HttpContext, permittedAddresses))
            { return (HttpStatusCode.Forbidden, null); }
            response.AddCorsHeaders(request, needAuthorization, method, allowedHeaders);

            request.EnableBuffering();
            request.Body.Position = 0;
            string? body = null;
            using (var reader = new StreamReader(request.Body, Encoding.UTF8, leaveOpen: true))
            {
                body = await reader.ReadToEndAsync();
                request.Body.Position = 0;
            }
            request.Body.Position = 0;

            string queryParams = JsonConvert.SerializeObject
                (
                    request.Query.ToDictionary
                    (kvp => kvp.Key, kvp => kvp.Value.ToString())
                );

            var claims = request.HttpContext.User.Claims.ToList();

            var headersIn = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            var cookiesIn = ParseCookies(request.Headers["Cookie"]);

            var (checkCode, checkRes) = await requestCheck(body, queryParams, claims, headersIn, cookiesIn);
            if (checkCode != HttpStatusCode.OK && checkCode != HttpStatusCode.NoContent)
            { return (checkCode, checkRes); }

            var (code, res, headersOut, cookiesOut) = await requestProcessing(body, queryParams, claims, headersIn, cookiesIn);

            response.ContentType = GetSmartContentType(contentType);

            if (headersOut != null && headersOut.Any())
            { foreach (var (key, value) in headersOut) { response.Headers[key] = value; } }
            if (cookiesOut != null && cookiesOut.Any())
            { AppendCookies(response, cookiesOut); }

            return (code, res);
        }

        public static Dictionary<string, Cookie> ParseCookies(string? cookieHeader)
        {
            var result = new Dictionary<string, Cookie>();
            if (string.IsNullOrWhiteSpace(cookieHeader)) return result;

            var cookiePairs = cookieHeader.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var pair in cookiePairs)
            {
                var parts = pair.Split('=', 2);
                if (parts.Length != 2) continue;

                var name = parts[0].Trim();
                var value = parts[1].Trim();
                result[name] = new Cookie(name, value);
            }

            return result;
        }

        public static void AppendCookies(HttpResponse response, Dictionary<string, Cookie> cookies)
        {
            foreach (var (_, cookie) in cookies)
            {
                response.Cookies.Append(cookie.Name, cookie.Value, new CookieOptions
                {
                    HttpOnly = cookie.HttpOnly,
                    Secure = cookie.Secure,
                    Domain = string.IsNullOrWhiteSpace(cookie.Domain) ? null : cookie.Domain,
                    Path = string.IsNullOrWhiteSpace(cookie.Path) ? "/" : cookie.Path,
                    Expires = cookie.Expires == DateTime.MinValue ? null : cookie.Expires,
                    SameSite = SameSiteMode.Unspecified
                });
            }
        }
    }
}
