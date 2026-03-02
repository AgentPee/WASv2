using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WASv2.Models;

namespace WASv2.Data
{
    public class MyDbService : IMyDbService
    {
        private readonly ApplicationDbContext _db;
        public MyDbService(ApplicationDbContext db) => _db = db;

        public async Task<User?> ValidateUser(string email, string password)
        {
            return await _db.Users.SingleOrDefaultAsync(u => u.Email == email && u.PasswordHash == password);
        }
    }
}
