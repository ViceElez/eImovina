using eImovina.Shared.DTOs.Auth;
using eImovina.Shared.Models.Users;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace eImovina.Api.Security;

public sealed class JwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public LoginResponseDto CreateLoginResponse(User user, IReadOnlyCollection<string> roles)
    {
        var now = DateTime.UtcNow;
        var expiresAtUtc = now.AddMinutes(_options.ExpirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        AddOptionalClaim(claims, AppClaimTypes.EmployeeId, user.EmployeeId);

        if (user.Employee is not null)
            AddOptionalClaim(claims, AppClaimTypes.LocationId, user.Employee.LocationId);

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return new LoginResponseDto
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expiresAtUtc,
            User = new LoggedUserDto
            {
                UserId = user.Id,
                Username = user.Username,
                EmployeeId = user.EmployeeId,
                Roles = roles.OrderBy(role => role).ToList()
            }
        };
    }

    private static void AddOptionalClaim(ICollection<Claim> claims, string claimType, int? value)
    {
        if (value.HasValue)
            claims.Add(new Claim(claimType, value.Value.ToString()));
    }
}
