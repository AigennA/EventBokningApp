using System.ComponentModel.DataAnnotations;

namespace EventBokningApp.DTOs;

public record RegisterDto(
    [Required][MaxLength(100)] string Username,
    [Required][EmailAddress] string Email,
    [Required][MinLength(6)] string Password
);

public record LoginDto(
    [Required][EmailAddress] string Email,
    [Required] string Password
);

public record AuthResponseDto(
    string Token,
    string Username,
    string Email,
    string Role,
    DateTime ExpiresAt
);
