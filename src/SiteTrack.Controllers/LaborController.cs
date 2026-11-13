using System;
using System.Collections.Generic;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Repositories;

namespace SiteTrack.Controllers
{
    public class LaborController : BaseController
    {
        private readonly ILaborRepository _laborRepository;
        private readonly IProjectRepository _projectRepository;

        public LaborController() : this(new LaborRepository(), new ProjectRepository())
        {
        }

        public LaborController(ILaborRepository laborRepository, IProjectRepository projectRepository)
        {
            this._laborRepository = laborRepository ?? new LaborRepository();
            this._projectRepository = projectRepository ?? new ProjectRepository();
        }

        public bool CanAccessProject(int projectId)
        {
            if (IsAdmin) return true;
            if (!CurrentUserId.HasValue) return false;

            List<Project> assigned = _projectRepository.GetByManager(CurrentUserId.Value);
            return assigned.Exists(delegate(Project p) { return p.Id == projectId; });
        }

        // ==========================================
        // 1. Worker Types (Skill Categories)
        // ==========================================

        public List<WorkerType> GetWorkerTypes()
        {
            return _laborRepository.GetAllWorkerTypes();
        }

        public WorkerType GetWorkerType(int id)
        {
            return _laborRepository.GetWorkerTypeById(id);
        }

        public Result<int> CreateWorkerType(string name, decimal defaultDailyRate, string description)
        {
            if (!IsAdmin)
            {
                return Result<int>.Failure("Access Denied: Only Administrators can create worker skill categories.");
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                return Result<int>.Failure("Worker type name is required.");
            }

            if (defaultDailyRate < 0)
            {
                return Result<int>.Failure("Default daily rate cannot be negative.");
            }

            WorkerType wt = new WorkerType
            {
                TypeName = name.Trim(),
                DefaultDailyRate = defaultDailyRate,
                Description = description != null ? description.Trim() : null
            };

            try
            {
                int id = _laborRepository.InsertWorkerType(wt);
                return Result<int>.Success(id);
            }
            catch (ValidationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Error creating worker type: " + ex.Message);
            }
        }

        public Result UpdateWorkerType(int id, string name, decimal defaultDailyRate, string description)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can edit worker skill categories.");
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                return Result.Failure("Worker type name is required.");
            }

            if (defaultDailyRate < 0)
            {
                return Result.Failure("Default daily rate cannot be negative.");
            }

            WorkerType wt = new WorkerType
            {
                Id = id,
                TypeName = name.Trim(),
                DefaultDailyRate = defaultDailyRate,
                Description = description != null ? description.Trim() : null
            };

