using System.Threading.Tasks;
using NotificationApi.Models; // Ensure this is here
using Microsoft.Extensions.Logging;

namespace NotificationApi.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int userId);  
        
        Task SendNotificationAsync(User user, string message, string notificationType);  
}
}

