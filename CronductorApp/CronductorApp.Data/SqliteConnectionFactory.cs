using System.Data;
using Microsoft.Data.Sqlite;

namespace CronductorApp.Data;

public class SqliteConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public IDbConnection CreateConnection()
    {
        return new SqliteConnection(connectionString);
    }
}