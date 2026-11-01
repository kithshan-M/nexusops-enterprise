
using System.ComponentModel.DataAnnotations;

namespace Identity.Application.DTOs;

public sealed record RegisterRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string FullName { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(256)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(12)]
    [StringLength(128)]
    public string Password { get; init; } = string.Empty;
}
