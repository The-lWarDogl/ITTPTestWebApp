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
using ITTPTestWebApp.Services.UsersController.UpdateBodyModels;

namespace ITTPTestWebApp.Services.UsersController
{
    public partial class UsersRequestController
    {
        [SwaggerOperation(Summary = "Обновление пользователя", Tags = new[] { "UsersController" })]
        [HttpPut("update")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [SwaggerResponse(200, "User updated.", typeof(ResponseUpdateBody))]
        [SwaggerResponse(400, "Bad request: missing parameters.", typeof(ResponseErrorBody))]
        [SwaggerResponse(403, "Forbidden: access denied.", typeof(ResponseErrorBody))]
        [SwaggerResponse(500, "Internal server error.", typeof(ResponseErrorBody))]
        [Authorize]
        public async Task<IActionResult> Update([FromBody] RequestUpdateBody request) =>
            await RequestControllerExt.ExecuteAsync
                (
                    controller: this,
                    needAuthorization: true,
                    "application/json",
                    CheckUpdate,
                    HandleUpdate
                );

        private async Task<(HttpStatusCode, string?)>
        CheckUpdate
        (
            string? stringBody,
            string stringQueryParams,
            List<Claim> claims,
            Dictionary<string, string> headersIn,
            Dictionary<string, Cookie> cookiesIn
        )
        {
            var res400 = JsonConvert.SerializeObject(new ResponseErrorBody()
            { error = "Bad request: missing parameters" }, Formatting.Indented);
            var res403 = JsonConvert.SerializeObject(new ResponseErrorBody()
            { error = "Forbidden: access denied" }, Formatting.Indented);
            var res403notAdmin = JsonConvert.SerializeObject(new ResponseErrorBody()
            { error = "Forbidden: “currentLogin” and “admin” body fields is AdminOnly. You can do not use them at all to update yourself." }, Formatting.Indented);

            try
            {
                if (stringBody == null) return (HttpStatusCode.BadRequest, res400);

                RequestUpdateBody? request;
                try { request = JsonConvert.DeserializeObject<RequestUpdateBody>(stringBody, SettingsIgnore); }
                catch { return (HttpStatusCode.BadRequest, res400); }
                if (request == null) return (HttpStatusCode.BadRequest, res400);

                Guid id = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
                if  (!UsersControllerService.Instance.TryGetUserFromId(id, out var user) || user!.RevokedOn != null)
                { return (HttpStatusCode.Forbidden, res403); }

                if (!user!.Admin && request.currentLogin != null && request.admin != null)
                { return (HttpStatusCode.Forbidden, res403notAdmin); }

                return (HttpStatusCode.OK, null);
            }
            finally { await Task.CompletedTask; }
        }

        private async Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>
        HandleUpdate
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
                var request = JsonConvert.DeserializeObject<RequestUpdateBody>(stringBody!, SettingsIgnore)!;
                Guid id = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
                UsersControllerService.Instance.TryGetUserFromId(id, out var user);

                Dictionary<string, object> parameters = new() { { "performedBy", user!.Login } };
                if (request.currentLogin != null) { parameters.Add("currentLogin", request.currentLogin); }
                else { parameters.Add("currentLogin", user!.Login); }
                if (request.login != null) { parameters.Add("newLogin", request.login); }
                if (request.password != null) { parameters.Add("newPassword", request.password); }
                if (request.name != null) { parameters.Add("newName", request.name); }
                if (request.gender != null) { parameters.Add("newGender", request.gender); }
                if (request.birthday != null) { parameters.Add("newBirthday", request.birthday); }
                if (request.admin != null) { parameters.Add("newAdmin", request.admin); }
                var task = new UsersUpdateServiceTask(parameters);
                ServicesTaskManager.Instance.TryAdd(task);
                (bool success, ResponseBodyUser? user, string? error) ret = ((bool, ResponseBodyUser?, string?))(await task.TCS.Task)!;

                if (ret.success)
                {
                    return
                    (
                        HttpStatusCode.OK,
                        JsonConvert.SerializeObject(new ResponseUpdateBody()
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
