using System.Collections.Concurrent;

using ITTPTestWebApp.Common;
using ITTPTestWebApp.Data;
using ITTPTestWebApp.Events;
using ITTPTestWebApp.Initialization;
using ITTPTestWebApp.Logging;

namespace ITTPTestWebApp.Services.UsersController
{
    partial class UsersControllerService : Service, IInitializable
    {
        public static readonly UsersControllerService Instance = new UsersControllerService();

        #region fields 
        private readonly string _Url = Config.Instance.Read("ServerConnectionUrl");
        private ConcurrentDictionary<Guid, User> _Users = new ConcurrentDictionary<Guid, User>();
        #endregion

        private UsersControllerService() : base(ServicesType.UsersControllerService, App.Ct) {  }
        ~UsersControllerService() { Uninitialize(); }

        #region public methods
        public List<User> GetAllUsers() =>
            _Users.Values.ToList();
        public bool TryGetUserFromLogin(string login, out User? user)
        { user = _Users.Values.FirstOrDefault(u => u.Login == login); return user != null; }
        public bool TryGetUserFromId(Guid id, out User? user)
        {  user = _Users.ContainsKey(id) ? _Users[id] : null; return user != null; }

        public void Initialize() { Reflection.UseReflection(this); }
        public void Uninitialize() { Reflection.UnUseReflection(this); }

        [ServerEvent(Event.ResourceStart)]
        public void OnResourceStart()
        {
            try { _Users = DataManager.Instance.GetUsers(); CheckForAtLeastOneAdmin(); }
            catch (Exception ex) { Logger.Instance.Log(ex); }
        }

        [ServerEvent(Event.ResourceStop)]
        public void OnResourceStop() { }
        #endregion
    }
}
