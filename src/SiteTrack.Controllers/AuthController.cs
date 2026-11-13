using System;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Core.Security;
using SiteTrack.Data.Repositories;

namespace SiteTrack.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IUserRepository _userRepository;

        public AuthController() : this(new UserRepository())
        {
        }

        public AuthController(IUserRepository userRepository)
        {
            this._userRepository = userRepository ?? new UserRepository();
        }

        public Result<User> Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(username.Trim()))
            {
                return Result<User>.Failure("Username is required.");
            }

            if (string.IsNullOrEmpty(password))
            {
                return Result<User>.Failure("Password is required.");
            }

            string trimmedUser = username.Trim();
            User user = _userRepository.GetByUsername(trimmedUser);

            if (user == null)
            {
                _userRepository.LogNotification(new NotificationLog
                {
                    EventType = "LOGIN_FAILED",
                    Message = string.Format("Failed login attempt for nonexistent user: {0}", trimmedUser),
                    Channel = "SYSTEM",
                    Status = "LOGGED"
                });
                return Result<User>.Failure("Invalid username or password.");
            }

            if (!user.IsActive)
            {
                _userRepository.LogNotification(new NotificationLog
                {
                    UserId = user.Id,
                    EventType = "LOGIN_BLOCKED",
                    Message = string.Format("Login blocked: user '{0}' is inactive.", user.Username),
                    Channel = "SYSTEM",
                    Status = "LOGGED"
                });
                return Result<User>.Failure("Your account has been deactivated. Please contact an Administrator.");
            }

            bool passwordValid = PasswordHasher.VerifyPassword(password, user.Password);
            if (!passwordValid)
            {
                _userRepository.LogNotification(new NotificationLog
                {
                    UserId = user.Id,
                    EventType = "LOGIN_FAILED",
                    Message = string.Format("Incorrect password entered for user: {0}", user.Username),
                    Channel = "SYSTEM",
                    Status = "LOGGED"
                });
                return Result<User>.Failure("Invalid username or password.");
            }

            // Successful Login
            SetSession(user);

            _userRepository.LogNotification(new NotificationLog
            {
                UserId = user.Id,
                EventType = "LOGIN_SUCCESS",
                Message = string.Format("User '{0}' logged in successfully as {1}.", user.Username, user.Role),
                Channel = "SYSTEM",
                Status = "LOGGED"
            });

            return Result<User>.Success(user);
        }

        public void Logout()
        {
            if (CurrentUser != null)
            {
                _userRepository.LogNotification(new NotificationLog
                {
                    UserId = CurrentUser.Id,
                    EventType = "LOGOUT",
                    Message = string.Format("User '{0}' logged out.", CurrentUser.Username),
                    Channel = "SYSTEM",
                    Status = "LOGGED"
                });
            }
            ClearSession();
        }
    }
}
