using Microsoft.AspNetCore.Authorization;
using PS7Api.Models;

namespace PS7Api.Utilities;

public class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params UserRole[] allowedRoles)
    {
        var allowedRolesAsStrings = allowedRoles.Select(x => Enum.GetName(typeof(UserRole), x));
        Roles = string.Join(",", allowedRolesAsStrings);
    }
}