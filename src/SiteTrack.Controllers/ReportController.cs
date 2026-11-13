using System;
using System.Collections.Generic;
using System.Linq;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Repositories;

namespace SiteTrack.Controllers
{
    public class ReportController : BaseController
    {
        private readonly IReportRepository _reportRepository;
        private readonly IStockRepository _stockRepository;
        private readonly ILaborRepository _laborRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMaterialRepository _materialRepository;

        public ReportController() : this(
            new ReportRepository(),
            new StockRepository(),
            new LaborRepository(),
            new ProjectRepository(),
            new MaterialRepository())
        {
        }

        public ReportController(
            IReportRepository reportRepository,
            IStockRepository stockRepository,
            ILaborRepository laborRepository,
            IProjectRepository projectRepository,
            IMaterialRepository materialRepository)
        {
            this._reportRepository = reportRepository ?? new ReportRepository();
            this._stockRepository = stockRepository ?? new StockRepository();
            this._laborRepository = laborRepository ?? new LaborRepository();
            this._projectRepository = projectRepository ?? new ProjectRepository();
            this._materialRepository = materialRepository ?? new MaterialRepository();
        }

        public bool CanAccessProject(int projectId)
        {
            if (IsAdmin) return true;
            if (!CurrentUserId.HasValue) return false;

            List<Project> assigned = _projectRepository.GetByManager(CurrentUserId.Value);
            return assigned.Exists(delegate(Project p) { return p.Id == projectId; });
        }

        public List<MaterialConsumptionReportItem> GetMaterialConsumptionReport(int projectId, DateTime startDate, DateTime endDate)
        {
            if (!CanAccessProject(projectId)) return new List<MaterialConsumptionReportItem>();

            Project project = _projectRepository.GetById(projectId);
            string projectName = project != null ? project.ProjectName : ("Project #" + projectId);

            List<MaterialUsage> usages = _stockRepository.GetUsages(projectId, startDate.Date, endDate.Date.AddDays(1).AddSeconds(-1));
            List<MaterialConsumptionReportItem> report = new List<MaterialConsumptionReportItem>();

            foreach (MaterialUsage u in usages)
            {
                Material m = _materialRepository.GetMaterialById(u.MaterialId);
                report.Add(new MaterialConsumptionReportItem
                {
                    UsageId = u.Id,
                    UsageDate = u.UsageDate,
                    ProjectName = projectName,
                    MaterialName = u.MaterialName ?? (m != null ? m.MaterialName : "Unknown"),
                    CategoryName = m != null ? m.CategoryName : "General",
                    Unit = u.Unit ?? (m != null ? m.Unit : "Units"),
                    QuantityUsed = u.QuantityUsed,
                    UnitPrice = u.UnitPrice,
                    TotalCost = u.TotalCost,
                    ActivityDescription = u.ActivityDescription,
                    RecordedByName = u.RecordedByName ?? "Site Mgr"
                });
            }

            return report.OrderByDescending(delegate(MaterialConsumptionReportItem r) { return r.UsageDate; }).ToList();
        }

        public List<LaborWageReportItem> GetLaborWageSummaryReport(int projectId, DateTime startDate, DateTime endDate, string status = null)
        {
            if (!CanAccessProject(projectId)) return new List<LaborWageReportItem>();

            Project project = _projectRepository.GetById(projectId);
            string projectName = project != null ? project.ProjectName : ("Project #" + projectId);

            List<WorkerPayment> payments = _laborRepository.GetPayments(projectId, startDate.Date, endDate.Date.AddDays(1).AddSeconds(-1), status);
            List<LaborWageReportItem> report = new List<LaborWageReportItem>();

            foreach (WorkerPayment p in payments)
            {
                SiteWorker w = _laborRepository.GetWorkerById(p.WorkerId);

                report.Add(new LaborWageReportItem
                {
                    PaymentId = p.Id,
                    AttendanceId = p.AttendanceId,
                    WorkDate = p.WorkDate,
                    ProjectName = projectName,
                    WorkerName = p.WorkerName ?? (w != null ? w.FullName : "Worker #" + p.WorkerId),
                    WorkerTypeName = w != null ? w.WorkerTypeName : "Craftsman",
                    WorkerPhone = p.WorkerPhone ?? (w != null ? w.Phone : "-"),
                    DailyRate = p.DailyRate,
                    AttendanceStatus = "PRESENT",
                    AmountOwed = p.AmountOwed,
                    AmountPaid = p.AmountPaid,
                    BalanceDue = p.BalanceDue,
                    PaymentStatus = p.PaymentStatus,
                    Notes = p.Notes,
                    PaidByName = p.PaidByName ?? "Cashier"
                });
            }

            return report.OrderByDescending(delegate(LaborWageReportItem r) { return r.WorkDate; }).ToList();
        }

