using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace FeatureServices
{
    internal static class ClaimExtensions
    {
        internal static void IsInRole(this IEnumerable<Claim> claims, string role)
        {
            if (!claims.Any(q => q.Type == ClaimTypes.Role && q.Value == role))
            {
                throw new UnauthorizedAccessException($"Requires {role} role");
            }
        }

        internal static void HasClaim(this IEnumerable<Claim> claims, string claimType)
        {
            if (!claims.Any(q => q.Type == claimType))
            {
                throw new UnauthorizedAccessException($"Requires {claimType} claim");
            }
        }

        internal static string UserName(this IEnumerable<Claim> claims)
        {
            return claims.First(q => q.Type == ClaimTypes.Name).Value;
        }
    }
}
