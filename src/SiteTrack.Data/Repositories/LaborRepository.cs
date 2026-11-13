using System;
using System.Collections.Generic;
using System.Linq;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Connection;

namespace SiteTrack.Data.Repositories
{
    public class LaborRepository : ILaborRepository
    {
        private readonly SiteTrackDataStore _store;

        public LaborRepository()
        {
            this._store = SiteTrackDataStore.Instance;
        }

        public List<WorkerType> GetAllWorkerTypes()
        {
            lock (_store.SyncRoot)
            {
                return _store.WorkerTypes.OrderBy(delegate(WorkerType t) { return t.TypeName; }).ToList();
            }
        }

        public WorkerType GetWorkerTypeById(int id)
        {
            lock (_store.SyncRoot)
            {
                return _store.WorkerTypes.FirstOrDefault(delegate(WorkerType t) { return t.Id == id; });
            }
        }

        public int InsertWorkerType(WorkerType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (type.DefaultDailyRate < 0) throw new ValidationException("DefaultDailyRate", "Daily rate cannot be negative.");

            lock (_store.SyncRoot)
            {
                if (_store.WorkerTypes.Any(delegate(WorkerType t) { return string.Equals(t.TypeName, type.TypeName, StringComparison.OrdinalIgnoreCase); }))
                {
                    throw new ValidationException("TypeName", string.Format("Worker type '{0}' already exists.", type.TypeName));
                }

                type.Id = _store.NextWorkerTypeId();
                type.CreatedAt = DateTime.Now;
                type.UpdatedAt = DateTime.Now;
                _store.WorkerTypes.Add(type);
                return type.Id;
            }
        }

        public bool UpdateWorkerType(WorkerType type)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (type.DefaultDailyRate < 0) throw new ValidationException("DefaultDailyRate", "Daily rate cannot be negative.");

            lock (_store.SyncRoot)
            {
                WorkerType existing = _store.WorkerTypes.FirstOrDefault(delegate(WorkerType t) { return t.Id == type.Id; });
                if (existing == null) return false;

                if (_store.WorkerTypes.Any(delegate(WorkerType t) { return t.Id != type.Id && string.Equals(t.TypeName, type.TypeName, StringComparison.OrdinalIgnoreCase); }))
                {
                    throw new ValidationException("TypeName", string.Format("Worker type '{0}' already exists.", type.TypeName));
                }

                existing.TypeName = type.TypeName;
                existing.DefaultDailyRate = type.DefaultDailyRate;
                existing.Description = type.Description;
                existing.UpdatedAt = DateTime.Now;
                return true;
            }
        }

        public bool DeleteWorkerType(int id)
        {
            lock (_store.SyncRoot)
            {
                if (_store.SiteWorkers.Any(delegate(SiteWorker w) { return w.WorkerTypeId == id; }))
                {
                    throw new ValidationException("WorkerTypeId", "Cannot delete worker type: site workers are assigned to this category.");
                }

                WorkerType existing = _store.WorkerTypes.FirstOrDefault(delegate(WorkerType t) { return t.Id == id; });
                if (existing == null) return false;

                _store.WorkerTypes.Remove(existing);
                return true;
            }
        }

        public List<SiteWorker> GetAllWorkers(int? workerTypeId, string status)
        {
            lock (_store.SyncRoot)
            {
                IEnumerable<SiteWorker> query = _store.SiteWorkers;
                if (workerTypeId.HasValue) query = query.Where(delegate(SiteWorker w) { return w.WorkerTypeId == workerTypeId.Value; });
                if (!string.IsNullOrEmpty(status)) query = query.Where(delegate(SiteWorker w) { return string.Equals(w.Status, status, StringComparison.OrdinalIgnoreCase); });

                List<SiteWorker> list = query.OrderBy(delegate(SiteWorker w) { return w.FullName; }).ToList();
                foreach (SiteWorker w in list)
                {
                    WorkerType wt = _store.WorkerTypes.FirstOrDefault(delegate(WorkerType t) { return t.Id == w.WorkerTypeId; });
                    if (wt != null) w.WorkerTypeName = wt.TypeName;
                }
                return list;
            }
        }

