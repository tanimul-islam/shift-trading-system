using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using shiftTrade.api.models;
using shiftTrade.Api.Services.Auth;

namespace shiftTrade.Api.Services.Auth;

public sealed class JwtTokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreateToken(
        ApplicationUser user,
        OrganizationMembership membership
    )
    {
        var jwtKey = _configuration["Jwt:Key"]
        ?? throw new InvalidOperationException("Jwt signing key not found");


        var jwtIssuer = _configuration["Jwt:Issuer"]
        ?? throw new InvalidOperationException("Jwt Issuer not found");

        var jwtAudience = _configuration["Jwt:Audience"]
        ?? throw new InvalidOperationException("Jwt Audience not found");

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new("display_name", user.displayName),
            new ("organization_id", membership.OrganizationId.ToString()),
            new("organization_role", membership.Role)
        };

        var signinKey = new SymmetricSecurityKey(
            Convert.FromBase64String(jwtKey)
        );

        var credentials = new SigningCredentials(
            signinKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience:jwtAudience,
            claims:claims,
            expires:DateTime.UtcNow.AddHours(2),
            signingCredentials:credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(token);

    }

}