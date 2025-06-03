using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITTPTestWebApp.Network
{
    partial class Server
    {
        public void AddControllers(List<IRequestController> controllers) =>
            _Controllers.AddRange(controllers);
    }
}
