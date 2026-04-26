using MVC_PROJECT.Models;
using System.Threading.Tasks;

namespace MVC_PROJECT.Services.Interfaces
{
    public interface IAccountService
    {
        Task<User?> ValidateLoginAsync(string userName, string password);
        string GetRedirectUrlByRole(User user);
    }
}