        public SiteWorker GetWorkerById(int id)
        {
            lock (_store.SyncRoot)
            {
                SiteWorker w = _store.SiteWorkers.FirstOrDefault(delegate(SiteWorker item) { return item.Id == id; });
                if (w != null)
                {
                    WorkerType wt = _store.WorkerTypes.FirstOrDefault(delegate(WorkerType t) { return t.Id == w.WorkerTypeId; });
                    if (wt != null) w.WorkerTypeName = wt.TypeName;
                }
                return w;
            }
        }

        public int InsertWorker(SiteWorker worker)
        {
            if (worker == null) throw new ArgumentNullException("worker");
            if (worker.DailyRate < 0) throw new ValidationException("DailyRate", "Daily wage rate cannot be negative.");

            lock (_store.SyncRoot)
            {
                worker.Id = _store.NextWorkerId();
                worker.CreatedAt = DateTime.Now;
                worker.UpdatedAt = DateTime.Now;
                _store.SiteWorkers.Add(worker);
                return worker.Id;
            }
        }

        public bool UpdateWorker(SiteWorker worker)
        {
            if (worker == null) throw new ArgumentNullException("worker");
            if (worker.DailyRate < 0) throw new ValidationException("DailyRate", "Daily wage rate cannot be negative.");

            lock (_store.SyncRoot)
            {
                SiteWorker existing = _store.SiteWorkers.FirstOrDefault(delegate(SiteWorker w) { return w.Id == worker.Id; });
                if (existing == null) return false;

                existing.WorkerTypeId = worker.WorkerTypeId;
                existing.FullName = worker.FullName;
                existing.Phone = worker.Phone;
                existing.DailyRate = worker.DailyRate;
                existing.Status = worker.Status;
                existing.UpdatedAt = DateTime.Now;
                return true;
            }
        }

        public List<WorkerAttendance> GetAttendance(int projectId, DateTime workDate)
        {
            DateTime targetDate = workDate.Date;
            lock (_store.SyncRoot)
            {
                List<WorkerAttendance> list = _store.WorkerAttendances
                    .Where(delegate(WorkerAttendance a) { return a.ProjectId == projectId && a.WorkDate.Date == targetDate; })
                    .OrderBy(delegate(WorkerAttendance a) { return a.Id; })
                    .ToList();

                foreach (WorkerAttendance a in list)
                {
                    SiteWorker w = _store.SiteWorkers.FirstOrDefault(delegate(SiteWorker item) { return item.Id == a.WorkerId; });
                    if (w != null)
                    {
                        a.WorkerName = w.FullName;
                        a.WorkerPhone = w.Phone;
                        a.DailyRate = w.DailyRate;
                        WorkerType wt = _store.WorkerTypes.FirstOrDefault(delegate(WorkerType t) { return t.Id == w.WorkerTypeId; });
                        if (wt != null) a.WorkerTypeName = wt.TypeName;
                    }
                }
                return list;
            }
        }

        public WorkerAttendance GetAttendanceRecord(int projectId, int workerId, DateTime workDate)
        {
            DateTime targetDate = workDate.Date;
            lock (_store.SyncRoot)
            {
                return _store.WorkerAttendances.FirstOrDefault(delegate(WorkerAttendance a) {
                    return a.ProjectId == projectId && a.WorkerId == workerId && a.WorkDate.Date == targetDate;
                });
            }
        }

