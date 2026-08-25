using eImovina.Shared.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace eImovina.Api.Data;

public static class UserSeeder
{
    public static async Task SeedAsync(eImovinaDbContext db)
    {
        if (await db.Users.AnyAsync())
            return;

        var hasher = new PasswordHasher<User>();

        var admin = new User
        {
            Username = "admin",
            EmployeeId = await db.Employees
                .Where(item => item.Email == "ana.admic@zupanija.hr")
                .Select(item => item.Id)
                .FirstOrDefaultAsync(),
            CreatedAt = DateTime.UtcNow
        };
        admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");

        var manager = new User
        {
            Username = "manager",
            EmployeeId = await db.Employees
                .Where(item => item.Email == "marko.maric@zupanija.hr")
                .Select(item => item.Id)
                .FirstOrDefaultAsync(),
            CreatedAt = DateTime.UtcNow
        };
        manager.PasswordHash = hasher.HashPassword(manager, "Manager123!");

        var responsible = new User
        {
            Username = "responsible",
            EmployeeId = await db.Employees
                .Where(item => item.Email == "petra.peric@zupanija.hr")
                .Select(item => item.Id)
                .FirstOrDefaultAsync(),
            CreatedAt = DateTime.UtcNow
        };
        responsible.PasswordHash = hasher.HashPassword(responsible, "Responsible123!");

        var employee = new User
        {
            Username = "employee",
            EmployeeId = await db.Employees
                .Where(item => item.Email == "ivo.ivic@zupanija.hr")
                .Select(item => item.Id)
                .FirstOrDefaultAsync(),
            CreatedAt = DateTime.UtcNow
        };
        employee.PasswordHash = hasher.HashPassword(employee, "Employee123!");

        db.Users.AddRange(admin, manager, responsible, employee);
        await db.SaveChangesAsync();

        db.UserRoles.AddRange(
            new UserRole { UserId = admin.Id, RoleId = 1 },
            new UserRole { UserId = manager.Id, RoleId = 2 },
            new UserRole { UserId = responsible.Id, RoleId = 3 },
            new UserRole { UserId = employee.Id, RoleId = 4 }
        );

        await db.SaveChangesAsync();
    }
}
