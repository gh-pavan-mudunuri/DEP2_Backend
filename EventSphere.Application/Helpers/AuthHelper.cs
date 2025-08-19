using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace EventSphere.Application.Helpers
{
    public static class AuthHelper
    {
        public static int? GetOrganizerIdFromClaims(HttpContext httpContext)
        {
            var userIdClaim = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return null;
            if (int.TryParse(userIdClaim, out int organizerId))
                return organizerId;
            return null;
        }
    }
}
