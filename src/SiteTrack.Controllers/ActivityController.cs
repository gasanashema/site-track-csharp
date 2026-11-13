using System;
using System.Collections.Generic;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Repositories;

namespace SiteTrack.Controllers
{
    public class ActivityController : BaseController
    {
        private readonly IProjectRepository _projectRepository;

        public ActivityController() : this(new ProjectRepository())
        {
        }

        public ActivityController(IProjectRepository projectRepository)
        {
            this._projectRepository = projectRepository ?? new ProjectRepository();
        }

        public bool CanAccessProject(int projectId)
        {
            if (IsAdmin) return true;
            if (!CurrentUserId.HasValue) return false;

            List<Project> assigned = _projectRepository.GetByManager(CurrentUserId.Value);
            return assigned.Exists(delegate(Project p) { return p.Id == projectId; });
        }

        public List<ProjectActivity> GetActivities(int projectId)
        {
            if (!CanAccessProject(projectId))
            {
                return new List<ProjectActivity>();
            }
            return _projectRepository.GetActivities(projectId);
        }

        public Result<int> RecordActivity(int projectId, string title, string description, DateTime date, int progressPercentage)
        {
            if (!CanAccessProject(projectId))
            {
                return Result<int>.Failure("Access Denied: You are not assigned to record activities for this project.");
            }

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(title.Trim()))
            {
                return Result<int>.Failure("Activity title is required.");
            }

            // Technical Rule 5: Progress percentage constrained between 0 and 100
            if (progressPercentage < 0 || progressPercentage > 100)
            {
                return Result<int>.Failure(
                    string.Format("Technical Rule 5 Violation: Progress percentage ({0}%) must be an integer between 0 and 100 inclusive.", progressPercentage));
            }

            Project project = _projectRepository.GetById(projectId);
            if (project == null)
            {
                return Result<int>.Failure("Project does not exist.");
            }

            ProjectActivity activity = new ProjectActivity
            {
                ProjectId = projectId,
                ActivityTitle = title.Trim(),
                ActivityDescription = description != null ? description.Trim() : null,
                ActivityDate = date.Date,
                ProgressPercentage = progressPercentage,
                RecordedBy = CurrentUserId
            };

            try
            {
                int id = _projectRepository.InsertActivity(activity);
                return Result<int>.Success(id);
            }
            catch (ValidationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Error recording activity: " + ex.Message);
            }
        }

        public int GetLatestProgress(int projectId)
        {
            List<ProjectActivity> activities = GetActivities(projectId);
            if (activities == null || activities.Count == 0) return 0;
            return activities[0].ProgressPercentage;
        }
    }
}
