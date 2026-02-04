using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SEMS.Infrastructure.Persistence;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using SEMS.Core.Identity;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly SemsDbContext _db;
    private readonly IConfiguration _config;

    public AuthController(SemsDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
    }

    public record LoginRequest(string Username, string Password, Guid? TenantId);
    public record TokenResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt);
    public record RefreshRequest(string RefreshToken, Guid? TenantId);
    public record RegisterRequest(string Username, string Email, string Password, Guid? TenantId);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Username == request.Username, ct);
        if (user is null) return Unauthorized();

        if (!VerifyPassword(request.Password, Convert.FromBase64String(user.PasswordSalt), Convert.FromBase64String(user.PasswordHash)))
            return Unauthorized();

        if (request.TenantId.HasValue)
        {
            user.TenantId = request.TenantId;
        }

        var accessToken = GenerateJwt(user);
        var refreshToken = GenerateSecureToken();
        var rt = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            TenantId = user.TenantId
        };
        _db.RefreshTokens.Add(rt);
        await _db.SaveChangesAsync(ct);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);
        var expires = jwt.ValidTo;

        return Ok(new TokenResponse(accessToken, refreshToken, expires));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct);
        if (token is null || !token.IsActive) return Unauthorized();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == token.UserId, ct);
        if (user is null) return Unauthorized();

        // rotate refresh token
        var newRefresh = GenerateSecureToken();
        token.RevokedAt = DateTime.UtcNow;
        token.ReplacedByToken = newRefresh;

        var newToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefresh,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            TenantId = user.TenantId
        };
        _db.RefreshTokens.Add(newToken);

        var accessToken = GenerateJwt(user);
        await _db.SaveChangesAsync(ct);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(accessToken);
        var expires = jwt.ValidTo;

        return Ok(new TokenResponse(accessToken, newRefresh, expires));
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var exists = await _db.Users.IgnoreQueryFilters()
            .AnyAsync(u => u.Username == request.Username || u.Email == request.Email, ct);
        if (exists) return Conflict(new { error = "Username or Email already exists" });

        var salt = GenerateSalt();
        var hash = HashPassword(request.Password, salt);
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordSalt = Convert.ToBase64String(salt),
            PasswordHash = Convert.ToBase64String(hash),
            Roles = new List<SEMS.Core.Enums.UserRole> { SEMS.Core.Enums.UserRole.Employee },
            TenantId = request.TenantId
        };
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);
        return Created($"/api/v1/auth/users/{user.Id}", new { id = user.Id });
    }

    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RefreshRequest request, CancellationToken ct)
    {
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == request.RefreshToken, ct);
        if (token is null) return NotFound();
        token.RevokedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static bool VerifyPassword(string password, byte[] salt, byte[] expectedHash)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(32);
        return CryptographicOperations.FixedTimeEquals(hash, expectedHash);
    }

    private static byte[] GenerateSalt()
    {
        var salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        return salt;
    }

    private static byte[] HashPassword(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32);
    }

    private string GenerateJwt(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
        };
        if (user.TenantId.HasValue)
        {
            claims.Add(new Claim("tenantid", user.TenantId.Value.ToString()));
        }
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"],
            audience: _config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"]!)),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}
