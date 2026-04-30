using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Lock.Client.Models;
using Lock.Client.Services;
using Lock.Shared.Dtos;

namespace Lock.Client.Services;

public class ActivationService
{
    private readonly HttpClient httpClient;
    private readonly TokenService tokenService;
    private readonly HardwareIdService hardwareIdService;

    public ActivationService(TokenService tokenService, HardwareIdService hardwareIdService)
    {
        this.tokenService = tokenService;
        this.hardwareIdService = hardwareIdService;
        this.httpClient = new HttpClient
        {
            BaseAddress = new Uri("https://lock-api-vznn.onrender.com")
        };
    }

    public async Task<ActivationToken> ActivateAsync(string fullName, string email, string licenseKey)
    {
        var hwid = hardwareIdService.GetHardwareId();
        var request = new ActivationRequestDto
        {
            Hwid = hwid,
            LicenseKey = licenseKey,
            Email = email,
            MachineName = Environment.MachineName,
            LicenseKeyId = string.Empty
        };

        var response = await httpClient.PostAsJsonAsync("/activate", request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception(error);
        }

        var result = await response.Content.ReadFromJsonAsync<ActivationResponseDto>();
        if (result == null) throw new Exception("Invalid response from server");

        if (!tokenService.VerifySignature(result.Token, result.Signature))
        {
            throw new Exception("signature verification failed");
        }

        tokenService.SaveToken(result.Token, result.Signature);

        var tokenModel = JsonSerializer.Deserialize<ActivationToken>(result.Token, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return tokenModel ?? throw new Exception("Failed to deserialize activation token");
    }

    public async Task<bool> RevalidateAsync(string licenseKeyId)
    {
        try
        {
            var hwid = hardwareIdService.GetHardwareId();
            var request = new ActivationRequestDto
            {
                Hwid = hwid,
                LicenseKeyId = licenseKeyId,
                LicenseKey = string.Empty,
                Email = string.Empty,
                MachineName = Environment.MachineName
            };

            var response = await httpClient.PostAsJsonAsync("/validate", request);
            if (!response.IsSuccessStatusCode) return false;

            var result = await response.Content.ReadFromJsonAsync<ActivationResponseDto>();
            if (result == null) return false;

            if (!tokenService.VerifySignature(result.Token, result.Signature)) return false;

            tokenService.SaveToken(result.Token, result.Signature);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> DeactivateAsync(string licenseKeyId)
    {
        try
        {
            var hwid = hardwareIdService.GetHardwareId();
            var request = new ActivationRequestDto
            {
                Hwid = hwid,
                LicenseKeyId = licenseKeyId,
                LicenseKey = string.Empty,
                Email = string.Empty,
                MachineName = Environment.MachineName
            };

            var response = await httpClient.PostAsJsonAsync("/deactivate", request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}
