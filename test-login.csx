#!/usr/bin/env dotnet-script
#r "nuget: Npgsql, 9.0.0"
#r "nuget: Dapper, 2.1.35"
#r "nuget: DotNetEnv, 3.1.1"

using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using Dapper;

// Type handler for List<string>
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

    public override void SetValue(System.Data.IDbDataParameter parameter, List<string> value)
    {
        parameter.Value = value == null || !value.Any()
            ? string.Empty
            : string.Join(',', value);
    }
}

// Initialize Dapper
SqlMapper.AddTypeHandler(new StringListTypeHandler());

// Load env
DotNetEnv.Env.Load();

var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=webmatcha;Username=postgres;Password=q";

Console.WriteLine("=== LOGIN FUNCTIONALITY TEST ===\n");

using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();
Console.WriteLine("✓ Database connected\n");

// Test query
const string sql = @"
    SELECT id, username, email, first_name AS FirstName, last_name AS LastName,
        birth_date AS BirthDate, gender, sexual_preference AS SexualPreference,
        biography, interest_tags AS InterestTags, profile_photo_url AS ProfilePhotoUrl,
        photo_urls AS PhotoUrls, latitude, longitude, fame_rating AS FameRating,
        is_online AS IsOnline, last_seen AS LastSeen, is_email_verified AS IsEmailVerified,
        email_verified_at AS EmailVerifiedAt, is_active AS IsActive,
        created_at AS CreatedAt
    FROM users
    WHERE is_email_verified = true
    LIMIT 1
";

try
{
    var result = await connection.QueryFirstOrDefaultAsync<dynamic>(sql);

    if (result == null)
    {
        Console.WriteLine("✗ No verified users found");
        return 1;
    }

    Console.WriteLine($"✓ User query successful");
    Console.WriteLine($"  Username: {result.username}");
    Console.WriteLine($"  Email: {result.email}");
    Console.WriteLine($"  InterestTags: {result.interesttags}");
    Console.WriteLine($"  PhotoUrls: {result.photourls}");
    Console.WriteLine("\n✓ ALL TESTS PASSED");
    return 0;
}
catch (Exception ex)
{
    Console.WriteLine($"✗ FAILED: {ex.Message}");
    if (ex.InnerException != null)
        Console.WriteLine($"  Inner: {ex.InnerException.Message}");
    return 1;
}