        public List<StockMovementReportItem> GetStockMovementAuditReport(int projectId, DateTime startDate, DateTime endDate, int? materialId = null)
        {
            if (!CanAccessProject(projectId)) return new List<StockMovementReportItem>();

            Project project = _projectRepository.GetById(projectId);
            string projectName = project != null ? project.ProjectName : ("Project #" + projectId);

            List<MaterialStockMovement> movements = _stockRepository.GetMovements(projectId, materialId);
            DateTime start = startDate.Date;
            DateTime end = endDate.Date.AddDays(1).AddSeconds(-1);

            List<StockMovementReportItem> report = new List<StockMovementReportItem>();

            foreach (MaterialStockMovement m in movements)
            {
                if (m.MovementDate < start || m.MovementDate > end) continue;

                Material mat = _materialRepository.GetMaterialById(m.MaterialId);

                report.Add(new StockMovementReportItem
                {
                    MovementId = m.Id,
                    MovementDate = m.MovementDate,
                    MovementType = m.MovementType,
                    ProjectName = projectName,
                    MaterialName = m.MaterialName ?? (mat != null ? mat.MaterialName : "Item #" + m.MaterialId),
                    CategoryName = mat != null ? mat.CategoryName : "Materials",
                    Unit = mat != null ? mat.Unit : "Units",
                    Quantity = m.Quantity,
                    UnitPrice = m.UnitPrice,
                    TotalPrice = m.TotalPrice,
                    Description = m.Description,
                    RecordedByName = m.RecordedByName ?? "System"
                });
            }

            return report.OrderByDescending(delegate(StockMovementReportItem r) { return r.MovementDate; }).ToList();
        }

        public ProjectExecutiveSummary GetProjectFinancialSummary(int projectId)
        {
            if (!CanAccessProject(projectId)) return null;

            Project p = _projectRepository.GetById(projectId);
            if (p == null) return null;

            decimal totalPurchases = _reportRepository.GetTotalMaterialExpenditure(projectId);
            decimal totalLaborPaid = _reportRepository.GetTotalLaborCost(projectId);

            List<MaterialUsage> usages = _stockRepository.GetUsages(projectId, null, null);
            decimal totalUsageCost = usages != null ? usages.Sum(delegate(MaterialUsage u) { return u.TotalCost; }) : 0m;

            List<WorkerPayment> payments = _laborRepository.GetPayments(projectId, null, null, null);
            decimal outstandingLabor = payments != null ? payments.Sum(delegate(WorkerPayment pay) { return pay.BalanceDue; }) : 0m;

            List<ProjectActivity> activities = _projectRepository.GetActivities(projectId);
            int latestProgress = (activities != null && activities.Count > 0) ? activities[0].ProgressPercentage : 0;

            List<ProjectMaterialStock> lowStock = _reportRepository.GetLowStockAlerts(projectId);

            return new ProjectExecutiveSummary
            {
                ProjectId = p.Id,
                ProjectName = p.ProjectName,
                Location = p.Location,
                Status = p.Status,
                StartDate = p.StartDate,
                ExpectedEndDate = p.ExpectedEndDate,
                CurrentProgress = latestProgress,
                TotalMaterialPurchases = totalPurchases,
                TotalMaterialUsageCost = totalUsageCost,
                TotalLaborWagesPaid = totalLaborPaid,
                OutstandingLaborLiabilities = outstandingLabor,
                TotalProjectExpenditure = totalPurchases + totalLaborPaid,
                LowStockAlertsCount = lowStock != null ? lowStock.Count : 0
            };
        }

        public List<ProjectExecutiveSummary> GetExecutiveDashboardSummaries()
        {
            List<Project> projs;
            if (IsAdmin)
            {
                projs = _projectRepository.GetAll();
            }
            else if (CurrentUserId.HasValue)
            {
                projs = _projectRepository.GetByManager(CurrentUserId.Value);
            }
            else
            {
                projs = new List<Project>();
            }

            List<ProjectExecutiveSummary> list = new List<ProjectExecutiveSummary>();
            foreach (Project p in projs)
            {
                ProjectExecutiveSummary summary = GetProjectFinancialSummary(p.Id);
                if (summary != null) list.Add(summary);
            }
            return list;
        }
    }
}

