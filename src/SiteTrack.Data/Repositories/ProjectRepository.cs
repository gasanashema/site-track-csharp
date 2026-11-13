using System;
using System.Collections.Generic;
using System.Linq;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Connection;

namespace SiteTrack.Data.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly SiteTrackDataStore _store;

        public ProjectRepository()
        {
            this._store = SiteTrackDataStore.Instance;
        }

        public Project GetById(int id)
        {
            lock (_store.SyncRoot)
            {
                return _store.Projects.FirstOrDefault(delegate(Project p) { return p.Id == id; });
            }
        }

        public List<Project> GetAll()
        {
            lock (_store.SyncRoot)
            {
                return _store.Projects.OrderBy(delegate(Project p) { return p.ProjectName; }).ToList();
            }
        }

        public List<Project> GetByManager(int userId)
        {
            lock (_store.SyncRoot)
            {
                List<int> assignedProjectIds = _store.ProjectManagers
                    .Where(delegate(ProjectManager pm) { return pm.UserId == userId && string.Equals(pm.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase); })
                    .Select(delegate(ProjectManager pm) { return pm.ProjectId; })
                    .Distinct()
                    .ToList();

                return _store.Projects
                    .Where(delegate(Project p) { return assignedProjectIds.Contains(p.Id); })
                    .OrderBy(delegate(Project p) { return p.ProjectName; })
                    .ToList();
            }
        }

        public int Insert(Project project)
        {
            if (project == null) throw new ArgumentNullException("project");

            if (project.ExpectedEndDate.HasValue && project.ExpectedEndDate.Value < project.StartDate)
            {
                throw new ValidationException("ExpectedEndDate", "Expected end date cannot precede start date.");
            }

            lock (_store.SyncRoot)
            {
                project.Id = _store.NextProjectId();
                project.CreatedAt = DateTime.Now;
                project.UpdatedAt = DateTime.Now;
                _store.Projects.Add(project);
                return project.Id;
            }
        }

        public bool Update(Project project)
        {
            if (project == null) throw new ArgumentNullException("project");

            if (project.ExpectedEndDate.HasValue && project.ExpectedEndDate.Value < project.StartDate)
            {
                throw new ValidationException("ExpectedEndDate", "Expected end date cannot precede start date.");
            }

            lock (_store.SyncRoot)
            {
                Project existing = _store.Projects.FirstOrDefault(delegate(Project p) { return p.Id == project.Id; });
                if (existing == null) return false;

                existing.ProjectName = project.ProjectName;
                existing.Location = project.Location;
                existing.Description = project.Description;
                existing.StartDate = project.StartDate;
                existing.ExpectedEndDate = project.ExpectedEndDate;
                existing.Status = project.Status;
                existing.UpdatedAt = DateTime.Now;
                return true;
            }
        }

        public bool Delete(int projectId)
        {
            lock (_store.SyncRoot)
            {
                if (HasActiveChildRecords(projectId))
                {
                    throw new ValidationException("ProjectId", "Cannot delete or cancel project: active material stock, usage records, or labor records exist.");
                }

                Project existing = _store.Projects.FirstOrDefault(delegate(Project p) { return p.Id == projectId; });
                if (existing == null) return false;

                _store.Projects.Remove(existing);
                _store.ProjectManagers.RemoveAll(delegate(ProjectManager pm) { return pm.ProjectId == projectId; });
                return true;
            }
        }

        public bool HasActiveChildRecords(int projectId)
        {
            lock (_store.SyncRoot)
            {
                bool hasStock = _store.ProjectMaterialStocks.Any(delegate(ProjectMaterialStock s) { return s.ProjectId == projectId && s.QuantityAvailable > 0; });
                bool hasUsage = _store.MaterialUsages.Any(delegate(MaterialUsage u) { return u.ProjectId == projectId; });
                bool hasAttendance = _store.WorkerAttendances.Any(delegate(WorkerAttendance a) { return a.ProjectId == projectId; });
                bool hasPendingPayments = _store.WorkerPayments.Any(delegate(WorkerPayment p) { return p.ProjectId == projectId && string.Equals(p.PaymentStatus, "PENDING", StringComparison.OrdinalIgnoreCase); });

                return hasStock || hasUsage || hasAttendance || hasPendingPayments;
            }
        }

        public List<ProjectManager> GetManagersForProject(int projectId)
        {
            lock (_store.SyncRoot)
            {
                List<ProjectManager> list = _store.ProjectManagers
                    .Where(delegate(ProjectManager pm) { return pm.ProjectId == projectId; })
                    .ToList();

                foreach (ProjectManager pm in list)
                {
                    User user = _store.Users.FirstOrDefault(delegate(User u) { return u.Id == pm.UserId; });
                    if (user != null)
                    {
                        pm.ManagerName = user.FullName;
                        pm.ManagerEmail = user.Email;
                    }
                    Project proj = _store.Projects.FirstOrDefault(delegate(Project p) { return p.Id == pm.ProjectId; });
                    if (proj != null)
                    {
                        pm.ProjectName = proj.ProjectName;
                    }
                }
                return list;
            }
        }

        public bool AssignManager(int projectId, int userId)
        {
            lock (_store.SyncRoot)
            {
                ProjectManager existing = _store.ProjectManagers.FirstOrDefault(delegate(ProjectManager pm) {
                    return pm.ProjectId == projectId && pm.UserId == userId;
                });

                if (existing != null)
                {
                    existing.Status = "ACTIVE";
                    existing.UpdatedAt = DateTime.Now;
                    return true;
                }

                ProjectManager pmNew = new ProjectManager
                {
                    Id = _store.NextProjectManagerId(),
                    ProjectId = projectId,
                    UserId = userId,
                    AssignedDate = DateTime.Today,
                    Status = "ACTIVE"
                };
                _store.ProjectManagers.Add(pmNew);
                return true;
            }
        }

        public bool RemoveManager(int projectId, int userId)
        {
            lock (_store.SyncRoot)
            {
                int removed = _store.ProjectManagers.RemoveAll(delegate(ProjectManager pm) {
                    return pm.ProjectId == projectId && pm.UserId == userId;
                });
                return removed > 0;
            }
        }

        public List<ProjectActivity> GetActivities(int projectId)
        {
            lock (_store.SyncRoot)
            {
                List<ProjectActivity> list = _store.ProjectActivities
                    .Where(delegate(ProjectActivity a) { return a.ProjectId == projectId; })
                    .OrderByDescending(delegate(ProjectActivity a) { return a.ActivityDate; })
                    .ToList();

                foreach (ProjectActivity act in list)
                {
                    if (act.RecordedBy.HasValue)
                    {
                        User user = _store.Users.FirstOrDefault(delegate(User u) { return u.Id == act.RecordedBy.Value; });
                        if (user != null) act.RecordedByName = user.FullName;
                    }
                }
                return list;
            }
        }

        public int InsertActivity(ProjectActivity activity)
        {
            if (activity == null) throw new ArgumentNullException("activity");

            if (activity.ProgressPercentage < 0 || activity.ProgressPercentage > 100)
            {
                throw new ValidationException("ProgressPercentage", "Progress percentage must be between 0 and 100.");
            }

            lock (_store.SyncRoot)
            {
                activity.Id = _store.NextActivityId();
                activity.CreatedAt = DateTime.Now;
                activity.UpdatedAt = DateTime.Now;
                _store.ProjectActivities.Add(activity);
                return activity.Id;
            }
        }
    }
}
