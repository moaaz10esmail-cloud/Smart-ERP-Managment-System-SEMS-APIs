using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using SEMS.Infrastructure.Persistence;
using SEMS.Core.Identity;
using SEMS.Core.Enums;
using System.Security.Cryptography;

namespace SEMS.API.Services;

public class DbSeederHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    public DbSeederHostedService(IServiceProvider services) => _services = services;
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SemsDbContext>();
        await db.Database.MigrateAsync(cancellationToken);
        if (!db.Departments.Any())
        {
            db.Departments.Add(new SEMS.Core.HR.Department { Name = "Engineering" });
            db.Departments.Add(new SEMS.Core.HR.Department { Name = "HR" });
        }
        if (!db.Roles.Any())
        {
            db.Roles.Add(new SEMS.Core.HR.Role { Name = "Developer" });
            db.Roles.Add(new SEMS.Core.HR.Role { Name = "Manager" });
        }

        if (!db.Users.Any())
        {
            var adminSalt = GenerateSalt();
            var adminHash = HashPassword("admin123", adminSalt);
            db.Users.Add(new User
            {
                Username = "admin",
                Email = "admin@sems.local",
                PasswordSalt = Convert.ToBase64String(adminSalt),
                PasswordHash = Convert.ToBase64String(adminHash),
                Roles = new List<UserRole> { UserRole.Admin }
            });

            var userSalt = GenerateSalt();
            var userHash = HashPassword("user123", userSalt);
            db.Users.Add(new User
            {
                Username = "user",
                Email = "user@sems.local",
                PasswordSalt = Convert.ToBase64String(userSalt),
                PasswordHash = Convert.ToBase64String(userHash),
                Roles = new List<UserRole> { UserRole.Employee }
            });
        }

        await db.SaveChangesAsync(cancellationToken);
    }
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

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
