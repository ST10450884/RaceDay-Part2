
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDay.Api.Authorization;

public class SessionAuthorizeAttribute : Attribute, IAsyncAuthorizationFilter
{
    private readonly string? _requiredRole;

    public SessionAuthorizeAttribute(string? requiredRole = null)
    {
        _requiredRole = requiredRole;
    }

    public Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var session = context.HttpContext.Session;
        var userId = session.GetInt32("UserId");
        var userRole = session.GetString("UserRole");

        if (userId == null || string.IsNullOrEmpty(userRole))
        {
            context.Result = new UnauthorizedObjectResult(new
            {
                message = "Please log in to access this resource."
            });

            return Task.CompletedTask;
        }

        if (_requiredRole != null &&
            !string.Equals(userRole, _requiredRole, StringComparison.OrdinalIgnoreCase))
        {
            context.Result = new ForbidResult();

            return Task.CompletedTask;
        }

        return Task.CompletedTask;
    }
}
