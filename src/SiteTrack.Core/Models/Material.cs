using System;

namespace SiteTrack.Core.Models
{
    public class MaterialCategory
    {
        public int Id { get; set; }
        public string CategoryName { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public MaterialCategory()
        {
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return this.CategoryName ?? string.Empty;
        }
    }

    public class Material
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public decimal CurrentPrice { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation helpers
        public string CategoryName { get; set; }

        public Material()
        {
            this.Status = "ACTIVE";
            this.Unit = "pcs";
            this.CurrentPrice = 0.00m;
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        public override string ToString()
        {
            return string.Format("{0} ({1})", this.MaterialName, this.Unit);
        }
    }
}

