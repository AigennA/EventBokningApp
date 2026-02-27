using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EventBokningApp.DTOs;

namespace EventBokningApp.Services;

/// <summary>
/// In-circuit auth state for Blazor Server pages.
/// Stores the JWT token in memory and exposes user info.
/// </summary>
public class BlazorAuthService
{
    private AuthResponseDto? _currentUser;

    public event Action? OnChange;

    public bool IsAuthenticated => _currentUser is not null && _currentUser.ExpiresAt > DateTime.UtcNow;
    public string? Username => _currentUser?.Username;
    public string? Email => _currentUser?.Email;
    public string? Token => _currentUser?.Token;
    public bool IsAdmin => _currentUser?.Role == "Admin";

    public void SetUser(AuthResponseDto response)
    {
        _currentUser = response;
        OnChange?.Invoke();
    }

    public void Logout()
    {
        _currentUser = null;
        OnChange?.Invoke();
    }

    public ClaimsPrincipal GetClaimsPrincipal()
    {
        if (!IsAuthenticated || _currentUser is null)
            return new ClaimsPrincipal(new ClaimsIdentity());

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(_currentUser.Token);
        var identity = new ClaimsIdentity(jwt.Claims, "jwt");
        return new ClaimsPrincipal(identity);
    }
}
