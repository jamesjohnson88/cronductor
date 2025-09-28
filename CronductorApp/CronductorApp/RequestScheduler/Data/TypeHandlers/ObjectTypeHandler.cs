using System.Data;
using System.Text.Json;
using Dapper;

namespace CronductorApp.RequestScheduler.Data.TypeHandlers;

public class ObjectTypeHandler : SqlMapper.TypeHandler<object>
{
    public override void SetValue(IDbDataParameter parameter, object? value)
    {
        parameter.Value = value == null ? null : JsonSerializer.Serialize(value);
    }

    public override object Parse(object? value)
    {
        if (value is null or DBNull)
            return new { };
            
        var json = value.ToString();
        return string.IsNullOrEmpty(json) 
            ? new { } 
            : JsonSerializer.Deserialize<object>(json) ?? new { };
    }
}