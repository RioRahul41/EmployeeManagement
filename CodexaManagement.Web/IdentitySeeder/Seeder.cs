
using CodexaManagement.Model.Entities;
using Microsoft.AspNetCore.Identity;

namespace CodexaManagement.Web.IdentitySeeder;

public static class Seeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<IdentityApplication>>();

        // 1. Create roles
        string[] roles = { "Admin", "HR", "Manager","User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(
                    new IdentityRole(role));

                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException(
                        string.Join("; ",
                            roleResult.Errors.Select(e => e.Description)));
                }
            }
        }

        // 2. Admin credentials
        string email = "admin@codexa.com";
        string password = "ChangeThis!Admin2026";

        // 3. Check whether Admin already exists
        var admin = await userManager.FindByEmailAsync(email);

        if (admin == null)
        {
            admin = new IdentityApplication
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Admin",
                Location = "Not specified"
            };

            var createResult = await userManager.CreateAsync(
                admin, password);

            if (!createResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        createResult.Errors.Select(e => e.Description)));
            }
        }

        // 4. Assign Admin role
        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            var roleResult = await userManager.AddToRoleAsync(
                admin, "Admin");

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        roleResult.Errors.Select(e => e.Description)));
            }
        }
    }
}
