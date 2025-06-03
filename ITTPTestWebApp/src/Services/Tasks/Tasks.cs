namespace ITTPTestWebApp.Services.ServicesTask
{
    enum ServicesTaskType
    {
        None = 0,
        Login,
        UsersAdd,
        UsersRemove,
        UsersUpdate
    }

    class NoneServiceTask : ServiceTaskBase
    {
        public NoneServiceTask() :
            base(ServicesTaskType.None, ServicesType.None, async (p1, p2) => { await Task.CompletedTask; }, null, null)
        { }
    }

    class LoginServiceTask : ServiceTaskBase
    {
        public LoginServiceTask(Dictionary<string, object> parameters) :
            base(ServicesTaskType.Login, ServicesType.LoginService, Login.LoginService.Instance.Login, null, parameters)
        { }
    }

    class UsersAddServiceTask : ServiceTaskBase
    {
        public UsersAddServiceTask(Dictionary<string, object> parameters) :
            base(ServicesTaskType.UsersAdd, ServicesType.UsersControllerService, UsersController.UsersControllerService.Instance.Add, null, parameters)
        { }
    }

    class UsersRemoveServiceTask : ServiceTaskBase
    {
        public UsersRemoveServiceTask(Dictionary<string, object> parameters) :
            base(ServicesTaskType.UsersRemove, ServicesType.UsersControllerService, UsersController.UsersControllerService.Instance.Remove, null, parameters)
        { }
    }

    class UsersUpdateServiceTask : ServiceTaskBase
    {
        public UsersUpdateServiceTask(Dictionary<string, object> parameters) :
            base(ServicesTaskType.UsersUpdate, ServicesType.UsersControllerService, UsersController.UsersControllerService.Instance.Update, null, parameters)
        { }
    }
}
