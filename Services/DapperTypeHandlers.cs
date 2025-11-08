using Dapper;
using System.Data;

namespace WebMatcha.Services;

/// <summary>
/// Custom Dapper type handlers for complex types
/// </summary>
public class StringListTypeHandler : SqlMapper.TypeHandler<List<string>>
{
    public override List<string> Parse(object value)
    {
        if (value == null || value is DBNull)
            return new List<string>();

        var stringValue = value.ToString();
        if (string.IsNullOrWhiteSpace(stringValue))
            return new List<string>();

        return stringValue.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Trim())
            .ToList();
    }

    public override void SetValue(IDbDataParameter parameter, List<string> value)
    {
        parameter.Value = value == null || !value.Any()
            ? string.Empty
            : string.Join(',', value);
    }
}

/// <summary>
/// Register all custom Dapper type handlers
/// </summary>
public static class DapperConfig
{
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (_initialized)
            return;

        SqlMapper.AddTypeHandler(new StringListTypeHandler());
        _initialized = true;
    }
}
