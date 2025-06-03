using ITTPTestWebApp.Logging;

namespace ITTPTestWebApp.Services.UsersController
{
    partial class UsersControllerService
    {
        private bool TryAddToUsers
        (
            string performedBy,
            string login,
            string password,
            string name,
            int gender,
            DateTime? birthday,
            bool admin
        )
        {
            try { AddToUsers(performedBy, login, password, name, gender, birthday, admin); return true; }
            catch { return false; }
        }

        private void AddToUsers
        (
            string performedBy,
            string login,
            string password,
            string name,
            int gender,
            DateTime? birthday,
            bool admin
        )
        {
            if (_Users.Values.Any(u => string.Equals(u.Login, login, StringComparison.OrdinalIgnoreCase)))
            { throw new InvalidOperationException("Login is already taken."); }

            Guid nextId = Guid.NewGuid();
            var nowUtc = DateTime.UtcNow;

            bool success = _Users.TryAdd
            (
                nextId,
                new User
                (
                    id: nextId,
                    login: login,
                    password: password,
                    name: name,
                    gender: gender,
                    birthday: birthday,
                    admin: admin,
                    createdOn: nowUtc,
                    createdBy: performedBy,
                    modifiedOn: nowUtc,
                    modifiedBy: performedBy,
                    revokedOn: null,
                    revokedBy: null
                )
            );
            if (success) { Logger.Instance.Log($"New user ({nextId}:{login}) was created.", tag: "users"); }
            else { throw new InvalidOperationException("User can not be created."); }
        }

        private bool TryRemoveFromUsers(string login)
        {
            try { RemoveFromUsers(login); return true; }
            catch { return false; }
        }

        private void RemoveFromUsers(string login)
        {
            if (!TryGetUserFromLogin(login, out var user)) { throw new InvalidOperationException("User not found."); }
            else { _RemoveFromUsers(user!); }
        }

        private bool TryRemoveFromUsers(Guid id)
        {
            try { RemoveFromUsers(id); return true; }
            catch { return false; }
        }

        private void RemoveFromUsers(Guid id)
        {
            if (!TryGetUserFromId(id, out var user)) { throw new InvalidOperationException("User not found."); }
            else { _RemoveFromUsers(user!); }
        }

        private void _RemoveFromUsers(User user)
        {
            if
            (
                user!.Admin &&
                user!.RevokedOn == null && 
                _Users.Values.Count(u => u.Admin && u.RevokedOn == null) <= 1
            )
            { throw new InvalidOperationException("The last active admin cannot be removed."); }

            bool removed = _Users.TryRemove(user.Id, out _);
            if (removed) { Logger.Instance.Log($"User ({user.Id}:{user.Login}) was permanently removed.", tag: "users"); }
            else { throw new InvalidOperationException("User can not be removed."); }
        }

        private bool TryUpdateUser
        (
            string performedBy,
            string currentLogin,
            string? newLogin,
            string? newPassword,
            string? newName,
            int? newGender,
            DateTime? newBirthday,
            bool? newAdmin,
            bool? revoke
        )
        {
            try { UpdateUser(performedBy, currentLogin, newLogin, newPassword, newName, newGender, newBirthday, newAdmin, revoke); return true; }
            catch { return false; }
        }

        private void UpdateUser
        (
            string performedBy,
            string currentLogin,
            string? newLogin,
            string? newPassword,
            string? newName,
            int? newGender,
            DateTime? newBirthday,
            bool? newAdmin,
            bool? revoke
        )
        {
            if (!TryGetUserFromLogin(currentLogin, out var user)) { throw new InvalidOperationException("User not found."); }
            else { _UpdateUser(performedBy, user!, newLogin, newPassword, newName, newGender, newBirthday, newAdmin, revoke); }
        }

        private bool TryUpdateUser
        (
            string performedBy,
            Guid id,
            string? newLogin,
            string? newPassword,
            string? newName,
            int? newGender,
            DateTime? newBirthday,
            bool? newAdmin,
            bool? revoke
        )
        {
            try { UpdateUser(performedBy, id, newLogin, newPassword, newName, newGender, newBirthday, newAdmin, revoke); return true; }
            catch { return false; }
        }

        private void UpdateUser
        (
            string performedBy,
            Guid id,
            string? newLogin,
            string? newPassword,
            string? newName,
            int? newGender,
            DateTime? newBirthday,
            bool? newAdmin,
            bool? revoke
        )
        {
            if (!TryGetUserFromId(id, out var user)) { throw new InvalidOperationException("User not found."); }
            else { _UpdateUser(performedBy, user!, newLogin, newPassword, newName, newGender, newBirthday, newAdmin, revoke); }
        }

        private void _UpdateUser
        (
            string performedBy,
            User user,
            string? newLogin,
            string? newPassword,
            string? newName,
            int? newGender,
            DateTime? newBirthday,
            bool? newAdmin,
            bool? revoke
        )
        {
            if (user!.Admin && _Users.Values.Count(u => u.Admin && u.RevokedOn == null) <= 1)
            {
                if (newAdmin.HasValue && !newAdmin.Value)
                { throw new InvalidOperationException("It is not possible to remove admin rights from the last active admin."); }
                if (revoke.HasValue && revoke.Value)
                { throw new InvalidOperationException("The last active admin cannot be revoked."); }
            }

            var nowUtc = DateTime.UtcNow;

            var updatedUser = new User
            (
                id: user.Id,
                login: 
                    (!string.IsNullOrEmpty(newLogin) && !_Users.Values.Any(u => string.Equals(u.Login, newLogin, StringComparison.OrdinalIgnoreCase)))
                    ? newLogin! 
                    : user.Login,
                password: !string.IsNullOrEmpty(newPassword) ? newPassword! : user.Password,
                name: !string.IsNullOrEmpty(newName) ? newName! : user.Name,
                gender: newGender.HasValue ? newGender.Value : user.Gender,
                birthday: newBirthday.HasValue ? newBirthday : user.Birthday,
                admin: newAdmin.HasValue ? newAdmin.Value : user.Admin,

                createdOn: user.CreatedOn,
                createdBy: user.CreatedBy,

                modifiedOn: nowUtc,
                modifiedBy: performedBy,
                
                revokedOn:
                    revoke.HasValue
                    ? revoke.Value 
                        ? nowUtc
                        :null
                    : user.RevokedOn,
                revokedBy:
                    revoke.HasValue
                    ? revoke.Value
                        ? performedBy
                        : null
                    : user.RevokedBy
            );

            if(updatedUser.Equals(user))
            { throw new InvalidOperationException("No data was updated."); }

            bool updated = _Users.TryUpdate(user.Id, updatedUser, user);
            if (updated) { Logger.Instance.Log($"User ({user.Id}:{user.Login}) was updated by {performedBy}.", tag: "users"); }
            else { throw new InvalidOperationException("User can not be updateed."); }
        }
    }
}
