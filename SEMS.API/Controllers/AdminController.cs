using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SEMS.Infrastructure.Persistence;
using System.Security.Cryptography;
using SEMS.Core.Enums;
using SEMS.Core.Identity;

namespace SEMS.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly SemsDbContext _db;
    public AdminController(SemsDbContext db) => _db = db;

    [HttpPost("init")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Init()
    {
        // Rely on EF Core migrations and seeding via this endpoint only
        if (!_db.Departments.Any())
        {
            _db.Departments.Add(new SEMS.Core.HR.Department { Name = "Engineering" });
            _db.Departments.Add(new SEMS.Core.HR.Department { Name = "HR" });
        }
        if (!_db.Roles.Any())
        {
            _db.Roles.Add(new SEMS.Core.HR.Role { Name = "Developer" });
            _db.Roles.Add(new SEMS.Core.HR.Role { Name = "Manager" });
        }

        if (!_db.Users.Any())
        {
            var users = new List<User>
            {
                CreateUser("admin", "admin@sems.local", "admin123", UserRole.Admin),
                CreateUser("hr", "hr@sems.local", "hr123", UserRole.HR),
                CreateUser("dev", "dev@sems.local", "dev123", UserRole.Employee),
                CreateUser("manager", "manager@sems.local", "manager123", UserRole.Manager)
            };
            _db.Users.AddRange(users);
        }

        await _db.SaveChangesAsync();
        return Ok(new { status = "initialized" });
    }

    private static User CreateUser(string username, string email, string password, params UserRole[] roles)
    {
        var salt = GenerateSalt();
        var hash = HashPassword(password, salt);
        return new User
        {
            Username = username,
            Email = email,
            PasswordSalt = Convert.ToBase64String(salt),
            PasswordHash = Convert.ToBase64String(hash),
            Roles = roles.ToList()
        };
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
}
