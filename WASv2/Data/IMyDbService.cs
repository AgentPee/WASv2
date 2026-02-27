using System.Threading.Tasks;

namespace WASv2.Data
{
    public interface IMyDbService
    {
        Task<User?> ValidateUser(string email, string password);
    }
}
