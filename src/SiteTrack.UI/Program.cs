using System;
using System.Drawing;
using System.Windows.Forms;

namespace SiteTrack.UI
{
    public class FormMainShellPlaceholder : Form
    {
        public FormMainShellPlaceholder()
        {
            this.Text = "SiteTrack Construction Manager";
            this.Size = new Size(1024, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            Label lbl = new Label();
            lbl.Text = "SiteTrack Construction Manager (.NET WinForms MVC)\nFoundation initialized successfully.";
            lbl.AutoSize = false;
            lbl.Dock = DockStyle.Fill;
            lbl.TextAlign = ContentAlignment.MiddleCenter;
            lbl.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            this.Controls.Add(lbl);
        }
    }

    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMainShellPlaceholder());
        }
    }
}

