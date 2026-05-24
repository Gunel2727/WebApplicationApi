using Microsoft.AspNetCore.Identity;

namespace WebApplication2.Models
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = null!;
        public string? RefreshToken { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}

