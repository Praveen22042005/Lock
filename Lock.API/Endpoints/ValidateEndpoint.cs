using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Lock.API.Services;
using Lock.API.Models;
using Lock.Shared.Dtos;

namespace Lock.API.Endpoints;

public class ValidateEndpoint
{
    public static async Task<IResult> Handle(
        ActivationRequest body,
        LicenseService licenseService,
        SigningService signingService,
        HttpContext httpContext)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (!Guid.TryParse(body.LicenseKeyId, out var licenseKeyId))
        {
            return Results.BadRequest("invalid request");
        }

        var status = await licenseService.GetActivationStatusAsync(licenseKeyId, body.Hwid);
        if (status == null || status != "active")
        {
            return Results.BadRequest("activation not found or inactive");
        }

        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddDays(30);

        var payloadObj = new
        {
            hwid = body.Hwid,
            licenseKeyId = licenseKeyId,
            issuedAt = issuedAt,
            expiresAt = expiresAt
        };

        var tokenPayload = JsonSerializer.Serialize(payloadObj);
        var signature = signingService.Sign(tokenPayload);

        var licenseKeyHash = HashKey(body.LicenseKey);
        await licenseService.LogEventAsync(licenseKeyHash, body.Hwid, ipAddress, "success", "validation completed");

        var response = new ActivationResponseDto
        {
            Token = tokenPayload,
            IssuedAt = issuedAt,
            ExpiresAt = expiresAt,
            Signature = signature
        };

        return Results.Ok(response);
    }

    private static string HashKey(string licenseKey)
    {
        if (string.IsNullOrEmpty(licenseKey)) return "unknown";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(licenseKey);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