        public int SaveAttendance(WorkerAttendance attendance)
        {
            if (attendance == null) throw new ArgumentNullException("attendance");

            DateTime targetDate = attendance.WorkDate.Date;
            lock (_store.SyncRoot)
            {
                // Verify worker is active
                SiteWorker worker = _store.SiteWorkers.FirstOrDefault(delegate(SiteWorker w) { return w.Id == attendance.WorkerId; });
                if (worker == null || !string.Equals(worker.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    throw new ValidationException("WorkerId", "Cannot record attendance: worker is inactive or does not exist.");
                }

                WorkerAttendance existing = _store.WorkerAttendances.FirstOrDefault(delegate(WorkerAttendance a) {
                    return a.ProjectId == attendance.ProjectId && a.WorkerId == attendance.WorkerId && a.WorkDate.Date == targetDate;
                });

                if (existing != null)
                {
                    existing.AttendanceStatus = attendance.AttendanceStatus;
                    existing.WorkDescription = attendance.WorkDescription;
                    existing.RecordedBy = attendance.RecordedBy;
                    existing.UpdatedAt = DateTime.Now;
                    return existing.Id;
                }

                attendance.Id = _store.NextAttendanceId();
                attendance.WorkDate = targetDate;
                attendance.CreatedAt = DateTime.Now;
                attendance.UpdatedAt = DateTime.Now;
                _store.WorkerAttendances.Add(attendance);
                return attendance.Id;
            }
        }

        public List<WorkerPayment> GetPayments(int projectId, DateTime? startDate, DateTime? endDate, string status)
        {
            lock (_store.SyncRoot)
            {
                IEnumerable<WorkerPayment> query = _store.WorkerPayments
                    .Where(delegate(WorkerPayment p) { return p.ProjectId == projectId; });

                if (startDate.HasValue) query = query.Where(delegate(WorkerPayment p) { return p.WorkDate >= startDate.Value.Date; });
                if (endDate.HasValue) query = query.Where(delegate(WorkerPayment p) { return p.WorkDate <= endDate.Value.Date; });
                if (!string.IsNullOrEmpty(status)) query = query.Where(delegate(WorkerPayment p) { return string.Equals(p.PaymentStatus, status, StringComparison.OrdinalIgnoreCase); });

                List<WorkerPayment> list = query.OrderByDescending(delegate(WorkerPayment p) { return p.WorkDate; }).ToList();
                foreach (WorkerPayment p in list)
                {
                    SiteWorker w = _store.SiteWorkers.FirstOrDefault(delegate(SiteWorker item) { return item.Id == p.WorkerId; });
                    if (w != null)
                    {
                        p.WorkerName = w.FullName;
                        p.WorkerPhone = w.Phone;
                    }
                    if (p.PaidBy.HasValue)
                    {
                        User u = _store.Users.FirstOrDefault(delegate(User usr) { return usr.Id == p.PaidBy.Value; });
                        if (u != null) p.PaidByName = u.FullName;
                    }
                }
                return list;
            }
        }

        public WorkerPayment GetPaymentByAttendanceId(int attendanceId)
        {
            lock (_store.SyncRoot)
            {
                return _store.WorkerPayments.FirstOrDefault(delegate(WorkerPayment p) { return p.AttendanceId == attendanceId; });
            }
        }

        public int SavePayment(WorkerPayment payment)
        {
            if (payment == null) throw new ArgumentNullException("payment");

            lock (_store.SyncRoot)
            {
                // Business Rule 2: Gated Payment Creation (Attendance must exist and be PRESENT)
                WorkerAttendance attendance = _store.WorkerAttendances.FirstOrDefault(delegate(WorkerAttendance a) { return a.Id == payment.AttendanceId; });
                if (attendance == null || !attendance.IsPresent)
                {
                    throw new ValidationException("AttendanceId",
                        "Payment rejected: A payment record can ONLY be created for a worker marked as PRESENT on that date.");
                }

                // Check 1-to-1 uniqueness on attendance_id
                WorkerPayment existing = _store.WorkerPayments.FirstOrDefault(delegate(WorkerPayment p) { return p.AttendanceId == payment.AttendanceId; });
                if (existing != null)
                {
                    throw new ValidationException("AttendanceId", "A payment record already exists for this attendance record.");
                }

                payment.Id = _store.NextPaymentId();
                payment.CreatedAt = DateTime.Now;
                payment.UpdatedAt = DateTime.Now;
                _store.WorkerPayments.Add(payment);
                return payment.Id;
            }
        }

        public bool UpdatePaymentStatus(int paymentId, string status, decimal amountPaid, int paidBy)
        {
            lock (_store.SyncRoot)
            {
                WorkerPayment existing = _store.WorkerPayments.FirstOrDefault(delegate(WorkerPayment p) { return p.Id == paymentId; });
                if (existing == null) return false;

                existing.AmountPaid = amountPaid;
                existing.PaymentStatus = status;
                existing.PaidBy = paidBy;
                existing.UpdatedAt = DateTime.Now;
                return true;
            }
        }
    }
}

