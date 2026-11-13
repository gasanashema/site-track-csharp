using System;
using SiteTrack.Core.Models;

namespace SiteTrack.Controllers
{
    public abstract class BaseController
    {
        public static User CurrentUser { get; set; }

        public static bool IsAuthenticated
        {
            get
            {
                return CurrentUser != null && CurrentUser.IsActive;
            }
        }

        public static bool IsAdmin
        {
            get
            {
                return IsAuthenticated && CurrentUser.IsAdmin;
            }
        }

        public static int? CurrentUserId
        {
            get
            {
                return CurrentUser != null ? (int?)CurrentUser.Id : null;
            }
        }

        public static void SetSession(User user)
        {
            CurrentUser = user;
        }

        public static void ClearSession()
        {
            CurrentUser = null;
        }
    }
}
