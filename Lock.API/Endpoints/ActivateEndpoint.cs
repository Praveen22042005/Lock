using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Lock.API.Services;
using Lock.API.Models;
using Lock.Shared.Dtos;

namespace Lock.API.Endpoints;

public class ActivateEndpoint
{
    public static async Task<IResult> Handle(
        ActivationRequest body,
        LicenseService licenseService,
        SigningService signingService,
        HttpContext httpContext)
    {
        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var license = await licenseService.GetLicenseKeyAsync(body.LicenseKey);
        if (license == null)
        {
            return Results.BadRequest("invalid license key");
        }

        var licenseKeyHash = HashKey(body.LicenseKey);

        if (license.Value.status != "active")
        {
            await licenseService.LogEventAsync(licenseKeyHash, body.Hwid, ipAddress, "failed", "key not active");
            return Results.BadRequest("this license is no longer active");
        }

        var activationCount = await licenseService.GetActiveActivationCountAsync(license.Value.id);
        if (activationCount >= license.Value.maxActivations)
        {
            await licenseService.LogEventAsync(licenseKeyHash, body.Hwid, ipAddress, "failed", "activation limit reached");
            return Results.BadRequest("maximum activations reached");
        }

        await licenseService.UpsertActivationAsync(license.Value.id, body.Hwid, null, body.MachineName);

        var issuedAt = DateTime.UtcNow;
        var expiresAt = issuedAt.AddDays(30);

        var payloadObj = new
        {
            hwid = body.Hwid,
            licenseKeyId = license.Value.id,
            productId = license.Value.productId,
            issuedAt = issuedAt,
            expiresAt = expiresAt
        };

        var tokenPayload = JsonSerializer.Serialize(payloadObj);
        var signature = signingService.Sign(tokenPayload);

        await licenseService.LogEventAsync(licenseKeyHash, body.Hwid, ipAddress, "success", "activation completed");

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
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(licenseKey);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLower();
    }
}
