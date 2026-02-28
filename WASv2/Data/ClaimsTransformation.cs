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
                var emailClaim = identity.FindFirst(ClaimTypes.Email);
                if (emailClaim != null)
                {
                    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == emailClaim.Value);
                    if (user != null)
                    {
                        // Add role claim
                        identity.AddClaim(new Claim(ClaimTypes.Role, user.Role));
                        // Add department claim
                        identity.AddClaim(new Claim("DepartmentId", user.DeptId.ToString()));
                    }
                }
            }
            return principal;
        }
    }
}
