using System.ComponentModel.DataAnnotations;
using ITTPTestWebApp.Network.RestBodyModels;

namespace ITTPTestWebApp.Services.UsersController.CreateBodyModels
{
    public class RequestCreateBody
    {
        [Required]
        public string login { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Password can only contain latin letters and numbers")]
        public string password { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[A-Za-zА-Яа-яЁё]+$", ErrorMessage = "Name can only contain latin and russian letters")]
        public string name { get; set; } = string.Empty;
        [Range(0, 2)]
        public int gender { get; set; } = 2;
        public DateTime? birthday { get; set; } = null;
        public bool admin { get; set; } = false;
    }

    public class ResponseCreateBody
    {
        public ResponseBodyUser user { get; set; } = new ResponseBodyUser();
    }
}
