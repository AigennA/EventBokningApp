using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EventBokningApp.DTOs;
using EventBokningApp.Exceptions;
using EventBokningApp.Models;
using EventBokningApp.Repositories;
using Microsoft.IdentityModel.Tokens;

namespace EventBokningApp.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepo;
    private readonly IConfiguration _config;

    public AuthService(IUserRepository userRepo, IConfiguration config)
    {
        _userRepo = userRepo;
        _config = config;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await _userRepo.ExistsByEmailAsync(dto.Email))
            throw new BusinessException("A user with this email already exists.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };

        await _userRepo.AddAsync(user);
        await _userRepo.SaveChangesAsync();

        return GenerateToken(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email)
            ?? throw new BusinessException("Invalid email or password.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new BusinessException("Invalid email or password.");

        return GenerateToken(user);
    }

    private AuthResponseDto GenerateToken(User user)
    {
        var jwtKey = _config["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.UtcNow.AddHours(8);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: expires,
            signingCredentials: creds
        );

        return new AuthResponseDto(
            Token: new JwtSecurityTokenHandler().WriteToken(token),
            Username: user.Username,
            Email: user.Email,
            Role: user.Role,
            ExpiresAt: expires
        );
    }
}
