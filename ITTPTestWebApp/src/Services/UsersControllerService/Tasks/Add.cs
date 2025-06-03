using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Services.ServicesTask;

namespace ITTPTestWebApp.Services.UsersController
{
    partial class UsersControllerService
    {
        public async Task Add
        (
            Dictionary<string, object> parameters,
            TaskCompletionSource<object?> tcs
        )
        {
            (bool success, ResponseBodyUser? user, string? error) ret = (false, null, null);
            try
            {
                if (!ParametersСheck.ParametersKeyСheck(parameters, ServicesTaskType.UsersAdd, out string exceptionText))
                { throw new Exception(exceptionText); }

                try
                {
                    AddToUsers
                    (
                        performedBy: (string) parameters["performedBy"],
                        login: (string) parameters["login"],
                        password: (string) parameters["password"],
                        name: (string) parameters["name"],
                        gender: (int) parameters["gender"],
                        birthday: ParametersСheck.ParameterСheck<DateTime>(parameters, "birthday") ? (DateTime)parameters["birthday"] : null,
                        admin: (bool) parameters["admin"]
                    );
                }
                catch (Exception ex) { ret.success = false; ret.error = ex.Message; tcs.SetResult(ret); return; }

                ret.success = TryGetUserFromLogin((string)parameters["login"], out var user);
                if (ret.success)
                {
                    ret.user = new ResponseBodyUser()
                    {
                        name = user!.Name,
                        gender = user!.Gender,
                        birthday = user!.Birthday,
                        isActive = true,
                    };
                }
                tcs.SetResult(ret);
            }
            catch (Exception ex) { tcs.SetCanceled(); Logger.Instance.Log(ex); }
            finally { await Task.CompletedTask; }
        }
    }
}
