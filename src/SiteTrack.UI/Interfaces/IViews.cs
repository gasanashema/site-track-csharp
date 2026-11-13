using System;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Interfaces
{
    public interface ILoginView
    {
        string Username { get; }
        string Password { get; }
        void ShowError(string message);
        void ClearError();
        void SetLoading(bool isLoading);
    }

    public interface IMainShellView
    {
        void SetCurrentUser(User user);
        void NavigateTo(string moduleName);
    }
}

