using System.Net;

using System.Text;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.IdentityModel.Tokens;

using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Services.ServicesTask;
using ITTPTestWebApp.Services.UsersController;

namespace ITTPTestWebApp.Services.Login
{
    partial class LoginService
    {
        public async Task Login
        (
            Dictionary<string, object> parameters,
            TaskCompletionSource<object?> tcs
        )
        {
            (bool success, string token, DateTime expires) ret = (false, string.Empty, DateTime.UtcNow);
            (string id, string login) userdata = (string.Empty, string.Empty);
            try
            {
                if (!ParametersСheck.ParametersKeyСheck(parameters, ServicesTaskType.Login, out string exceptionText))
                { throw new Exception(exceptionText); }

                if (!TryValidate((string)parameters["login"], (string)parameters["password"], out string id))
                { ret.success = false; tcs.SetResult(ret); return; }
                userdata.id = id; userdata.login = (string)parameters["login"];

                ret.success = true;

                var expires = DateTime.UtcNow.AddHours(1); 
                ret.expires = expires;
                var tokenDescriptor = new JwtSecurityToken
                (
                    issuer: _JwtIssuer,
                    audience: _JwtAudience,
                    claims: new List<Claim> { new Claim(JwtRegisteredClaimNames.Sub, id) },
                    expires: expires,
                    signingCredentials: new SigningCredentials
                    (
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSecretKey)),
                        SecurityAlgorithms.HmacSha256
                    )
                );
                ret.token = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

                tcs.SetResult(ret);
            }
            catch (Exception ex) { tcs.SetCanceled(); Logger.Instance.Log(ex); }
            finally
            {
                await Task.CompletedTask;
                if (ret.success) { Logger.Instance.Log($"User ({userdata.id}:{userdata.login}) was logined via api.", tag: "users"); }
            }
        }

        private bool TryValidate(string login, string password, out string id)
        {
            if (!UsersControllerService.Instance.TryGetUserFromLogin(login, out var user) || user!.RevokedOn != null) { id = "0"; return false; }
            else { id = user!.Id.ToString(); return user!.Password == password; }
        }
    }
}
