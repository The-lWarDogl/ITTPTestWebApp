using System.ComponentModel.DataAnnotations;
using ITTPTestWebApp.Network.RestBodyModels;

namespace ITTPTestWebApp.Services.UsersController.UpdateBodyModels
{
    public class RequestUpdateBody
    {
        public string? currentLogin { get; set; } = null;
        public string? login { get; set; } = null;
        public string? password { get; set; } = null;
        public string? name { get; set; } = null;
        public int? gender { get; set; } = null;
        public DateTime? birthday { get; set; } = null;
        public bool? admin { get; set; } = null;
    }

    public class ResponseUpdateBody
    {
        public ResponseBodyUser user { get; set; } = new ResponseBodyUser();
    }
}
