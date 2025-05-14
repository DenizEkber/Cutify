using Microsoft.AspNetCore.Authorization;

namespace Cutify.Web.Authorization;

public class BarberAuthorizationAttribute : AuthorizeAttribute
{
    public BarberAuthorizationAttribute()
    {
        Roles = Cutify.Domain.Entities.Barber.Role;
    }
} 