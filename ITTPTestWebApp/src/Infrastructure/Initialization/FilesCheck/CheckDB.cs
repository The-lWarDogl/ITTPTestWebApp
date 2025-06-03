using Npgsql;
using Dapper;

using ITTPTestWebApp.Logging;
using ITTPTestWebApp.Data;

namespace ITTPTestWebApp.Initialization.FilesCheck
{
    partial class FilesChecker
    {
        private static Dictionary<string, HashSet<string>> _TablePermissions = new Dictionary<string, HashSet<string>>()
        {
            { "servicetasks", new HashSet<string> { "INSERT", "TRUNCATE", "DELETE", "UPDATE", "SELECT" } },
            { "users", new HashSet<string> { "INSERT", "TRUNCATE", "DELETE", "UPDATE", "SELECT" } }
        };

        public class TablePermission
        {
            public string TableName { get; set; } = string.Empty;
            public string Permission { get; set; } = string.Empty;
        }

        private async Task CheckDB()
        {
            string connectionString = Config.Instance.Read("DBConnectionString");

            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                { await connection.OpenAsync(); }
                Logger.Instance.Log($"DBConnectionString is correct", tag: "filescheck");
            }
            catch { throw new Exception("DBConnectionString is invalid"); }

            ComparePermissions(_TablePermissions, await GetUserPermissions(connectionString));
        }

        static async Task<Dictionary<string, HashSet<string>>> GetUserPermissions(string connectionString)
        {
            string sql = @"
            SELECT 
                t.tablename AS ""TableName"", 
                g.privilege_type AS ""Permission""
            FROM pg_tables t
            LEFT JOIN information_schema.role_table_grants g 
                ON t.schemaname = g.table_schema AND t.tablename = g.table_name
            WHERE t.schemaname NOT IN ('pg_catalog', 'information_schema')
            ORDER BY t.tablename;";

            using (var connection = new NpgsqlConnection(connectionString))
            { 
                var result = await connection.QueryAsync<TablePermission>(sql);
                return result
                .Where(r => r.Permission != null)
                .GroupBy(r => r.TableName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(r => r.Permission).ToHashSet()
                );
            }
        }

        static void ComparePermissions(Dictionary<string, HashSet<string>> expected, Dictionary<string, HashSet<string>> actual)
        {
            bool failed = false;
            foreach (var table in expected.Keys)
            {
                if (!actual.ContainsKey(table)) { Logger.Instance.Log($"DB table {table} is missing", tag: "filescheck"); failed = true; continue; }

                var expectedPerms = expected[table];
                var actualPerms = actual[table];

                var missingPerms = expectedPerms.Except(actualPerms).ToList();
                var extraPerms = actualPerms.Except(expectedPerms).ToList();

                if (missingPerms.Any()) { Logger.Instance.Log($"DB table {table} is missing permissions: {string.Join(", ", missingPerms)}", tag: "filescheck"); failed = true; }
                if (extraPerms.Any()) { Logger.Instance.Log($"DB table {table} has extra permissions: {string.Join(", ", extraPerms)}", tag: "filescheck"); }
                if (!missingPerms.Any() && !extraPerms.Any()) { Logger.Instance.Log($"DB Table {table} is correct", tag: "filescheck"); }
            }
            foreach (var extraTable in actual.Keys.Except(expected.Keys).ToList()) { Logger.Instance.Log($"DB an extra table {extraTable} was found", tag: "filescheck"); }
            if (failed) { throw new Exception("DB check failed"); }
        }
    }
}
