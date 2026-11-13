using System;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.UI.Forms;

namespace SiteTrack.UI
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            bool keepRunning = true;
            while (keepRunning)
            {
                using (FormLogin loginForm = new FormLogin())
                {
                    DialogResult loginResult = loginForm.ShowDialog();
                    if (loginResult != DialogResult.OK)
                    {
                        // User cancelled or closed login dialog -> exit application
                        keepRunning = false;
                        break;
                    }

                    // User authenticated successfully -> open Main Shell
                    using (FormMainShell shellForm = new FormMainShell())
                    {
                        DialogResult shellResult = shellForm.ShowDialog();
                        if (shellResult == DialogResult.Retry)
                        {
                            // Sign Out clicked -> loop back to login form
                            keepRunning = true;
                        }
                        else
                        {
                            // Shell closed -> exit application
                            keepRunning = false;
                        }
                    }
                }
            }
        }
    }
}
