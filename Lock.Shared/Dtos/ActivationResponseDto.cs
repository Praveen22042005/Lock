using System;

namespace Lock.Shared.Dtos;

public class ActivationResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Signature { get; set; } = string.Empty;
}
