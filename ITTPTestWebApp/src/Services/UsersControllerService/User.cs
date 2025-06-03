using System.ComponentModel.DataAnnotations;

using Newtonsoft.Json;

using ITTPTestWebApp.Common;
using ITTPTestWebApp.Data.Pgsql.TableElements;

namespace ITTPTestWebApp.Services.UsersController
{
    public class User : ICloneable
    {
        [KeyProperty]
        public Guid Id { get; set; }
        [Required]
        public string Login { get; set; }
        [Required]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Password can only contain latin letters and numbers")]
        public string Password { get; set; } //TODO password hash
        [Required]
        [RegularExpression(@"^[A-Za-zА-Яа-яЁё]+$", ErrorMessage = "Name can only contain latin and russian letters")]
        public string Name { get; set; }
        /// <summary>
        /// 0 — женщина, 1 — мужчина, 2 — неизвестно
        /// </summary>
        [Range(0, 2)]
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; } = null;
        public bool Admin { get; set; }

        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime? RevokedOn { get; set; } = null;
        public string? RevokedBy { get; set; } = null;

        public User
        (
            Guid id, string login, string password, string name, int gender, DateTime? birthday, bool admin,
            DateTime createdOn, string createdBy, DateTime modifiedOn, string modifiedBy, DateTime? revokedOn = null, string? revokedBy = null
        )
        {
            Id = id; Login = login; Password = password; Name = name; Gender = gender; Birthday = birthday; Admin = admin;
            CreatedOn = createdOn; CreatedBy = createdBy; ModifiedOn = modifiedOn; ModifiedBy = modifiedBy; RevokedOn = revokedOn; RevokedBy = revokedBy;
        }

        public object Clone() =>
            new User(Id, Login, Password, Name, Gender, Birthday, Admin, 
                CreatedOn, CreatedBy, ModifiedOn, ModifiedBy, RevokedOn, RevokedBy);

        public static explicit operator User(UserTableElement te) =>
            new User(Guid.Parse(te.Id), te.Login, te.Password, te.Name, te.Gender, te.Birthday, te.Admin, 
                te.CreatedOn, te.CreatedBy, te.ModifiedOn, te.ModifiedBy, te.RevokedOn, te.RevokedBy);

        public static explicit operator UserTableElement(User u) =>
            new UserTableElement(u.Id.ToString(), u.Login, u.Password, u.Name, u.Gender, u.Birthday, u.Admin,
                u.CreatedOn, u.CreatedBy, u.ModifiedOn, u.ModifiedBy, u.RevokedOn, u.RevokedBy);

        public bool Equals(User user) =>
            user.Id == Id &&
            string.Equals(user.Login, Login, StringComparison.Ordinal) &&
            string.Equals(user.Password, Password, StringComparison.Ordinal) &&
            string.Equals(user.Name, Name, StringComparison.Ordinal) &&
            user.Gender == Gender &&
            user.Birthday == Birthday &&
            user.Admin == Admin &&
            user.RevokedOn == RevokedOn &&
            user.RevokedBy == RevokedBy;
    }
}
