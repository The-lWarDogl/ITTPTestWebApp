namespace ITTPTestWebApp.Services
{
    enum ServicesType
    {
        None = 0,
        LoginService,
        UsersControllerService
    }

    abstract class Service
    {
        protected readonly ServicesType _ServiceType;
        protected readonly CancellationToken _Ct;
        protected Service(ServicesType serviceType, CancellationToken ct) { _ServiceType = serviceType; _Ct = ct; }
    }
}
