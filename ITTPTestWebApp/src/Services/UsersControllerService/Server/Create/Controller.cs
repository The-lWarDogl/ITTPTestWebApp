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
using ITTPTestWebApp.Services.UsersController.CreateBodyModels;

namespace ITTPTestWebApp.Services.UsersController
{
    public partial class UsersRequestController
    {
        [SwaggerOperation(Summary = "(AdminOnly) Создание пользователя", Tags = new[] { "UsersController" })]
        [HttpPost("create")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [SwaggerResponse(200, "User created.", typeof(ResponseCreateBody))]
        [SwaggerResponse(400, "Bad request: missing parameters.", typeof(ResponseErrorBody))]
        [SwaggerResponse(403, "Forbidden: access denied.", typeof(ResponseErrorBody))]
        [SwaggerResponse(500, "Internal server error.", typeof(ResponseErrorBody))]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] RequestCreateBody request) =>
            await RequestControllerExt.ExecuteAsync
                (
                    controller: this,
                    needAuthorization: true,
                    "application/json",
                    CheckCreate,
                    HandleCreate
                );

        private async Task<(HttpStatusCode, string?)> 
        CheckCreate
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

            try
            {
                if (stringBody == null) return (HttpStatusCode.BadRequest, res400);

                RequestCreateBody? request;
                try { request = JsonConvert.DeserializeObject<RequestCreateBody>(stringBody, SettingsIgnore); }
                catch { return (HttpStatusCode.BadRequest, res400); }
                if (request == null) return (HttpStatusCode.BadRequest, res400);

                Guid id = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
                if (!UsersControllerService.Instance.TryGetUserFromId(id, out var user) || user!.RevokedOn != null || !user!.Admin)
                { return (HttpStatusCode.Forbidden, res403); }

                return (HttpStatusCode.OK, null);
            }
            finally { await Task.CompletedTask; }
        }

        private async Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>
        HandleCreate
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
                var request = JsonConvert.DeserializeObject<RequestCreateBody>(stringBody!, SettingsIgnore)!;
                Guid id = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
                UsersControllerService.Instance.TryGetUserFromId(id, out var user);

                Dictionary<string, object> parameters = new()
                {
                    { "performedBy", user!.Login },
                    { "login", request.login },
                    { "password", request.password },
                    { "name", request.name },
                    { "gender", request.gender },
                    { "admin", request.admin }
                };
                if (request.birthday != null) { parameters.Add("birthday", request.birthday); }
                var task = new UsersAddServiceTask(parameters);
                ServicesTaskManager.Instance.TryAdd(task);
                (bool success, ResponseBodyUser? user, string? error) ret = ((bool, ResponseBodyUser?, string?))(await task.TCS.Task)!;

                if(ret.success)
                {
                    return 
                    (
                        HttpStatusCode.OK,
                        JsonConvert.SerializeObject(new ResponseCreateBody()
                        { user = ret.user! }, Formatting.Indented),
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
                        { error = ret.error ?? "Bad request: missing parameters." }, Formatting.Indented), 
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
