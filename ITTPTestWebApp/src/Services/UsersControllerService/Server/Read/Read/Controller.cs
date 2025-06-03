using System.Diagnostics;
using System.Net;
using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

using Newtonsoft.Json;

using ITTPTestWebApp.Adapters.AspNetCore.Controller;
using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Network.RestBodyModels;
using ITTPTestWebApp.Services.ServicesTask;
using ITTPTestWebApp.Services.UsersController.ReadBodyModels;

namespace ITTPTestWebApp.Services.UsersController
{
    public partial class UsersRequestController
    {
        [SwaggerOperation(Summary = "Чтение пользователя", Tags = new[] { "UsersController" })]
        [HttpGet("read")]
        [Produces("application/json")]
        [SwaggerResponse(200, "User readed.", typeof(ResponseReadBody))]
        [SwaggerResponse(400, "Bad request: missing parameters.", typeof(ResponseErrorBody))]
        [SwaggerResponse(403, "Forbidden: access denied.", typeof(ResponseErrorBody))]
        [SwaggerResponse(500, "Internal server error.", typeof(ResponseErrorBody))]
        [Authorize]
        public async Task<IActionResult> Read([FromQuery] string? login = null) =>
            await RequestControllerExt.ExecuteAsync
                (
                    controller: this,
                    needAuthorization: true,
                    "application/json",
                    CheckRead,
                    HandleRead
                );

        private async Task<(HttpStatusCode, string?)>
        CheckRead
        (
            string? stringBody,
            string stringQueryParams,
            List<Claim> claims,
            Dictionary<string, string> headersIn,
            Dictionary<string, Cookie> cookiesIn
        )
        {
            var res400 = JsonConvert.SerializeObject(new ResponseErrorBody()
            { error = "Bad request: missing parameters." }, Formatting.Indented);
            var res403 = JsonConvert.SerializeObject(new ResponseErrorBody()
            { error = "Forbidden: access denied." }, Formatting.Indented);
            var res403notAdmin = JsonConvert.SerializeObject(new ResponseErrorBody()
            { error = "Forbidden: “login” query parameter is AdminOnly. You can do not use it at all to read yourself." }, Formatting.Indented);

            try
            {
                Dictionary<string, string>? request;
                try { request = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringQueryParams); }
                catch { return (HttpStatusCode.BadRequest, res400); }
                if (request == null) return (HttpStatusCode.BadRequest, res400);

                Guid id = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
                if (!UsersControllerService.Instance.TryGetUserFromId(id, out var user) || user!.RevokedOn != null)
                { return (HttpStatusCode.Forbidden, res403); }

                if (!user!.Admin && request.ContainsKey("login"))
                { return (HttpStatusCode.Forbidden, res403notAdmin); }

                return (HttpStatusCode.OK, null);
            }
            finally { await Task.CompletedTask; }
        }

        private async Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>
        HandleRead
        (
            string? stringBody,
            string stringQueryParams,
            List<Claim> claims,
            Dictionary<string, string> headersIn,
            Dictionary<string, Cookie> cookiesIn
        )
        {
            try
            {
                var request = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringQueryParams!)!;
                Guid id = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
                UsersControllerService.Instance.TryGetUserFromId(id, out var user);

                if (request.ContainsKey("login")) { UsersControllerService.Instance.TryGetUserFromLogin(request["login"], out user); }

                if (user != null)
                {
                    return
                    (
                        HttpStatusCode.OK,
                        JsonConvert.SerializeObject(new ResponseReadBody()
                        { user = (ResponseBodyUser) user }, Formatting.Indented),
                        null,
                        null
                    );
                }
                else
                {
                    return
                    (
                        HttpStatusCode.BadRequest,
                        JsonConvert.SerializeObject(new ResponseErrorBody()
                        { error = "User not found." }, Formatting.Indented),
                        null,
                        null
                    );
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log(ex);
                return
                (
                    HttpStatusCode.InternalServerError,
                    JsonConvert.SerializeObject(new ResponseErrorBody()
                    { error = ex.Message }, Formatting.Indented),
                    null,
                    null
                );
            }
            finally { await Task.CompletedTask; }
        }
    }
}
