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
using ITTPTestWebApp.Services.Login.LoginBodyModels;

namespace ITTPTestWebApp.Services.Login
{
    public partial class LoginRequestController
    {
        [SwaggerOperation(Summary = "Вход за пользователя", Tags = new[] { "Login" })]
        [HttpPost("login")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [SwaggerResponse(200, "Logined", typeof(ResponseLoginBody))]
        [SwaggerResponse(400, "Bad request: missing parameters", typeof(ResponseErrorBody))]
        [SwaggerResponse(401, "Unauthorized: login and/or password incorrect", typeof(ResponseErrorBody))]
        [SwaggerResponse(500, "Internal server error", typeof(ResponseErrorBody))]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] RequestLoginBody request) =>
            await RequestControllerExt.ExecuteAsync
                (
                    controller: this,
                    needAuthorization: false,
                    "application/json",
                    CheckLogin,
                    HandleLogin
                );

        private async Task<(HttpStatusCode, string?)>
        CheckLogin
        (
            string? stringBody,
            string stringQueryParams,
            List<Claim> claims,
            Dictionary<string, string> headersIn,
            Dictionary<string, Cookie> cookiesIn
        )
        {
            var res400 = JsonConvert.SerializeObject(new ResponseErrorBody()
            { error = "Bad Request: missing parameters" }, Formatting.Indented, SettingsIgnore);

            try
            {
                if (stringBody == null) return (HttpStatusCode.BadRequest, res400);

                RequestLoginBody? request;
                try { request = JsonConvert.DeserializeObject<RequestLoginBody>(stringBody); }
                catch { return (HttpStatusCode.BadRequest, res400); }
                if (request == null) return (HttpStatusCode.BadRequest, res400);

                return (HttpStatusCode.OK, null);
            }
            finally { await Task.CompletedTask; }
        }

        private async Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>
        HandleLogin
        (
            string? stringBody,
            string stringQueryParams,
            List<Claim> claims,
            Dictionary<string, string> headersIn,
            Dictionary<string, Cookie> cookiesIn
        )
        {
            var error401 = JsonConvert.SerializeObject(new ResponseErrorBody()
            { error = "Unauthorized: login and/or password incorrect" }, Formatting.Indented, SettingsIgnore);

            try
            {
                var request = JsonConvert.DeserializeObject<RequestLoginBody>(stringBody!)!;

                var task = new LoginServiceTask(new ()
                {
                    { "login", request.login },
                    { "password", request.password }
                });
                ServicesTaskManager.Instance.TryAdd(task);
                (bool success, string token, DateTime expires) ret = ((bool, string, DateTime))(await task.TCS.Task)!;

                if (!ret.success) return (HttpStatusCode.Unauthorized, error401, null, null);

                return 
                (
                    HttpStatusCode.OK,
                    JsonConvert.SerializeObject(new ResponseLoginBody()
                    { token = ret.token, expires = ret.expires }, Formatting.Indented),
                    null,
                    null
                );
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
