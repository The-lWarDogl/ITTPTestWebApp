using System.ComponentModel.DataAnnotations;
using ITTPTestWebApp.Network.RestBodyModels;

namespace ITTPTestWebApp.Services.UsersController.RevokeBodyModels
{
    public class ResponseRevokeBody
    {
        public ResponseBodyUser user { get; set; } = new ResponseBodyUser();
    }
}
