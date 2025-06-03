using System.ComponentModel.DataAnnotations;
using ITTPTestWebApp.Network.RestBodyModels;

namespace ITTPTestWebApp.Services.Login.LoginBodyModels
{
    public class RequestLoginBody
    {
        [Required]
        public string login { get; set; } = string.Empty;
        [Required]
        [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "Password can only contain latin letters and numbers")]
        public string password { get; set; } = string.Empty;
    }

    public class ResponseLoginBody
    {
        public string token { get; set; } = string.Empty;
        public DateTime expires { get; set; } = DateTime.MinValue;
    }
}
