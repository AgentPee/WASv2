using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WASv2.Data;
using WASv2.Models;

namespace WASv2.Services
{
    public class ClaimsTransformation : IClaimsTransformation
    {
        private readonly ApplicationDbContext _context;

        public ClaimsTransformation(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is ClaimsIdentity identity)
            {
                // FIX: Define emailClaim before using it
                var emailClaim = identity.FindFirst(ClaimTypes.Email);

                if (emailClaim != null)
                {
                    // Fetch user and include the Role object to get the RoleName
                    var user = await _context.Users
                        .Include(u => u.Role)
                        .FirstOrDefaultAsync(u => u.Email == emailClaim.Value);

                    if (user != null)
                    {
                        // Add role claim using the helper property
                        identity.AddClaim(new Claim(ClaimTypes.Role, user.RoleName));

                        // FIX: Use DeptID (matching your User.cs)
                        identity.AddClaim(new Claim("DepartmentId", user.DeptID?.ToString() ?? "0"));
                    }
                }
            }
            return principal;
        }
    }
}