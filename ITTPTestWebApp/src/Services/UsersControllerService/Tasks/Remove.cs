using ITTPTestWebApp.Services.ServicesTask;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using ITTPTestWebApp.Logging;

namespace ITTPTestWebApp.Services.UsersController
{
    partial class UsersControllerService
    {
        public async Task Remove
        (
            Dictionary<string, object> parameters,
            TaskCompletionSource<object?> tcs
        )
        {
            (bool success, ResponseBodyUser user, string? error) ret = (false, new ResponseBodyUser(), null);
            try
            {
                if (!ParametersСheck.ParametersKeyСheck(parameters, ServicesTaskType.UsersRemove, out string exceptionText))
                { throw new Exception(exceptionText); }

                TryGetUserFromLogin((string)parameters["login"], out var user);

                try { RemoveFromUsers(login: (string)parameters["login"]); }
                catch (Exception ex) { ret.success = false; ret.error = ex.Message; tcs.SetResult(ret); return; }

                ret.success = true;
                ret.user = new ResponseBodyUser()
                {
                    name = user!.Name,
                    gender = user!.Gender,
                    birthday = user!.Birthday,
                    isActive = false,
                };
                tcs.SetResult(ret);
            }
            catch (Exception ex) { tcs.SetCanceled(); Logger.Instance.Log(ex); }
            finally { await Task.CompletedTask; }
        }
    }
}
