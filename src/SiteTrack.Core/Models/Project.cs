using System;

namespace SiteTrack.Core.Models
{
    public class Project
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public string Location { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public string Status { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public Project()
        {
            this.StartDate = DateTime.Today;
            this.Status = "Planning";
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return this.ProjectName ?? string.Empty;
        }
    }

    public class ProjectManager
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int UserId { get; set; }
        public DateTime AssignedDate { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation helpers
        public string ProjectName { get; set; }
        public string ManagerName { get; set; }
        public string ManagerEmail { get; set; }

        public ProjectManager()
        {
            this.AssignedDate = DateTime.Today;
            this.Status = "ACTIVE";
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }
    }
}

