using System;

namespace SiteTrack.Core.Models
{
    public class OtpVerification
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string OtpCode { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }

        public OtpVerification()
        {
            this.CreatedAt = DateTime.Now;
            this.ExpiresAt = DateTime.Now.AddMinutes(5);
            this.IsUsed = false;
        }

        public bool IsExpired
        {
            get
            {
                return DateTime.Now > this.ExpiresAt;
            }
        }

        public bool IsValid
        {
            get
            {
                return !this.IsUsed && !this.IsExpired;
            }
        }
    }

    public class NotificationLog
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string EventType { get; set; }
        public string Message { get; set; }
        public string Channel { get; set; } // EMAIL, SMS, SYSTEM
        public string Status { get; set; }  // DELIVERED, PENDING, FAILED
        public DateTime CreatedAt { get; set; }

        public NotificationLog()
        {
            this.Channel = "SYSTEM";
            this.Status = "DELIVERED";
            this.CreatedAt = DateTime.Now;
        }
    }
}
