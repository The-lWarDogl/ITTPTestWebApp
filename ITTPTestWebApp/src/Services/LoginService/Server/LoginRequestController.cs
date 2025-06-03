using Microsoft.AspNetCore.Mvc;

using Newtonsoft.Json;

namespace ITTPTestWebApp.Services.Login
{
    [ApiController]
    [Route("api")]
    public partial class LoginRequestController : ControllerBase
    {
        #region fields 
        private static readonly JsonSerializerSettings SettingsIgnore = new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore };
        #endregion

        public LoginRequestController() { }
        ~LoginRequestController() { }
    }
}
