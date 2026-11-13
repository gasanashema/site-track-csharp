using System;
using System.Collections.Generic;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Repositories;

namespace SiteTrack.Controllers
{
    public class ProjectController : BaseController
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;

        public ProjectController() : this(new ProjectRepository(), new UserRepository())
        {
        }

        public ProjectController(IProjectRepository projectRepository, IUserRepository userRepository)
        {
            this._projectRepository = projectRepository ?? new ProjectRepository();
            this._userRepository = userRepository ?? new UserRepository();
        }

        public List<Project> GetProjectsForCurrentUser()
        {
            if (IsAdmin)
            {
                return _projectRepository.GetAll();
            }

            if (CurrentUserId.HasValue)
            {
                return _projectRepository.GetByManager(CurrentUserId.Value);
            }

            return new List<Project>();
        }

        public Project GetProjectById(int id)
        {
            Project proj = _projectRepository.GetById(id);
            if (proj == null) return null;

            // Authorization check for site managers
            if (!IsAdmin && CurrentUserId.HasValue)
            {
                List<Project> assigned = _projectRepository.GetByManager(CurrentUserId.Value);
                bool hasAccess = assigned.Exists(delegate(Project p) { return p.Id == id; });
                if (!hasAccess) return null; // Scoped restriction
            }

            return proj;
        }

        public Result<int> CreateProject(string projectName, string location, string description, DateTime startDate, DateTime? expectedEndDate, string status)
        {
            if (!IsAdmin)
            {
                return Result<int>.Failure("Access Denied: Only Administrators can create new projects.");
            }

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(projectName.Trim()))
            {
                return Result<int>.Failure("Project name is required.");
            }

            if (string.IsNullOrEmpty(location) || string.IsNullOrEmpty(location.Trim()))
            {
                return Result<int>.Failure("Location is required.");
            }

            if (expectedEndDate.HasValue && expectedEndDate.Value < startDate)
            {
                return Result<int>.Failure("Expected end date cannot be earlier than start date.");
            }

            Project proj = new Project
            {
                ProjectName = projectName.Trim(),
                Location = location.Trim(),
                Description = description != null ? description.Trim() : string.Empty,
                StartDate = startDate.Date,
                ExpectedEndDate = expectedEndDate.HasValue ? (DateTime?)expectedEndDate.Value.Date : null,
                Status = string.IsNullOrEmpty(status) ? "Planning" : status,
                CreatedBy = CurrentUserId
            };

            try
            {
                int id = _projectRepository.Insert(proj);
                return Result<int>.Success(id);
            }
            catch (ValidationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Error creating project: " + ex.Message);
            }
        }

        public Result UpdateProject(int id, string projectName, string location, string description, DateTime startDate, DateTime? expectedEndDate, string status)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can modify project details.");
            }

            Project existing = _projectRepository.GetById(id);
            if (existing == null)
            {
                return Result.Failure("Project not found.");
            }

            if (string.IsNullOrEmpty(projectName) || string.IsNullOrEmpty(projectName.Trim()))
            {
                return Result.Failure("Project name is required.");
            }

            if (string.IsNullOrEmpty(location) || string.IsNullOrEmpty(location.Trim()))
            {
                return Result.Failure("Location is required.");
            }

            if (expectedEndDate.HasValue && expectedEndDate.Value < startDate)
            {
                return Result.Failure("Expected end date cannot be earlier than start date.");
            }

            existing.ProjectName = projectName.Trim();
            existing.Location = location.Trim();
            existing.Description = description != null ? description.Trim() : string.Empty;
            existing.StartDate = startDate.Date;
            existing.ExpectedEndDate = expectedEndDate.HasValue ? (DateTime?)expectedEndDate.Value.Date : null;
            existing.Status = status ?? existing.Status;

            try
            {
                bool success = _projectRepository.Update(existing);
                return success ? Result.Success() : Result.Failure("Failed to update project.");
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error updating project: " + ex.Message);
            }
        }

        public Result DeleteProject(int id)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can delete projects.");
            }

            // Business Rule 5: Cannot delete project with active transactions
            if (_projectRepository.HasActiveChildRecords(id))
            {
                return Result.Failure("Cannot delete project: Active material stock, usage records, or labor records exist for this project.");
            }

            try
            {
                bool deleted = _projectRepository.Delete(id);
                return deleted ? Result.Success() : Result.Failure("Project not found or could not be deleted.");
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error deleting project: " + ex.Message);
            }
        }

        public List<ProjectManager> GetProjectManagers(int projectId)
        {
            return _projectRepository.GetManagersForProject(projectId);
        }

        public Result AssignManager(int projectId, int userId)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can assign site managers.");
            }

            User user = _userRepository.GetById(userId);
            if (user == null || !user.IsActive)
            {
                return Result.Failure("Selected user does not exist or is inactive.");
            }

            bool success = _projectRepository.AssignManager(projectId, userId);
            return success ? Result.Success() : Result.Failure("Failed to assign manager.");
        }

        public Result RemoveManager(int projectId, int userId)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can remove site managers.");
            }

            bool success = _projectRepository.RemoveManager(projectId, userId);
            return success ? Result.Success() : Result.Failure("Failed to remove manager.");
        }
    }
}

