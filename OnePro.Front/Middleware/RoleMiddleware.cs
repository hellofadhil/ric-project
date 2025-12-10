// RoleRequiredAttribute
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OnePro.Front.Middleware
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class RoleRequiredAttribute : Attribute, IAuthorizationFilter
    {
        private readonly int[] _allowedRoles;

        public RoleRequiredAttribute() : this(1, 4)
        {
        }

        public RoleRequiredAttribute(params int[] allowedRoles)
        {
            _allowedRoles = allowedRoles ?? Array.Empty<int>();
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var session = context.HttpContext.Session;

            var roleValueString = session.GetString("UserRole");
            if (!int.TryParse(roleValueString, out var roleInt))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
                return;
            }

            if (!_allowedRoles.Contains(roleInt))
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            }
        }
    }
}
