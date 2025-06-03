using System.ComponentModel.DataAnnotations;
using ITTPTestWebApp.Network.RestBodyModels;

namespace ITTPTestWebApp.Services.UsersController.DeleteBodyModels
{
    public class ResponseDeleteBody
    {
        public ResponseBodyUser user { get; set; } = new ResponseBodyUser();
    }
}
