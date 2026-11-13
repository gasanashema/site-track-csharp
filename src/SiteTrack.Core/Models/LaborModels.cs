using System;

namespace SiteTrack.Core.Models
{
    public class ProjectActivity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string ActivityTitle { get; set; }
        public string ActivityDescription { get; set; }
        public DateTime ActivityDate { get; set; }
        public int ProgressPercentage { get; set; }
        public int? RecordedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation helpers
        public string ProjectName { get; set; }
        public string RecordedByName { get; set; }

        public ProjectActivity()
        {
            this.ActivityDate = DateTime.Today;
            this.ProgressPercentage = 0;
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }
    }

    public class WorkerType
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public decimal DefaultDailyRate { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public WorkerType()
        {
            this.DefaultDailyRate = 0.00m;
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return this.TypeName ?? string.Empty;
        }
    }

    public class SiteWorker
    {
        public int Id { get; set; }
        public int WorkerTypeId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public decimal DailyRate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation helpers
        public string WorkerTypeName { get; set; }

        public SiteWorker()
        {
            this.DailyRate = 0.00m;
            this.Status = "ACTIVE";
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.FullName, this.WorkerTypeName ?? "Worker");
        }
    }

    public class WorkerAttendance
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int WorkerId { get; set; }
        public DateTime WorkDate { get; set; }
        public string AttendanceStatus { get; set; } // PRESENT, ABSENT, HALF_DAY
        public string WorkDescription { get; set; }
        public int? RecordedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation helpers
        public string ProjectName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerPhone { get; set; }
        public string WorkerTypeName { get; set; }
        public decimal DailyRate { get; set; }

        public WorkerAttendance()
        {
            this.WorkDate = DateTime.Today;
            this.AttendanceStatus = "PRESENT";
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        public bool IsPresent
        {
            get
            {
                return string.Equals(this.AttendanceStatus, "PRESENT", StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    public class WorkerPayment
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int WorkerId { get; set; }
        public int AttendanceId { get; set; }
        public DateTime WorkDate { get; set; }
        public decimal DailyRate { get; set; }
        public decimal AmountOwed { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentStatus { get; set; } // PENDING, PAID
        public int? PaidBy { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation helpers
        public string ProjectName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerPhone { get; set; }
        public string PaidByName { get; set; }

        public WorkerPayment()
        {
            this.WorkDate = DateTime.Today;
            this.DailyRate = 0.00m;
            this.AmountOwed = 0.00m;
            this.AmountPaid = 0.00m;
            this.PaymentStatus = "PENDING";
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        public decimal BalanceDue
        {
            get
            {
                return this.AmountOwed - this.AmountPaid;
            }
        }

        public bool IsFullyPaid
        {
            get
            {
                return string.Equals(this.PaymentStatus, "PAID", StringComparison.OrdinalIgnoreCase) ||
                       (this.AmountPaid >= this.AmountOwed && this.AmountOwed > 0);
            }
        }
    }
}

