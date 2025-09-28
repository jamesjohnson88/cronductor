using CronductorApp.RequestScheduler.Data.TypeHandlers;
using Dapper;

namespace CronductorApp.Startup;

public static class DapperConfig
{
    public static void RegisterTypeHandlers()
    {
        SqlMapper.AddTypeHandler(new ListHeaderItemTypeHandler());
        SqlMapper.AddTypeHandler(new ObjectTypeHandler());
    }
}