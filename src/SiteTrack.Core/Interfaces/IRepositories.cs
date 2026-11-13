using System;
using System.Collections.Generic;
using SiteTrack.Core.Models;

namespace SiteTrack.Core.Interfaces
{
    public interface IUserRepository
    {
        User GetById(int id);
        User GetByUsername(string username);
        User GetByEmail(string email);
        List<User> GetAll();
        int Insert(User user);
        bool Update(User user);
        bool SetStatus(int userId, string status);
        bool SaveOtp(OtpVerification otp);
        OtpVerification GetValidOtp(int userId, string otpCode);
        bool MarkOtpUsed(int otpId);
        void LogNotification(NotificationLog log);
        List<NotificationLog> GetNotificationLogs(int? userId);
    }

    public interface IProjectRepository
    {
        Project GetById(int id);
        List<Project> GetAll();
        List<Project> GetByManager(int userId);
        int Insert(Project project);
        bool Update(Project project);
        bool Delete(int projectId);
        List<ProjectManager> GetManagersForProject(int projectId);
        bool AssignManager(int projectId, int userId);
        bool RemoveManager(int projectId, int userId);
        bool HasActiveChildRecords(int projectId);
        List<ProjectActivity> GetActivities(int projectId);
        int InsertActivity(ProjectActivity activity);
    }

    public interface IMaterialRepository
    {
        List<MaterialCategory> GetAllCategories();
        MaterialCategory GetCategoryById(int id);
        int InsertCategory(MaterialCategory category);
        bool UpdateCategory(MaterialCategory category);
        bool DeleteCategory(int id);

        List<Material> GetAllMaterials(int? categoryId);
        Material GetMaterialById(int id);
        int InsertMaterial(Material material);
        bool UpdateMaterial(Material material);
        bool DeleteMaterial(int id);
    }

    public interface IStockRepository
    {
        List<ProjectMaterialStock> GetStockByProject(int projectId);
        ProjectMaterialStock GetStock(int projectId, int materialId);
        int RecordPurchase(MaterialPurchase purchase);
        int RecordUsage(MaterialUsage usage);
        int RecordAdjustment(int projectId, int materialId, decimal newQuantity, string reason, int recordedBy);
        List<MaterialStockMovement> GetMovements(int projectId, int? materialId);
        List<MaterialUsage> GetUsages(int projectId, DateTime? startDate, DateTime? endDate);
        List<MaterialPurchase> GetPurchases(int projectId, DateTime? startDate, DateTime? endDate);
    }

    public interface ILaborRepository
    {
        List<WorkerType> GetAllWorkerTypes();
        WorkerType GetWorkerTypeById(int id);
        int InsertWorkerType(WorkerType type);
        bool UpdateWorkerType(WorkerType type);
        bool DeleteWorkerType(int id);

        List<SiteWorker> GetAllWorkers(int? workerTypeId, string status);
        SiteWorker GetWorkerById(int id);
        int InsertWorker(SiteWorker worker);
        bool UpdateWorker(SiteWorker worker);

        List<WorkerAttendance> GetAttendance(int projectId, DateTime workDate);
        WorkerAttendance GetAttendanceRecord(int projectId, int workerId, DateTime workDate);
        int SaveAttendance(WorkerAttendance attendance);

        List<WorkerPayment> GetPayments(int projectId, DateTime? startDate, DateTime? endDate, string status);
        WorkerPayment GetPaymentByAttendanceId(int attendanceId);
        int SavePayment(WorkerPayment payment);
        bool UpdatePaymentStatus(int paymentId, string status, decimal amountPaid, int paidBy);
    }

    public interface IReportRepository
    {
        decimal GetTotalMaterialExpenditure(int projectId);
        decimal GetTotalLaborCost(int projectId);
        List<ProjectMaterialStock> GetLowStockAlerts(int? projectId);
    }
}
