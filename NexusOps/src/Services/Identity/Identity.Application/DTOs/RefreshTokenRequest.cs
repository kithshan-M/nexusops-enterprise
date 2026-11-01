
using System.ComponentModel.DataAnnotations;

namespace Identity.Application.DTOs;

public sealed record RefreshTokenRequest
{
    [Required]
    public string RefreshToken { get; init; } = string.Empty;
}
