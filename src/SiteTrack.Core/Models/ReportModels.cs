using System;

namespace SiteTrack.Core.Models
{
    public class MaterialConsumptionReportItem
    {
        public int UsageId { get; set; }
        public DateTime UsageDate { get; set; }
        public string ProjectName { get; set; }
        public string MaterialName { get; set; }
        public string CategoryName { get; set; }
        public string Unit { get; set; }
        public decimal QuantityUsed { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalCost { get; set; }
        public string ActivityDescription { get; set; }
        public string RecordedByName { get; set; }
    }

    public class LaborWageReportItem
    {
        public int PaymentId { get; set; }
        public int AttendanceId { get; set; }
        public DateTime WorkDate { get; set; }
        public string ProjectName { get; set; }
        public string WorkerName { get; set; }
        public string WorkerTypeName { get; set; }
        public string WorkerPhone { get; set; }
        public decimal DailyRate { get; set; }
        public string AttendanceStatus { get; set; }
        public decimal AmountOwed { get; set; }
        public decimal AmountPaid { get; set; }
        public decimal BalanceDue { get; set; }
        public string PaymentStatus { get; set; }
        public string Notes { get; set; }
        public string PaidByName { get; set; }
    }

    public class StockMovementReportItem
    {
        public int MovementId { get; set; }
        public DateTime MovementDate { get; set; }
        public string MovementType { get; set; } // IN, OUT, ADJUSTMENT
        public string ProjectName { get; set; }
        public string MaterialName { get; set; }
        public string CategoryName { get; set; }
        public string Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public string Description { get; set; }
        public string RecordedByName { get; set; }
    }

    public class ProjectExecutiveSummary
    {
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? ExpectedEndDate { get; set; }
        public int CurrentProgress { get; set; }
        public decimal TotalMaterialPurchases { get; set; }
        public decimal TotalMaterialUsageCost { get; set; }
        public decimal TotalLaborWagesPaid { get; set; }
        public decimal OutstandingLaborLiabilities { get; set; }
        public decimal TotalProjectExpenditure { get; set; }
        public int LowStockAlertsCount { get; set; }
    }
}
