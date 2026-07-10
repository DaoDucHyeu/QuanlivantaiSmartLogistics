using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Identity;
using SmartLogistics.Models;

namespace SmartLogistics.Helpers
{
    public class AuthorizeUserTypeAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
    {
        private readonly UserType[] _allowedRoles;

        public AuthorizeUserTypeAttribute(params UserType[] allowedRoles)
        {
            _allowedRoles = allowedRoles;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Get UserManager from DI
            var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<AppUser>>();

            var appUser = await userManager.GetUserAsync(user);

            if (appUser == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Admin is allowed to access everything
            if (appUser.UserType == UserType.Admin)
            {
                return;
            }

            // Check if current user type is in allowed roles
            if (!_allowedRoles.Contains(appUser.UserType))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
