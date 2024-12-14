using NotificationApi.Models; // Ensure this is here

namespace NotificationApi.Services
{
    public interface IUserService
    {
        Task<User> GetUserByIdAsync(int userId);  // Ensure return type is Task<User>
        Task SendNotificationAsync(User user, string message, string notificationType);  // Ensure parameters match
    }
}
