using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;

namespace Tasky.Models
{
    public class ApplicationUser : IdentityUser
    { 
        public int UserAccountID { get; set; }
        public virtual UserAccount Account { get; set; }    
    }
}