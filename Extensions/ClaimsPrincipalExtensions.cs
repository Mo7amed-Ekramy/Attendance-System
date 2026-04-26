using System;
using System.Security.Claims;

namespace MVC_PROJECT.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the User ID from the current claims principal
        /// </summary>
        public static int GetUserId(this System.Security.Principal.IPrincipal user)
        {
            if (user == null)
            {
                return 0;
            }

            var claimsPrincipal = user as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                return 0;
            }

            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return 0;
            }

            return userId;
        }

        /// <summary>
        /// Gets the user's role from the current claims principal
        /// </summary>
        public static string? GetUserRole(this System.Security.Principal.IPrincipal user)
        {
            if (user == null)
            {
                return null;
            }

            var claimsPrincipal = user as ClaimsPrincipal;
            if (claimsPrincipal == null)
            {
                return null;
            }

            var roleClaim = claimsPrincipal.FindFirst(ClaimTypes.Role);
            return roleClaim?.Value;
        }

        /// <summary>
        /// Checks if the current user has a specific role
        /// </summary>
        public static bool IsInRole(this System.Security.Principal.IPrincipal user, string role)
        {
            if (user == null)
            {
                return false;
            }

            return user.IsInRole(role);
        }
    }
}
