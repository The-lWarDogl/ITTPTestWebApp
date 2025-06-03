using System.Reflection;
using System.Text;
using Npgsql;
using Dapper;

namespace ITTPTestWebApp.Data.Pgsql
{
    public class TablePgsql<T> where T : class
    {
        #region fields 
        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly List<PropertyInfo> _properties;
        private readonly List<PropertyInfo> _primaryKeyProperties;
        private readonly List<PropertyInfo> _nonPrimaryKeyProperties;
        #endregion

        public TablePgsql(string connectionString)
        {
            _connectionString = connectionString;

            var type = typeof(T);
            var tableAttr = type.GetCustomAttribute<TableAttribute>();
            if (tableAttr == null) throw new InvalidOperationException($"Class {type.Name} has no attribute TableAttribute.");

            _tableName = tableAttr.Name;

            _properties = type.GetProperties()
                .Where(p => p.GetCustomAttribute<ColumnAttribute>() != null)
                .ToList();

            _primaryKeyProperties = _properties
                .Where(p => p.GetCustomAttribute<ColumnAttribute>()!.IsPrimaryKey)
                .ToList();

            if (!_primaryKeyProperties.Any()) throw new InvalidOperationException($"Class {type.Name} must have at least one primary key.");

            _nonPrimaryKeyProperties = _properties
                                        .Where(p => !p.GetCustomAttribute<ColumnAttribute>()!.IsPrimaryKey)
                                        .ToList();
        }

        #region public methods
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            var query = $"SELECT * FROM {_tableName}";

            using (var connection = new NpgsqlConnection(_connectionString))
            { return await connection.QueryAsync<T>(query); }
        }

        public async Task<T?> GetByIdAsync(object key)
        {
            var whereClause = BuildWhereClause(key, out DynamicParameters parameters);
            var query = $"SELECT * FROM {_tableName} WHERE {whereClause}";

            using (var connection = new NpgsqlConnection(_connectionString))
            { return await connection.QuerySingleOrDefaultAsync<T>(query, parameters); }
        }

        public async Task AddAsync(T entity)
        {
            var insertQuery = GenerateInsertQuery();
            using (var connection = new NpgsqlConnection(_connectionString))
            { await connection.ExecuteAsync(insertQuery, entity); }
        }

        public async Task UpdateAsync(T entity)
        {
            var updateQuery = GenerateUpdateQuery();
            using (var connection = new NpgsqlConnection(_connectionString))
            { await connection.ExecuteAsync(updateQuery, entity); }
        }

        public async Task DeleteAsync(object key)
        {
            var whereClause = BuildWhereClause(key, out DynamicParameters parameters);
            var deleteQuery = $"DELETE FROM {_tableName} WHERE {whereClause}";

            using (var connection = new NpgsqlConnection(_connectionString))
            { await connection.ExecuteAsync(deleteQuery, parameters); }
        }

        public async Task ReplaceAllAsync(IEnumerable<T> data)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                using (var transaction = await connection.BeginTransactionAsync())
                {
                    try
                    {
                        var clearQuery = $"TRUNCATE TABLE {_tableName} RESTART IDENTITY CASCADE";
                        await connection.ExecuteAsync(clearQuery, transaction: transaction);
                        var insertQuery = GenerateInsertQuery();
                        await connection.ExecuteAsync(insertQuery, data, transaction: transaction);
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }
            }
        }
        #endregion

        #region private methods
        private string GenerateInsertQuery()
        {
            var columns = _properties.Select(p => p.GetCustomAttribute<ColumnAttribute>()!.Name).ToList();
            var parameters = _properties.Select(p => "@" + p.Name).ToList();

            var insertQuery = new StringBuilder($"INSERT INTO {_tableName} ");
            insertQuery.Append("(");
            insertQuery.Append(string.Join(", ", columns));
            insertQuery.Append(") VALUES (");
            insertQuery.Append(string.Join(", ", parameters));
            insertQuery.Append(")");

            return insertQuery.ToString();
        }

        private string GenerateUpdateQuery()
        {
            var setClause = string.Join(", ", _nonPrimaryKeyProperties.Select(p => $"{p.GetCustomAttribute<ColumnAttribute>()!.Name} = @{p.Name}"));
            var whereClause = string.Join(" AND ", _primaryKeyProperties.Select(p => $"{p.GetCustomAttribute<ColumnAttribute>()!.Name} = @{p.Name}"));

            var updateQuery = new StringBuilder($"UPDATE {_tableName} SET {setClause} WHERE {whereClause}");
            return updateQuery.ToString();
        }

        private string BuildWhereClause(object key, out DynamicParameters parameters)
        {
            parameters = new DynamicParameters();

            if (_primaryKeyProperties.Count == 1)
            {
                // Если один первичный ключ, ожидаем одно значение
                var keyValue = key;
                var pkProp = _primaryKeyProperties.First();
                parameters.Add("@" + pkProp.Name, keyValue);
                return $"{pkProp.GetCustomAttribute<ColumnAttribute>()!.Name} = @{pkProp.Name}";
            }
            else
            {
                // Если составной ключ, ожидаем объект с соответствующими свойствами
                var keyObject = key as IDictionary<string, object>;
                if (keyObject == null)
                    throw new ArgumentException("For a composite key, you must pass IDictionary<string, object>.");

                var clauses = new List<string>();
                foreach (var pkProp in _primaryKeyProperties)
                {
                    if (!keyObject.ContainsKey(pkProp.Name))
                        throw new ArgumentException($"The key does not contain a value for the primary key: {pkProp.Name}");

                    var columnName = pkProp.GetCustomAttribute<ColumnAttribute>()!.Name;
                    var paramName = "@" + pkProp.Name;
                    clauses.Add($"{columnName} = {paramName}");
                    parameters.Add(paramName, keyObject[pkProp.Name]);
                }

                return string.Join(" AND ", clauses);
            }
        }
        #endregion
    }
}