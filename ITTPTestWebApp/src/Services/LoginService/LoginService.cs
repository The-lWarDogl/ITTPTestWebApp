using ITTPTestWebApp.Common;
using ITTPTestWebApp.Data;
using ITTPTestWebApp.Events;
using ITTPTestWebApp.Initialization;

namespace ITTPTestWebApp.Services.Login
{
    partial class LoginService : Service, IInitializable
    {
        public static readonly LoginService Instance = new LoginService();

        #region fields 
        private readonly string _Url = Config.Instance.Read("ServerConnectionUrl");

        private readonly string _JwtIssuer = Config.Instance.Read("Jwt_Issuer");
        private readonly string _JwtAudience = Config.Instance.Read("Jwt_Audience");
        private readonly string _JwtSecretKey = Config.Instance.Read("Jwt_SecretKey");
        #endregion

        private LoginService() : base(ServicesType.LoginService, App.Ct) { }
        ~LoginService() { Uninitialize(); }

        #region public methods
        public void Initialize() { Reflection.UseReflection(this); }
        public void Uninitialize() { Reflection.UnUseReflection(this); }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart() { }

        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop() { }

        #endregion
    }
}
