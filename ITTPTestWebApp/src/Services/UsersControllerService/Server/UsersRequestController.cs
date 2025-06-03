using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

using Newtonsoft.Json;

namespace ITTPTestWebApp.Services.UsersController
{
    [ApiController]
    [Route("api/users")]
    public partial class UsersRequestController : ControllerBase
    {
        #region fields
        private static readonly JsonSerializerSettings SettingsIgnore = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
        #endregion

        public UsersRequestController() { }
        ~UsersRequestController() { }
    }
}
