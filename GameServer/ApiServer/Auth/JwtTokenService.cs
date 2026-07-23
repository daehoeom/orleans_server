using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GrainLibrary.Utility;
using Microsoft.IdentityModel.Tokens;

namespace ApiServer.Auth;

public class JwtTokenService
{
    private const string PlayerSqidClaimType = "pid";

    private readonly JwtSetting _setting;
    private readonly PlayerSqidService _sqidService;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(JwtSetting setting, PlayerSqidService sqidService)
    {
        _setting = setting;
        _sqidService = sqidService;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_setting.SecretKey));
    }

    public string GenerateToken(long playerId)
    {
        var claims = new[]
        {
            new Claim(PlayerSqidClaimType, _sqidService.Encode(playerId)),
        };

        var token = new JwtSecurityToken(
            issuer: _setting.Issuer,
            audience: _setting.Audience,
            claims: claims,
            expires: TimeUtil.UtcNow.AddMinutes(_setting.ExpiryMinutes),
            signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public long? ValidatePlayerId(string token)
    {
        var parameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = _setting.Issuer,
            ValidateAudience = true,
            ValidAudience = _setting.Audience,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ClockSkew = TimeSpan.FromSeconds(30),
        };

        try
        {
            var principal = new JwtSecurityTokenHandler().ValidateToken(token, parameters, out _);
            var sqid = principal.FindFirstValue(PlayerSqidClaimType);
            return string.IsNullOrEmpty(sqid) ? null : _sqidService.Decode(sqid);
        }
        catch (SecurityTokenException)
        {
            return null;
        }
    }
}
