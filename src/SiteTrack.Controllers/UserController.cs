using System;
using System.Collections.Generic;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Core.Security;
using SiteTrack.Data.Repositories;

namespace SiteTrack.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserRepository _userRepository;

        public UserController() : this(new UserRepository())
        {
        }

        public UserController(IUserRepository userRepository)
        {
            this._userRepository = userRepository ?? new UserRepository();
        }

        public List<User> GetAllUsers()
        {
            return _userRepository.GetAll();
        }

        public User GetUserById(int id)
        {
            return _userRepository.GetById(id);
        }

        public Result<int> CreateUser(string fullName, string username, string email, string password, string role, string phone)
        {
            if (!IsAdmin)
            {
                return Result<int>.Failure("Access Denied: Only Administrators can create user accounts.");
            }

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(fullName.Trim()))
            {
                return Result<int>.Failure("Full name is required.");
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(username.Trim()))
            {
                return Result<int>.Failure("Username is required.");
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(email.Trim()))
            {
                return Result<int>.Failure("Email address is required.");
            }

            if (string.IsNullOrEmpty(password) || password.Length < 6)
            {
                return Result<int>.Failure("Password is required and must be at least 6 characters.");
            }

            if (string.IsNullOrEmpty(role))
            {
                role = "SiteManager";
            }

            User user = new User
            {
                FullName = fullName.Trim(),
                Username = username.Trim().ToLowerInvariant(),
                Email = email.Trim().ToLowerInvariant(),
                Password = PasswordHasher.HashPassword(password),
                Role = role,
                Phone = phone != null ? phone.Trim() : string.Empty,
                Status = "ACTIVE"
            };

            try
            {
                int newId = _userRepository.Insert(user);
                return Result<int>.Success(newId);
            }
            catch (ValidationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Error creating user: " + ex.Message);
            }
        }

        public Result UpdateUser(int id, string fullName, string username, string email, string role, string phone, string newPassword)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can modify user accounts.");
            }

            User existing = _userRepository.GetById(id);
            if (existing == null)
            {
                return Result.Failure("User not found.");
            }

            if (string.IsNullOrEmpty(fullName) || string.IsNullOrEmpty(fullName.Trim()))
            {
                return Result.Failure("Full name is required.");
            }

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(username.Trim()))
            {
                return Result.Failure("Username is required.");
            }

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(email.Trim()))
            {
                return Result.Failure("Email address is required.");
            }

            existing.FullName = fullName.Trim();
            existing.Username = username.Trim().ToLowerInvariant();
            existing.Email = email.Trim().ToLowerInvariant();
            existing.Role = role ?? existing.Role;
            existing.Phone = phone != null ? phone.Trim() : string.Empty;

            if (!string.IsNullOrEmpty(newPassword))
            {
                if (newPassword.Length < 6)
                {
                    return Result.Failure("New password must be at least 6 characters.");
                }
                existing.Password = PasswordHasher.HashPassword(newPassword);
            }

            try
            {
                bool updated = _userRepository.Update(existing);
                return updated ? Result.Success() : Result.Failure("Failed to update user.");
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error updating user: " + ex.Message);
            }
        }

        public Result ToggleUserStatus(int id)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can toggle account status.");
            }

            if (CurrentUser != null && CurrentUser.Id == id)
            {
                return Result.Failure("Operation Blocked: You cannot deactivate your own active account.");
            }

            User existing = _userRepository.GetById(id);
            if (existing == null)
            {
                return Result.Failure("User not found.");
            }

            string newStatus = string.Equals(existing.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase) ? "INACTIVE" : "ACTIVE";
            bool success = _userRepository.SetStatus(id, newStatus);
            return success ? Result.Success() : Result.Failure("Failed to change user status.");
        }
    }
}
