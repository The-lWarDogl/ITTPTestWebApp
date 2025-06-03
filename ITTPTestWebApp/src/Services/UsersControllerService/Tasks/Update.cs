using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Services.ServicesTask;

namespace ITTPTestWebApp.Services.UsersController
{
    partial class UsersControllerService
    {
        public async Task Update
        (
            Dictionary<string, object> parameters,
            TaskCompletionSource<object?> tcs
        )
        {
            (bool success, ResponseBodyUser user, string? error) ret = (false, new ResponseBodyUser(), null);
            try
            {
                if (!ParametersСheck.ParametersKeyСheck(parameters, ServicesTaskType.UsersUpdate, out string exceptionText))
                { throw new Exception(exceptionText); }

                string? newLogin = ParametersСheck.ParameterСheck<string>(parameters, "newLogin") ? (string)parameters["newLogin"] : null;
                try
                {
                    UpdateUser
                    (
                        performedBy: (string)parameters["performedBy"],
                        currentLogin: (string)parameters["currentLogin"],
                        newLogin: newLogin,
                        newPassword: ParametersСheck.ParameterСheck<string>(parameters, "newPassword") ? (string)parameters["newPassword"] : null,
                        newName: ParametersСheck.ParameterСheck<string>(parameters, "newName") ? (string)parameters["newName"] : null,
                        newGender: ParametersСheck.ParameterСheck<int>(parameters, "newGender") ? (int)parameters["newGender"] : null,
                        newBirthday: ParametersСheck.ParameterСheck<DateTime>(parameters, "newBirthday") ? (DateTime) parameters["newBirthday"] : null,
                        newAdmin: ParametersСheck.ParameterСheck<bool>(parameters, "newAdmin") ? (bool)parameters["newAdmin"] : null,
                        revoke: ParametersСheck.ParameterСheck<bool>(parameters, "revoke") ? (bool)parameters["revoke"] : null
                    );
                }
                catch (Exception ex) { ret.success = false; ret.error = ex.Message; tcs.SetResult(ret); return; }

                TryGetUserFromLogin(newLogin ?? (string)parameters["currentLogin"], out var user);

                ret.success = true;
                ret.user = new ResponseBodyUser()
                {
                    name = user!.Name,
                    gender = user!.Gender,
                    birthday = user!.Birthday,
                    isActive = user!.RevokedOn == null,
                };
                tcs.SetResult(ret);
            }
            catch (Exception ex) { tcs.SetCanceled(); Logger.Instance.Log(ex); }
            finally { await Task.CompletedTask; }
        }
    }
}
