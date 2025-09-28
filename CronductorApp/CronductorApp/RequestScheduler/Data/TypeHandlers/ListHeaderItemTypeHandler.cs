using System.Data;
using System.Text.Json;
using CronductorApp.Components.Composition.Models;
using Dapper;

namespace CronductorApp.RequestScheduler.Data.TypeHandlers;

public class ListHeaderItemTypeHandler : SqlMapper.TypeHandler<List<HeaderItem>>
{
    public override void SetValue(IDbDataParameter parameter, List<HeaderItem>? value)
    {
        parameter.Value = value == null ? null : JsonSerializer.Serialize(value);
    }

    public override List<HeaderItem> Parse(object? value)
    {
        if (value is null or DBNull)
        {
            return [];
        }
            
        var json = value.ToString();
        return string.IsNullOrEmpty(json) 
            ? []
            : JsonSerializer.Deserialize<List<HeaderItem>>(json) ?? [];
    }
}