            try
            {
                bool ok = _laborRepository.UpdateWorkerType(wt);
                return ok ? Result.Success() : Result.Failure("Worker type not found.");
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error updating worker type: " + ex.Message);
            }
        }

        public Result DeleteWorkerType(int id)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can delete worker skill categories.");
            }

            try
            {
                bool ok = _laborRepository.DeleteWorkerType(id);
                return ok ? Result.Success() : Result.Failure("Worker type not found.");
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error deleting worker type: " + ex.Message);
            }
        }

        // ==========================================
        // 2. Site Workers (Aba-byizi & Craftsmen)
        // ==========================================

        public List<SiteWorker> GetWorkers(int? workerTypeId = null, string status = null)
        {
            return _laborRepository.GetAllWorkers(workerTypeId, status);
        }

        public SiteWorker GetWorker(int id)
        {
            return _laborRepository.GetWorkerById(id);
        }

        public Result<int> CreateWorker(int workerTypeId, string fullName, string phone, decimal dailyRate, string status)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(fullName.Trim()))
            {
                return Result<int>.Failure("Worker full name is required.");
            }

            if (dailyRate < 0)
            {
                return Result<int>.Failure("Daily wage rate cannot be negative.");
            }

            WorkerType wt = _laborRepository.GetWorkerTypeById(workerTypeId);
            if (wt == null)
            {
                return Result<int>.Failure("Invalid worker skill category selected.");
            }

            SiteWorker worker = new SiteWorker
            {
                WorkerTypeId = workerTypeId,
                FullName = fullName.Trim(),
                Phone = phone != null ? phone.Trim() : null,
                DailyRate = dailyRate,
                Status = string.Equals(status, "INACTIVE", StringComparison.OrdinalIgnoreCase) ? "INACTIVE" : "ACTIVE"
            };

            try
            {
                int id = _laborRepository.InsertWorker(worker);
                return Result<int>.Success(id);
            }
            catch (ValidationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Error creating site worker: " + ex.Message);
            }
        }

        public Result UpdateWorker(int id, int workerTypeId, string fullName, string phone, decimal dailyRate, string status)
        {
            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(fullName.Trim()))
            {
                return Result.Failure("Worker full name is required.");
            }

            if (dailyRate < 0)
            {
                return Result.Failure("Daily wage rate cannot be negative.");
            }

            WorkerType wt = _laborRepository.GetWorkerTypeById(workerTypeId);
            if (wt == null)
            {
                return Result.Failure("Invalid worker skill category selected.");
            }

            SiteWorker worker = new SiteWorker
            {
                Id = id,
                WorkerTypeId = workerTypeId,
                FullName = fullName.Trim(),
                Phone = phone != null ? phone.Trim() : null,
                DailyRate = dailyRate,
                Status = string.Equals(status, "INACTIVE", StringComparison.OrdinalIgnoreCase) ? "INACTIVE" : "ACTIVE"
            };

            try
            {
                bool ok = _laborRepository.UpdateWorker(worker);
                return ok ? Result.Success() : Result.Failure("Worker not found.");
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error updating worker: " + ex.Message);
            }
        }

        // ==========================================
        // 3. Daily Attendance
        // ==========================================

        public List<WorkerAttendance> GetAttendance(int projectId, DateTime workDate)
        {
            if (!CanAccessProject(projectId)) return new List<WorkerAttendance>();
            return _laborRepository.GetAttendance(projectId, workDate);
        }

        public Result<int> RecordAttendance(int projectId, int workerId, DateTime workDate, string status, string description)
        {
            if (!CanAccessProject(projectId))
            {
                return Result<int>.Failure("Access Denied: You are not assigned to manage attendance for this project.");
            }

            if (string.IsNullOrEmpty(status))
            {
                return Result<int>.Failure("Attendance status is required.");
            }

            string upperStatus = status.Trim().ToUpperInvariant();
            if (upperStatus != "PRESENT" && upperStatus != "ABSENT" && upperStatus != "HALF_DAY")
            {
                return Result<int>.Failure("Invalid attendance status. Must be PRESENT, ABSENT, or HALF_DAY.");
            }

            SiteWorker worker = _laborRepository.GetWorkerById(workerId);
            if (worker == null || !string.Equals(worker.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                return Result<int>.Failure("Cannot record attendance: Worker does not exist or is marked INACTIVE.");
            }

            WorkerAttendance attendance = new WorkerAttendance
            {
                ProjectId = projectId,
                WorkerId = workerId,
                WorkDate = workDate.Date,
                AttendanceStatus = upperStatus,
                WorkDescription = description != null ? description.Trim() : null,
                RecordedBy = CurrentUserId
            };

            try
            {
                int id = _laborRepository.SaveAttendance(attendance);
                return Result<int>.Success(id);
            }
            catch (ValidationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Error saving attendance: " + ex.Message);
            }
        }

        // ==========================================
        // 4. Attendance-Gated Wage Payments (Business Rule 2)
        // ==========================================

        public List<WorkerPayment> GetPayments(int projectId, DateTime? startDate = null, DateTime? endDate = null, string status = null)
        {
            if (!CanAccessProject(projectId)) return new List<WorkerPayment>();
            return _laborRepository.GetPayments(projectId, startDate, endDate, status);
        }

        public Result<int> RecordPayment(int projectId, int workerId, int attendanceId, DateTime workDate, decimal dailyRate, decimal amountPaid, string notes)
        {
            if (!CanAccessProject(projectId))
            {
                return Result<int>.Failure("Access Denied: You are not assigned to record payments for this project.");
            }

            // Verify attendance record
            WorkerAttendance att = _laborRepository.GetAttendanceRecord(projectId, workerId, workDate);
            if (att == null || att.Id != attendanceId)
            {
                return Result<int>.Failure("Business Rule 2 Violation: Attendance record not found for this worker on specified date.");
            }

            // Business Rule 2: Gated Payment Creation
            if (!att.IsPresent)
            {
                return Result<int>.Failure(
                    string.Format("Business Rule 2 Violation: Cannot create payment record. Worker '{0}' is marked as {1} on {2:yyyy-MM-dd}. Payments can ONLY be created for workers marked as PRESENT.",
                        att.WorkerName ?? ("Worker #" + workerId), att.AttendanceStatus, workDate));
            }

            // Check if payment already exists
            WorkerPayment existing = _laborRepository.GetPaymentByAttendanceId(attendanceId);
            if (existing != null)
            {
                return Result<int>.Failure("A payment record already exists for this attendance record.");
            }

            if (dailyRate < 0)
            {
                return Result<int>.Failure("Daily wage rate cannot be negative.");
            }

            if (amountPaid < 0)
            {
                return Result<int>.Failure("Amount paid cannot be negative.");
            }

            if (amountPaid > dailyRate)
            {
                return Result<int>.Failure(string.Format("Amount paid ({0:N2}) cannot exceed amount owed ({1:N2}).", amountPaid, dailyRate));
            }

            string paymentStatus = (amountPaid >= dailyRate) ? "PAID" : "PENDING";

            WorkerPayment payment = new WorkerPayment
            {
                ProjectId = projectId,
                WorkerId = workerId,
                AttendanceId = attendanceId,
                WorkDate = workDate.Date,
                DailyRate = dailyRate,
                AmountOwed = dailyRate,
                AmountPaid = amountPaid,
                PaymentStatus = paymentStatus,
                PaidBy = CurrentUserId,
                Notes = notes != null ? notes.Trim() : null
            };

            try
            {
                int id = _laborRepository.SavePayment(payment);
                return Result<int>.Success(id);
            }
            catch (ValidationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Error creating wage payment: " + ex.Message);
            }
        }

        public Result SettlePayment(int paymentId, decimal totalAmountPaid, decimal amountOwed)
        {
            if (totalAmountPaid < 0)
            {
                return Result.Failure("Amount paid cannot be negative.");
            }

            if (totalAmountPaid > amountOwed)
            {
                return Result.Failure(string.Format("Amount paid ({0:N2}) cannot exceed amount owed ({1:N2}).", totalAmountPaid, amountOwed));
            }

            string status = (totalAmountPaid >= amountOwed) ? "PAID" : "PENDING";
            int paidBy = CurrentUserId.HasValue ? CurrentUserId.Value : 1;

            try
            {
                bool ok = _laborRepository.UpdatePaymentStatus(paymentId, status, totalAmountPaid, paidBy);
                return ok ? Result.Success() : Result.Failure("Payment record not found.");
            }
            catch (Exception ex)
            {
                return Result.Failure("Error updating payment: " + ex.Message);
            }
        }
    }
}
