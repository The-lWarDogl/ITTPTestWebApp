using ITTPTestWebApp.Data.Pgsql.TableElements;

namespace ITTPTestWebApp.Data.Pgsql
{
    class DBPgsqlContext
    {
        private readonly string _connectionString;

        public DBPgsqlContext(string connectionString)
        { _connectionString = connectionString; }

        public TablePgsql<ServiceTaskTableElement> ServiceTasks => new (_connectionString);
        public TablePgsql<UserTableElement> Users => new (_connectionString);
    }
}