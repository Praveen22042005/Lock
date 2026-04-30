using System;

namespace Lock.Client.Models;

public class ActivationToken
{
    public string Hwid { get; set; } = string.Empty;
    public string LicenseKeyId { get; set; } = string.Empty;
    public string ProductId { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; } = DateTime.MinValue;
    public DateTime ExpiresAt { get; set; } = DateTime.MinValue;
}
