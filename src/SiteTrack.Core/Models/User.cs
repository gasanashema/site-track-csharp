using System;

namespace SiteTrack.Core.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public User()
        {
            this.Role = "SiteManager";
            this.Status = "ACTIVE";
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        public bool IsAdmin
        {
            get
            {
                return string.Equals(this.Role, "Administrator", StringComparison.OrdinalIgnoreCase) ||
                       string.Equals(this.Role, "Admin", StringComparison.OrdinalIgnoreCase);
            }
        }

        public bool IsActive
        {
            get
            {
                return string.Equals(this.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}

