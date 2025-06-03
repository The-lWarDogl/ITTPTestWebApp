using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Common;

namespace ITTPTestWebApp.Services.UsersController
{
    partial class UsersControllerService
    {
        private void CheckForAtLeastOneAdmin()
        {
            if (!_Users.Values.Where(u => u.Admin && u.RevokedOn == null).Any())
            {
                string login = $"admin-{Guid.NewGuid().ToString().Substring(0, 8)}";
                string password = Func.GenerateRandomPassword
                    (
                        chars: "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789",
                        length: 8
                    );

                bool adminAdded = TryAddToUsers
                    (
                        performedBy: "system",
                        login: login,
                        password: password,
                        name: "admin",
                        gender: 2,
                        birthday: null,
                        admin: true
                    );

                if (adminAdded) { Logger.Instance.Log($"New admin was created (login: {login} | password: {password}). This message will not be written to files.", tag: "users", writeToFile: false); }
                else { throw new InvalidOperationException("Failed to automatically create 'system' admin."); }
            }
        }
    }
}
