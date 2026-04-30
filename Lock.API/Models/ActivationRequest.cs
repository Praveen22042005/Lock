namespace Lock.API.Models;

public class ActivationRequest
{
    public string Hwid { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string LicenseKeyId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string IpAddress { get; set; } = string.Empty;
}
