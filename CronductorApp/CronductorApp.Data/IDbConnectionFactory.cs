using System.Data;

namespace CronductorApp.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}