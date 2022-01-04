using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PS7Api.Models;

namespace PS7Api.Utilities;

public static class DataGenerator
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        await using var context = services.GetRequiredService<Ps7Context>();
        await context.Database.EnsureCreatedAsync();

        using var roleStore = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in Enum.GetNames(typeof(UserRole)))
            await roleStore.CreateAsync(new IdentityRole { Name = role, NormalizedName = role });

        var userStore = new UserStore<User>(context);
        
        var admin = new User
        {
            Email = "admin@local",
            NormalizedEmail = "ADMIN@LOCAL",
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = "admin@local"
        };
        admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, "admin");
        await userStore.CreateAsync(admin);
        await userStore.AddToRoleAsync(admin, UserRole.Administrator.Name());
        
        var customs = new User
        {
            Email = "customs@local",
            NormalizedEmail = "CUSTOMS@LOCAL",
            SecurityStamp = Guid.NewGuid().ToString(),
            UserName = "customs@local"
        };
        customs.PasswordHash = new PasswordHasher<User>().HashPassword(customs, "customs");
        await userStore.CreateAsync(customs);
        await userStore.AddToRoleAsync(customs, UserRole.CustomsOfficer.Name());

        await context.SaveChangesAsync();
    }
}