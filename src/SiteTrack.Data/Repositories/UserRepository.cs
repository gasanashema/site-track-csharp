using System;
using System.Collections.Generic;
using System.Linq;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Connection;

namespace SiteTrack.Data.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SiteTrackDataStore _store;

        public UserRepository()
        {
            this._store = SiteTrackDataStore.Instance;
        }

        public User GetById(int id)
        {
            lock (_store.SyncRoot)
            {
                return _store.Users.FirstOrDefault(delegate(User u) { return u.Id == id; });
            }
        }

        public User GetByUsername(string username)
        {
            if (string.IsNullOrEmpty(username)) return null;
            lock (_store.SyncRoot)
            {
                return _store.Users.FirstOrDefault(delegate(User u) {
                    return string.Equals(u.Username, username.Trim(), StringComparison.OrdinalIgnoreCase);
                });
            }
        }

        public User GetByEmail(string email)
        {
            if (string.IsNullOrEmpty(email)) return null;
            lock (_store.SyncRoot)
            {
                return _store.Users.FirstOrDefault(delegate(User u) {
                    return string.Equals(u.Email, email.Trim(), StringComparison.OrdinalIgnoreCase);
                });
            }
        }

        public List<User> GetAll()
        {
            lock (_store.SyncRoot)
            {
                return _store.Users.OrderBy(delegate(User u) { return u.FullName; }).ToList();
            }
        }

        public int Insert(User user)
        {
            if (user == null) throw new ArgumentNullException("user");

            lock (_store.SyncRoot)
            {
                // Uniqueness checks
                if (_store.Users.Any(delegate(User u) { return string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase); }))
                {
                    throw new ValidationException("Username", string.Format("Username '{0}' is already taken.", user.Username));
                }

                if (_store.Users.Any(delegate(User u) { return string.Equals(u.Email, user.Email, StringComparison.OrdinalIgnoreCase); }))
                {
                    throw new ValidationException("Email", string.Format("Email '{0}' is already registered.", user.Email));
                }

                user.Id = _store.NextUserId();
                user.CreatedAt = DateTime.Now;
                user.UpdatedAt = DateTime.Now;
                _store.Users.Add(user);
                return user.Id;
            }
        }

        public bool Update(User user)
        {
            if (user == null) throw new ArgumentNullException("user");

            lock (_store.SyncRoot)
            {
                User existing = _store.Users.FirstOrDefault(delegate(User u) { return u.Id == user.Id; });
                if (existing == null) return false;

                // Uniqueness checks excluding self
                if (_store.Users.Any(delegate(User u) { return u.Id != user.Id && string.Equals(u.Username, user.Username, StringComparison.OrdinalIgnoreCase); }))
                {
                    throw new ValidationException("Username", string.Format("Username '{0}' is already taken.", user.Username));
                }

                if (_store.Users.Any(delegate(User u) { return u.Id != user.Id && string.Equals(u.Email, user.Email, StringComparison.OrdinalIgnoreCase); }))
                {
                    throw new ValidationException("Email", string.Format("Email '{0}' is already registered.", user.Email));
                }

                existing.FullName = user.FullName;
                existing.Username = user.Username;
                existing.Email = user.Email;
                if (!string.IsNullOrEmpty(user.Password))
                {
                    existing.Password = user.Password;
                }
                existing.Role = user.Role;
                existing.Phone = user.Phone;
                existing.Status = user.Status;
                existing.UpdatedAt = DateTime.Now;
                return true;
            }
        }

        public bool SetStatus(int userId, string status)
        {
            lock (_store.SyncRoot)
            {
                User existing = _store.Users.FirstOrDefault(delegate(User u) { return u.Id == userId; });
                if (existing == null) return false;
                existing.Status = status;
                existing.UpdatedAt = DateTime.Now;
                return true;
            }
        }

        public bool SaveOtp(OtpVerification otp)
        {
            if (otp == null) throw new ArgumentNullException("otp");
            lock (_store.SyncRoot)
            {
                otp.Id = _store.NextOtpId();
                otp.CreatedAt = DateTime.Now;
                _store.OtpVerifications.Add(otp);
                return true;
            }
        }

        public OtpVerification GetValidOtp(int userId, string otpCode)
        {
            if (string.IsNullOrEmpty(otpCode)) return null;
            lock (_store.SyncRoot)
            {
                return _store.OtpVerifications
                    .Where(delegate(OtpVerification o) {
                        return o.UserId == userId &&
                               string.Equals(o.OtpCode, otpCode.Trim()) &&
                               !o.IsUsed &&
                               DateTime.Now <= o.ExpiresAt;
                    })
                    .OrderByDescending(delegate(OtpVerification o) { return o.CreatedAt; })
                    .FirstOrDefault();
            }
        }

        public bool MarkOtpUsed(int otpId)
        {
            lock (_store.SyncRoot)
            {
                OtpVerification existing = _store.OtpVerifications.FirstOrDefault(delegate(OtpVerification o) { return o.Id == otpId; });
                if (existing == null) return false;
                existing.IsUsed = true;
                return true;
            }
        }

        public void LogNotification(NotificationLog log)
        {
            if (log == null) return;
            lock (_store.SyncRoot)
            {
                log.Id = _store.NextNotificationId();
                log.CreatedAt = DateTime.Now;
                _store.NotificationLogs.Add(log);
            }
        }

        public List<NotificationLog> GetNotificationLogs(int? userId)
        {
            lock (_store.SyncRoot)
            {
                IEnumerable<NotificationLog> query = _store.NotificationLogs;
                if (userId.HasValue)
                {
                    query = query.Where(delegate(NotificationLog l) { return l.UserId == userId.Value; });
                }
                return query.OrderByDescending(delegate(NotificationLog l) { return l.CreatedAt; }).ToList();
            }
        }
    }
}
