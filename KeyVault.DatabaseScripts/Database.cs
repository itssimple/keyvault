using DbUp;
using Microsoft.Data.Sqlite;
using System;
using System.Reflection;

namespace KeyVault.DatabaseScripts
{
    public class Database : IDisposable
    {
        private readonly SqliteConnection _connection;

        private static bool UpgradePerformed = false;

        public Database(string connectionString)
        {
            _connection = new SqliteConnection(connectionString);
            EnsureDatabaseIsCreated();
        }

        private DbUp.Engine.DatabaseUpgradeResult EnsureDatabaseIsCreated()
        {
            if (!UpgradePerformed)
            {
                UpgradePerformed = true;
                return DeployChanges.To
                    .SQLiteDatabase(_connection.ConnectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build().PerformUpgrade();
            }
            return null;
        }

        public T GetSingleItem<T>(Func<SqliteDataReader, T> convertToObject, string sql, params SqliteParameter[] parameters) where T : class
        {
            using (var cmd = CreateCommand(sql, parameters))
            {
                using var r = cmd.ExecuteReader();
                if (!r.HasRows)
                {
                    return null;
                }
                r.Read();

                return convertToObject(r);
            }
        }

        public int ExecuteNonQuery(string sql, params SqliteParameter[] parameters)
        {
            using (var cmd = CreateCommand(sql, parameters))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        private SqliteCommand CreateCommand(string sql, params SqliteParameter[] parameters)
        {
            if (_connection.State != System.Data.ConnectionState.Open)
            {
                _connection.Open();
            }
            var cmd = _connection.CreateCommand();
            cmd.CommandText = sql;
            cmd.Parameters.AddRange(parameters);

            return cmd;
        }

        public void Dispose()
        {
            _connection?.Close();
            _connection?.Dispose();
        }
    }
}
