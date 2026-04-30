using System.Security.Cryptography;
using System.Text;
using Npgsql;

var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
var random = new Random();

string GenerateGroup() => new string(Enumerable.Range(0, 5).Select(_ => chars[random.Next(chars.Length)]).ToArray());

var g1 = GenerateGroup();
var g2 = GenerateGroup();
var g3 = GenerateGroup();
var g4 = GenerateGroup();

var combined = g1 + g2 + g3 + g4;
var asciiSum = combined.Sum(c => (int)c);
var checksum = (asciiSum % 100000).ToString("D5");

var licenseKey = $"{g1}-{g2}-{g3}-{g4}-{checksum}";
Console.WriteLine($"Generated License Key: {licenseKey}");

var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(licenseKey));
var keyHash = Convert.ToHexString(hashBytes).ToLower();

var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (string.IsNullOrEmpty(databaseUrl))
{
    Console.WriteLine("Error: DATABASE_URL environment variable is not set.");
    return;
}

try
{
    using var connection = new NpgsqlConnection(databaseUrl);
    await connection.OpenAsync();

    var sql = @"
        INSERT INTO license_keys (key_hash, product_id, max_activations, status, expires_at)
        VALUES (@keyHash, 'lock-v1', 1, 'active', @expiresAt)";

    using var command = new NpgsqlCommand(sql, connection);
    command.Parameters.AddWithValue("keyHash", keyHash);
    command.Parameters.AddWithValue("expiresAt", DateTime.UtcNow.AddYears(1));

    await command.ExecuteNonQueryAsync();
    Console.WriteLine("License key successfully inserted into database.");
}
catch (Exception ex)
{
    Console.WriteLine($"Error inserting license key: {ex.Message}");
}
