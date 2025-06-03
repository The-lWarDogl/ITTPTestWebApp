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
using ITTPTestWebApp.Services.UsersController.ReadAllBodyModels;

namespace ITTPTestWebApp.Services.UsersController
{
    public partial class UsersRequestController
    {
        [SwaggerOperation(Summary = "(AdminOnly) Чтение всех пользователей", Tags = new[] { "UsersController" })]
        [HttpGet("readall")]
        [Produces("application/json")]
        [SwaggerResponse(200, "Users readed.", typeof(ResponseReadAllBody))]
        [SwaggerResponse(400, "Bad request: missing parameters.", typeof(ResponseErrorBody))]
        [SwaggerResponse(403, "Forbidden: access denied.", typeof(ResponseErrorBody))]
        [SwaggerResponse(500, "Internal server error.", typeof(ResponseErrorBody))]
        [Authorize]
        public async Task<IActionResult> ReadAll([FromQuery] bool? activeOnly = null, [FromQuery] int? minAgeYears = null, [FromQuery] int? maxAgeYears = null) =>
            await RequestControllerExt.ExecuteAsync
                (
                    controller: this,
                    needAuthorization: true,
                    "application/json",
                    CheckReadAll,
                    HandleReadAll
                );

        private async Task<(HttpStatusCode, string?)>
        CheckReadAll
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
                if (request == null) return (HttpStatusCode.BadRequest, res400);

                Guid id = Guid.Parse(claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)!.Value);
                if (!UsersControllerService.Instance.TryGetUserFromId(id, out var user) || user!.RevokedOn != null || !user!.Admin)
                { return (HttpStatusCode.Forbidden, res403); }

                return (HttpStatusCode.OK, null);
            }
            finally { await Task.CompletedTask; }
        }

        private async Task<(HttpStatusCode, string?, Dictionary<string, string>?, Dictionary<string, Cookie>?)>
        HandleReadAll
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
                var request = JsonConvert.DeserializeObject<Dictionary<string, string>>(stringQueryParams)!;

                var users = UsersControllerService.Instance.GetAllUsers()
                    .OrderBy(u => u.CreatedOn)
                    .Select(u => (ResponseBodyUser) u).ToList();
                if (request.ContainsKey("activeOnly") && bool.Parse(request["activeOnly"])) { users = users.Where(u => u.isActive).ToList(); }
                DateTime utcNow = DateTime.UtcNow;
                if (request.ContainsKey("minAgeYears"))
                { users = users.Where(u => u.birthday.HasValue && u.birthday.Value.Date <= utcNow.AddYears(-int.Parse(request["minAgeYears"]))).ToList(); }
                if (request.ContainsKey("maxAgeYears"))
                { users = users.Where(u => u.birthday.HasValue && u.birthday.Value.Date >= utcNow.AddYears(-int.Parse(request["maxAgeYears"]))).ToList(); }

                return 
                (
                    HttpStatusCode.OK,
                    JsonConvert.SerializeObject(new ResponseReadAllBody()
                    { users = users }, Formatting.Indented),
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
