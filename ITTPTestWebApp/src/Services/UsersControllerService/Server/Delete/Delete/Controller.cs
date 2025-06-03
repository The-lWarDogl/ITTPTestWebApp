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
using ITTPTestWebApp.Services.UsersController.DeleteBodyModels;

namespace ITTPTestWebApp.Services.UsersController
{
    public partial class UsersRequestController
    {
        [SwaggerOperation(Summary = "(AdminOnly) Полное удаление пользователя", Tags = new[] { "UsersController" })]
        [HttpDelete("delete")]
        [Produces("application/json")]
        [SwaggerResponse(200, "User deleted.", typeof(ResponseDeleteBody))]
        [SwaggerResponse(400, "Bad request: missing parameters.", typeof(ResponseErrorBody))]
        [SwaggerResponse(403, "Forbidden: access denied.", typeof(ResponseErrorBody))]
        [SwaggerResponse(500, "Internal server error.", typeof(ResponseErrorBody))]
        [Authorize]
        public async Task<IActionResult> Delete([FromQuery] string login = "") =>
            await RequestControllerExt.ExecuteAsync
                (
                    controller: this,
                    needAuthorization: true,
                    "application/json",
                    CheckDelete,
                    HandleDelete
                );

        private async Task<(HttpStatusCode, string?)>
        CheckDelete
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
                Dictionary<string, string>? request;
                try { request = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringQueryParams); }
                catch { return (HttpStatusCode.BadRequest, res400); }
                if (request == null || !request.Any()) return (HttpStatusCode.BadRequest, res400);

                if (!request.ContainsKey("login") || string.IsNullOrEmpty(request["login"]))
                { return (HttpStatusCode.BadRequest, res400); }

                Guid id = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
                if (!UsersControllerService.Instance.TryGetUserFromId(id, out var user) || user!.RevokedOn != null || !user!.Admin)
                { return (HttpStatusCode.Forbidden, res403); }

                return (HttpStatusCode.OK, null);
            }
            finally { await Task.CompletedTask; }
        }

        private async Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>
        HandleDelete
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

                var task = new UsersRemoveServiceTask(new() { { "login", request["login"] } });
                ServicesTaskManager.Instance.TryAdd(task);
                (bool success, ResponseBodyUser? user, string? error) ret = ((bool, ResponseBodyUser?, string?))(await task.TCS.Task)!;

                if (ret.success)
                {
                    return
                    (
                        HttpStatusCode.OK,
                        JsonConvert.SerializeObject(new ResponseDeleteBody()
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
