using Microsoft.AspNetCore.Identity;

namespace PS7Api.Models;

public class User : IdentityUser
{
}

public static class UserRoles
{
    public const string CustomsOfficer = "CustomsOfficer";
    public const string Administrator = "Administrator";
}