using System.ComponentModel.DataAnnotations;
using ITTPTestWebApp.Network.RestBodyModels;

namespace ITTPTestWebApp.Services.UsersController.ReadAllBodyModels
{
    public class ResponseReadAllBody
    {
        public List<ResponseBodyUser> users { get; set; } = new List<ResponseBodyUser>();
    }
}
