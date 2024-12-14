namespace NotificationApi.Models
{
    public class User
    {
        public int Id { get; set; } // Unique identifier for the user
        public string Name { get; set; } // User's full name
        public string Email { get; set; } // User's email address
        public string Password { get; set; } // User's password
        public string PhoneNumber { get; set; } // User's phone number
        public string NotificationPreference { get; set; } // "Email" or "SMS" indicating the user's notification preference
    }
}
