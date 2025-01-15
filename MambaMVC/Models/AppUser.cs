using Microsoft.AspNetCore.Identity;

namespace MambaMVC.Models
{
    public class AppUser:IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
