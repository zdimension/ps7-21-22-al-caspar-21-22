using Microsoft.AspNetCore.Authorization;
using PS7Api.Models;

namespace PS7Api.Utilities;

public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params UserRole[] allowedRoles)
    {
        Roles = string.Join(",", allowedRoles.Select(x => Enum.GetName(typeof(UserRole), x)));
    }
}