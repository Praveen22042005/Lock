using Microsoft.AspNetCore.Http;
using Lock.API.Services;
using Lock.API.Models;

namespace Lock.API.Endpoints;

public class DeactivateEndpoint
{
    public static async Task<IResult> Handle(
        ActivationRequest body,
        LicenseService licenseService,
        HttpContext httpContext)
    {
        if (!Guid.TryParse(body.LicenseKeyId, out var licenseKeyId))
        {
            return Results.BadRequest("invalid request");
        }

        if (string.IsNullOrWhiteSpace(body.Hwid))
        {
            return Results.BadRequest("hwid is required");
        }

        await licenseService.DeactivateAsync(licenseKeyId, body.Hwid);

        var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        await licenseService.LogEventAsync(string.Empty, body.Hwid, ipAddress, "deactivated", "user initiated deactivation");

        return Results.Ok("deactivation successful");
    }
}
