using System.Security.Cryptography;
using System.Text;
using Npgsql;

namespace Lock.API.Services;

public class LicenseService
{
    private readonly string connectionString;

    public LicenseService(string connectionString)
    {
        this.connectionString = connectionString;
    }

    private string HashKey(string licenseKey)
    {
        var bytes = Encoding.UTF8.GetBytes(licenseKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLower();
    }

    public async Task<(Guid id, string productId, int maxActivations, string status)?> GetLicenseKeyAsync(string licenseKey)
    {
        var keyHash = HashKey(licenseKey);
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand("SELECT id, product_id, max_activations, status FROM license_keys WHERE key_hash = @keyHash", connection);
        command.Parameters.AddWithValue("keyHash", keyHash);

        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return (
                reader.GetGuid(0),
                reader.GetString(1),
                reader.GetInt32(2),
                reader.GetString(3)
            );
        }

        return null;
    }

    public async Task<int> GetActiveActivationCountAsync(Guid licenseKeyId)
    {
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand("SELECT COUNT(*) FROM activations WHERE license_key_id = @licenseKeyId AND status = 'active'", connection);
        command.Parameters.AddWithValue("licenseKeyId", licenseKeyId);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task UpsertActivationAsync(Guid licenseKeyId, string hardwareId, Guid? userId, string machineName)
    {
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var sql = @"
            INSERT INTO activations (license_key_id, hardware_id, user_id, machine_name, status, activated_at)
            VALUES (@licenseKeyId, @hardwareId, @userId, @machineName, 'active', @now)
            ON CONFLICT (license_key_id, hardware_id) 
            DO UPDATE SET 
                status = 'active', 
                activated_at = @now,
                user_id = @userId,
                machine_name = @machineName";

        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("licenseKeyId", licenseKeyId);
        command.Parameters.AddWithValue("hardwareId", hardwareId);
        command.Parameters.AddWithValue("userId", (object?)userId ?? DBNull.Value);
        command.Parameters.AddWithValue("machineName", machineName);
        command.Parameters.AddWithValue("now", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<string?> GetActivationStatusAsync(Guid licenseKeyId, string hardwareId)
    {
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand("SELECT status FROM activations WHERE license_key_id = @licenseKeyId AND hardware_id = @hardwareId", connection);
        command.Parameters.AddWithValue("licenseKeyId", licenseKeyId);
        command.Parameters.AddWithValue("hardwareId", hardwareId);

        return (string?)await command.ExecuteScalarAsync();
    }

    public async Task DeactivateAsync(Guid licenseKeyId, string hardwareId)
    {
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        using var command = new NpgsqlCommand("UPDATE activations SET status = 'deactivated' WHERE license_key_id = @licenseKeyId AND hardware_id = @hardwareId", connection);
        command.Parameters.AddWithValue("licenseKeyId", licenseKeyId);
        command.Parameters.AddWithValue("hardwareId", hardwareId);

        await command.ExecuteNonQueryAsync();
    }

    public async Task LogEventAsync(string licenseKeyHash, string hardwareId, string ipAddress, string eventType, string reason)
    {
        using var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();

        var sql = "INSERT INTO activation_logs (license_key_hash, hardware_id, ip_address, event_type, reason, created_at) VALUES (@licenseKeyHash, @hardwareId, @ipAddress, @eventType, @reason, @now)";
        
        using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("licenseKeyHash", licenseKeyHash);
        command.Parameters.AddWithValue("hardwareId", hardwareId);
        command.Parameters.AddWithValue("ipAddress", ipAddress);
        command.Parameters.AddWithValue("eventType", eventType);
        command.Parameters.AddWithValue("reason", reason);
        command.Parameters.AddWithValue("now", DateTime.UtcNow);

        await command.ExecuteNonQueryAsync();
    }
}
