using ITTPTestWebApp.Services.Login;
using ITTPTestWebApp.Services.UsersController;


namespace ITTPTestWebApp.Initialization
{
    interface IInitializable
    {
        void Initialize();
        void Uninitialize();
    }

    public static class Initializing
    {
        //INFO Контроль порядка создания объектов
        public static void InitAll() 
        {
            LoginService.Instance.Initialize();
            UsersControllerService.Instance.Initialize();
        }
        public static void UninitAll()
        {
            LoginService.Instance.Uninitialize();
            UsersControllerService.Instance.Uninitialize();
        }
    }
}
