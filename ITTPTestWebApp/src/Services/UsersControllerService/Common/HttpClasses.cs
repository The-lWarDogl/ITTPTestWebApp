using System.ComponentModel.DataAnnotations;

namespace ITTPTestWebApp.Services.UsersController
{
    public class ResponseBodyUser
    {
        [RegularExpression(@"^[A-Za-zА-Яа-яЁё]+$", ErrorMessage = "Name can only contain latin and russian letters")]
        public string name { get; set; } = string.Empty;
        [Range(0, 2)]
        public int gender { get; set; } = 2;
        public DateTime? birthday { get; set; } = null;
        public bool isActive { get; set; } = true;

        public static explicit operator ResponseBodyUser(User u) =>
            new ResponseBodyUser(){ name = u.Name, gender = u.Gender, birthday = u.Birthday, isActive = u.RevokedOn == null };
    }
}
