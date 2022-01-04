using Microsoft.AspNetCore.Identity;

namespace PS7Api.Models;

public class User : IdentityUser
{
}

public enum UserRole
{
    CustomsOfficer,
    Administrator
}

public static class UserRoleExtensions
{
    public static string Name(this UserRole role)
    {
        return role.ToString().ToUpper();
    }
}