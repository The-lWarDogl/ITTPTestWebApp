using System.ComponentModel.DataAnnotations;
using ITTPTestWebApp.Network.RestBodyModels;

namespace ITTPTestWebApp.Services.UsersController.ReadBodyModels
{
    public class ResponseReadBody
    {
        public ResponseBodyUser user { get; set; } = new ResponseBodyUser();
    }
}
