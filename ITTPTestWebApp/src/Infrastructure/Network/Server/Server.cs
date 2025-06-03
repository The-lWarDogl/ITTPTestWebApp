using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;

using ITTPTestWebApp.Data;

namespace ITTPTestWebApp.Network
{
    partial class Server
    {
        public static readonly Server Instance = new Server();

        #region fields 
        private readonly CancellationToken _Ct = App.Ct;
        private readonly List<IRequestController> _Controllers = new();

        private readonly string _JwtIssuer = Config.Instance.Read("Jwt_Issuer");
        private readonly string _JwtAudience = Config.Instance.Read("Jwt_Audience");
        private readonly string _JwtSecretKey = Config.Instance.Read("Jwt_SecretKey");

        private WebApplication? _App = null;
        #endregion

        private Server() { }
        ~Server() { }

        public void Start()
        { if (TryInitHost()) { _App!.StartAsync(_Ct); } }

        public void Stop()
        { if (_App != null) { _App.StopAsync(_Ct); } }
    }
}
