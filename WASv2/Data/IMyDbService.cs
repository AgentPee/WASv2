using System.Threading.Tasks;
using WASv2.Models;

namespace WASv2.Data
{
    public interface IMyDbService
    {
        Task<User?> ValidateUser(string email, string password);
    }
}
