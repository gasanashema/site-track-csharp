using System;

namespace SiteTrack.Core.Models
{
    public class MaterialPurchase
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int MaterialId { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string SupplierName { get; set; }
        public DateTime PurchaseDate { get; set; }
        public int? RecordedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation helpers
        public string ProjectName { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public string RecordedByName { get; set; }

        public MaterialPurchase()
        {
            this.PurchaseDate = DateTime.Today;
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }
    }

    public class ProjectMaterialStock
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int MaterialId { get; set; }
        public decimal QuantityAvailable { get; set; }
        public decimal MinimumQuantity { get; set; }
        public decimal AverageUnitPrice { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation helpers
        public string ProjectName { get; set; }
        public string MaterialName { get; set; }
        public string CategoryName { get; set; }
        public string Unit { get; set; }

        public ProjectMaterialStock()
        {
            this.QuantityAvailable = 0.00m;
            this.MinimumQuantity = 0.00m;
            this.AverageUnitPrice = 0.00m;
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }

        public bool IsLowStock
        {
            get
            {
                return this.QuantityAvailable <= this.MinimumQuantity;
            }
        }

        public decimal TotalStockValue
        {
            get
            {
                return this.QuantityAvailable * this.AverageUnitPrice;
            }
        }
    }

    public class MaterialStockMovement
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int MaterialId { get; set; }
        public string MovementType { get; set; } // IN, OUT, ADJUSTMENT
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime MovementDate { get; set; }
        public string Description { get; set; }
        public string ReferenceType { get; set; } // purchase, usage, adjustment
        public int? ReferenceId { get; set; }
        public int? RecordedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation helpers
        public string ProjectName { get; set; }
        public string MaterialName { get; set; }
        public string RecordedByName { get; set; }

        public MaterialStockMovement()
        {
            this.MovementDate = DateTime.Today;
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }
    }

    public class MaterialUsage
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int MaterialId { get; set; }
        public decimal QuantityUsed { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public DateTime UsageDate { get; set; }
        public string ActivityDescription { get; set; }
        public int? RecordedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation helpers
        public string ProjectName { get; set; }
        public string MaterialName { get; set; }
        public string Unit { get; set; }
        public string RecordedByName { get; set; }

        public MaterialUsage()
        {
            this.UsageDate = DateTime.Today;
            this.CreatedAt = DateTime.Now;
            this.UpdatedAt = DateTime.Now;
        }
    }
}
