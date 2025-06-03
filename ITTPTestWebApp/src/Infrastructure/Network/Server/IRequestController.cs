using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Routing;

namespace ITTPTestWebApp.Network
{
    public interface IRequestController
    {
        void Register(IEndpointRouteBuilder endpoints);
    }
}
