using System.ComponentModel.DataAnnotations;
using ITTPTestWebApp.Network.RestBodyModels;

namespace ITTPTestWebApp.Services.UsersController.RestoreBodyModels
{
    public class RequestRestoreBody
    {
        [Required]
        public string login { get; set; } = string.Empty;
    }

    public class ResponseRestoreBody
    {
        public ResponseBodyUser user { get; set; } = new ResponseBodyUser();
    }
}
