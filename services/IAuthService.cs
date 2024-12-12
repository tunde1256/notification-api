using NotificationApi.Models;

namespace NotificationApi.Services
{
    public interface IAuthService
    {
        Task<User> RegisterAsync(User user, string password);
        Task<string> LoginAsync(string email, string password);
    }
}
