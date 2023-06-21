
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Tasky.Models
{
    public class UserAccount
    {
        public int Id { get; set; }
        [StringLength(50)]
        [Display(Name = "Name")]
        public string? Username { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? Locale { get; set; }
        [JsonIgnore]
        public string UserID { get; set; }
        [JsonIgnore]
        public virtual ApplicationUser User { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; }


    }
}
