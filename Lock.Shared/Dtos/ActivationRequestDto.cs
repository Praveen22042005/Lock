namespace Lock.Shared.Dtos;

public class ActivationRequestDto
{
    public string Hwid { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string LicenseKeyId { get; set; } = string.Empty;
}
