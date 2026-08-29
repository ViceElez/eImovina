using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace eImovina.App.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly AuthTokenProvider _tokenProvider;
    private static readonly ClaimsPrincipal Anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(AuthTokenProvider tokenProvider)
    {
        _tokenProvider = tokenProvider;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var stored = await _tokenProvider.GetAsync();
        if (stored is null)
            return new AuthenticationState(Anonymous);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, stored.User.UserId.ToString()),
            new(ClaimTypes.Name, stored.User.Username)
        };

        claims.AddRange(stored.User.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        if (stored.User.EmployeeId.HasValue)
            claims.Add(new Claim("employee_id", stored.User.EmployeeId.Value.ToString()));

        var identity = new ClaimsIdentity(claims, authenticationType: "jwt");
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }


    public void NotifyUserChanged()
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}
