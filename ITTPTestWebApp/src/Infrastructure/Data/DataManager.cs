using System.Collections.Concurrent;

using ITTPTestWebApp.Common;
using ITTPTestWebApp.Data.Pgsql;
using ITTPTestWebApp.Data;
using ITTPTestWebApp.Services.UsersController;
using ITTPTestWebApp.Services.ServicesTask;
using ITTPTestWebApp.Data.Pgsql.TableElements;

namespace ITTPTestWebApp.Data
{
    partial class DataManager
    {
        public static readonly DataManager Instance = new DataManager();

        #region fields
        private readonly DBPgsqlContext _DBPgsqlContext;

        private PeriodicTableSynchronizedDictionary<Guid, ServiceTaskBase, ServiceTaskTableElement> _ServiceTasks;
        private PeriodicTableSynchronizedDictionary<Guid, User, UserTableElement> _Users;
        #endregion

        private DataManager() 
        { 
            _DBPgsqlContext = new DBPgsqlContext(Config.Instance.Read("DBConnectionString"));
            _ServiceTasks = new(_DBPgsqlContext.ServiceTasks, TimeSpan.FromMinutes(2));
            _Users = new(_DBPgsqlContext.Users, TimeSpan.FromMinutes(2));
        }
        ~DataManager() { }

        #region public methods
        public ConcurrentDictionary<Guid, ServiceTaskBase> GetServiceTasks() =>
            _ServiceTasks.ConcurrentDictionary;
        public ConcurrentDictionary<Guid, User> GetUsers() =>
            _Users.ConcurrentDictionary;
        #endregion
    }
}
