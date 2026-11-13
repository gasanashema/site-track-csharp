using System;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core.Models;
using SiteTrack.UI.Interfaces;

namespace SiteTrack.UI.Forms
{
    public class FormMainShell : Form, IMainShellView
    {
        private readonly AuthController _authController;

        private Panel pnlSidebar;
        private Panel pnlHeader;
        private Panel pnlContent;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatusUser;
        private ToolStripStatusLabel lblStatusRole;
        private ToolStripStatusLabel lblStatusDb;
        private ToolStripStatusLabel lblStatusTime;

        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;
        private Label lblUserBadge;

        // Navigation Buttons
        private Button btnNavDashboard;
        private Button btnNavProjects;
        private Button btnNavMaterials;
        private Button btnNavLabor;
        private Button btnNavActivities;
        private Button btnNavUsers;
        private Button btnNavReports;
        private Button btnNavSignOut;

        private Button _activeNavButton;

        public FormMainShell() : this(new AuthController())
        {
        }

        public FormMainShell(AuthController authController)
        {
            this._authController = authController ?? new AuthController();
            InitializeComponent();
            ApplyRoleRestrictions();
            NavigateTo("Dashboard");
        }

        private void InitializeComponent()
        {
            this.Text = "SiteTrack Construction Manager — AUCA DotNet Lab 3";
            this.Size = new Size(1200, 780);
            this.MinimumSize = new Size(1024, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            this.BackColor = Color.FromArgb(248, 250, 252);

            // 1. Sidebar Panel (Left)
            pnlSidebar = new Panel();
            pnlSidebar.Dock = DockStyle.Left;
            pnlSidebar.Width = 240;
            pnlSidebar.BackColor = Color.FromArgb(30, 41, 59); // Slate 800

            // Brand Header in Sidebar
            Panel pnlBrand = new Panel();
            pnlBrand.Dock = DockStyle.Top;
            pnlBrand.Height = 85;
            pnlBrand.BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            pnlBrand.Padding = new Padding(20, 16, 20, 12);

            Label lblBrand = new Label();
            lblBrand.Text = "SITETRACK";
            lblBrand.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            lblBrand.ForeColor = Color.White;
            lblBrand.AutoSize = true;
            lblBrand.Location = new Point(18, 16);

            Label lblBrandSub = new Label();
            lblBrandSub.Text = "Construction Manager";
            lblBrandSub.Font = new Font("Segoe UI", 8.5f);
            lblBrandSub.ForeColor = Color.FromArgb(148, 163, 184);
            lblBrandSub.AutoSize = true;
            lblBrandSub.Location = new Point(20, 46);

            pnlBrand.Controls.Add(lblBrand);
            pnlBrand.Controls.Add(lblBrandSub);
            pnlSidebar.Controls.Add(pnlBrand);

            // Navigation Container
            Panel pnlNavButtons = new Panel();
            pnlNavButtons.Dock = DockStyle.Fill;
            pnlNavButtons.Padding = new Padding(12, 16, 12, 16);

            btnNavDashboard = CreateNavButton("Dashboard", 16);
            btnNavProjects = CreateNavButton("Projects", 68);
            btnNavMaterials = CreateNavButton("Materials & Stock", 120);
            btnNavLabor = CreateNavButton("Labor & Attendance", 172);
            btnNavActivities = CreateNavButton("Site Activities", 224);
            btnNavUsers = CreateNavButton("User Management", 276); // Admin only
            btnNavReports = CreateNavButton("Reports & Export", 328);

            pnlNavButtons.Controls.Add(btnNavDashboard);
            pnlNavButtons.Controls.Add(btnNavProjects);
            pnlNavButtons.Controls.Add(btnNavMaterials);
            pnlNavButtons.Controls.Add(btnNavLabor);
            pnlNavButtons.Controls.Add(btnNavActivities);
            pnlNavButtons.Controls.Add(btnNavUsers);
            pnlNavButtons.Controls.Add(btnNavReports);

            // Sign Out at Bottom of Sidebar
            btnNavSignOut = new Button();
            btnNavSignOut.Text = "Sign Out";
            btnNavSignOut.Dock = DockStyle.Bottom;
            btnNavSignOut.Height = 44;
            btnNavSignOut.BackColor = Color.FromArgb(51, 65, 85);
            btnNavSignOut.ForeColor = Color.FromArgb(248, 113, 113); // Red 400
            btnNavSignOut.FlatStyle = FlatStyle.Flat;
            btnNavSignOut.FlatAppearance.BorderSize = 0;
            btnNavSignOut.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnNavSignOut.Cursor = Cursors.Hand;
            btnNavSignOut.Click += BtnNavSignOut_Click;
            pnlSidebar.Controls.Add(btnNavSignOut);

            pnlSidebar.Controls.Add(pnlNavButtons);
            this.Controls.Add(pnlSidebar);

            // 2. Top Header Bar
            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 70;
            pnlHeader.BackColor = Color.White;
            pnlHeader.Padding = new Padding(24, 12, 24, 12);
            pnlHeader.Paint += delegate(object s, PaintEventArgs e) {
                // Draw bottom border
                e.Graphics.DrawLine(new Pen(Color.FromArgb(226, 232, 240), 1), 0, pnlHeader.Height - 1, pnlHeader.Width, pnlHeader.Height - 1);
            };

            lblHeaderTitle = new Label();
            lblHeaderTitle.Text = "Dashboard";
            lblHeaderTitle.Font = new Font("Segoe UI", 14f, FontStyle.Bold);
            lblHeaderTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblHeaderTitle.AutoSize = true;
            lblHeaderTitle.Location = new Point(24, 12);

            lblHeaderSubtitle = new Label();
            lblHeaderSubtitle.Text = "System Overview & Site Metrics";
            lblHeaderSubtitle.Font = new Font("Segoe UI", 9f);
            lblHeaderSubtitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblHeaderSubtitle.AutoSize = true;
            lblHeaderSubtitle.Location = new Point(25, 40);

            // User Info on Right side of Header
            lblUserBadge = new Label();
            lblUserBadge.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblUserBadge.ForeColor = Color.White;
            lblUserBadge.TextAlign = ContentAlignment.MiddleCenter;
            lblUserBadge.Size = new Size(130, 26);
            lblUserBadge.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblUserBadge.Location = new Point(pnlHeader.Width - 160, 20);

            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Controls.Add(lblHeaderSubtitle);
            pnlHeader.Controls.Add(lblUserBadge);
            this.Controls.Add(pnlHeader);

            // 3. Status Bar at Bottom
            statusStrip = new StatusStrip();
            statusStrip.BackColor = Color.FromArgb(15, 23, 42);
            statusStrip.Font = new Font("Segoe UI", 9f);

            lblStatusUser = new ToolStripStatusLabel();
            lblStatusUser.ForeColor = Color.White;
            lblStatusUser.Text = "User: None";

            lblStatusRole = new ToolStripStatusLabel();
            lblStatusRole.ForeColor = Color.FromArgb(251, 191, 36); // Amber
            lblStatusRole.Text = "Role: None";

            lblStatusDb = new ToolStripStatusLabel();
            lblStatusDb.ForeColor = Color.FromArgb(74, 222, 128); // Green
            lblStatusDb.Text = "● Database: Connected (site_track_construction_manager_db)";

            lblStatusTime = new ToolStripStatusLabel();
            lblStatusTime.ForeColor = Color.FromArgb(148, 163, 184);
            lblStatusTime.Text = DateTime.Now.ToString("dd MMM yyyy HH:mm");
            lblStatusTime.Alignment = ToolStripItemAlignment.Right;

            statusStrip.Items.Add(lblStatusUser);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(lblStatusRole);
            statusStrip.Items.Add(new ToolStripSeparator());
            statusStrip.Items.Add(lblStatusDb);
            statusStrip.Items.Add(lblStatusTime);
            this.Controls.Add(statusStrip);

            // 4. Central Content Panel
            pnlContent = new Panel();
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.BackColor = Color.FromArgb(248, 250, 252);
            pnlContent.Padding = new Padding(24);
            this.Controls.Add(pnlContent);

            // Bring header and sidebar to front
            pnlHeader.BringToFront();
            pnlSidebar.BringToFront();
            statusStrip.SendToBack();
        }

        private Button CreateNavButton(string text, int top)
        {
            Button btn = new Button();
            btn.Text = "   " + text;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Location = new Point(12, top);
            btn.Size = new Size(216, 42);
            btn.BackColor = Color.Transparent;
            btn.ForeColor = Color.FromArgb(203, 213, 225);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
            btn.Cursor = Cursors.Hand;
            btn.Tag = text;
            btn.Click += NavButton_Click;
            return btn;
        }

        private void NavButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn == null) return;
            string module = btn.Tag as string;
            NavigateTo(module);
        }

        public void NavigateTo(string moduleName)
        {
            // Update Active button visuals
            Button[] buttons = new Button[] {
                btnNavDashboard, btnNavProjects, btnNavMaterials,
                btnNavLabor, btnNavActivities, btnNavUsers, btnNavReports
            };

            foreach (Button b in buttons)
            {
                if (string.Equals(b.Tag as string, moduleName, StringComparison.OrdinalIgnoreCase))
                {
                    b.BackColor = Color.FromArgb(37, 99, 235); // Blue 600
                    b.ForeColor = Color.White;
                    b.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
                    _activeNavButton = b;
                }
                else
                {
                    b.BackColor = Color.Transparent;
                    b.ForeColor = Color.FromArgb(203, 213, 225);
                    b.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
                }
            }

            lblHeaderTitle.Text = moduleName;
            lblHeaderSubtitle.Text = GetSubtitleForModule(moduleName);

            // Update Content Panel with Module View or Placeholder
            pnlContent.Controls.Clear();
            Control view = CreateViewForModule(moduleName);
            view.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(view);
        }

        private string GetSubtitleForModule(string module)
        {
            switch (module)
            {
                case "Dashboard": return "Real-time construction KPIs, stock health, and site status";
                case "Projects": return "Multi-site registry, timelines, and site manager assignments";
                case "Materials & Stock": return "Master material catalogs, purchase entries, and stock balances";
                case "Labor & Attendance": return "Daily worker registration, attendance logs, and gated wage payments";
                case "Site Activities": return "Daily operational progress logging constrained between 0% and 100%";
                case "User Management": return "Admin user provisioning, credentials, and role assignments";
                case "Reports & Export": return "Material consumption, labor cost summaries, and CSV/PDF export";
                default: return string.Empty;
            }
        }

        private Control CreateViewForModule(string module)
        {
            if (string.Equals(module, "User Management", StringComparison.OrdinalIgnoreCase))
            {
                if (BaseController.IsAdmin)
                {
                    return new Controls.UcUsers();
                }
            }

            if (string.Equals(module, "Projects", StringComparison.OrdinalIgnoreCase))
            {
                return new Controls.UcProjects();
            }

            if (string.Equals(module, "Materials & Stock", StringComparison.OrdinalIgnoreCase))
            {
                return new Controls.UcStock();
            }

            Panel card = new Panel();
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Padding = new Padding(32);

            Label lblTitle = new Label();
            lblTitle.Text = string.Format("SiteTrack — {0}", module);
            lblTitle.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(32, 28);
            card.Controls.Add(lblTitle);

            Label lblDesc = new Label();
            lblDesc.Text = string.Format("Module '{0}' is fully registered in the MVC Shell.\nActive Role: {1}\nActive User: {2}",
                module,
                BaseController.CurrentUser != null ? BaseController.CurrentUser.Role : "None",
                BaseController.CurrentUser != null ? BaseController.CurrentUser.FullName : "None");
            lblDesc.Font = new Font("Segoe UI", 10.5f);
            lblDesc.ForeColor = Color.FromArgb(71, 85, 105);
            lblDesc.AutoSize = true;
            lblDesc.Location = new Point(34, 75);
            card.Controls.Add(lblDesc);

            // Highlight role boundary
            if (string.Equals(module, "User Management") && !BaseController.IsAdmin)
            {
                Label lblRestricted = new Label();
                lblRestricted.Text = "Access Denied: This module is restricted to Administrators only.";
                lblRestricted.ForeColor = Color.FromArgb(220, 38, 38);
                lblRestricted.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
                lblRestricted.Location = new Point(34, 160);
                lblRestricted.AutoSize = true;
                card.Controls.Add(lblRestricted);
            }

            return card;
        }

        private void ApplyRoleRestrictions()
        {
            User user = BaseController.CurrentUser;
            if (user != null)
            {
                lblStatusUser.Text = string.Format("User: {0}", user.FullName);
                lblStatusRole.Text = string.Format("Role: {0}", user.Role);

                if (user.IsAdmin)
                {
                    lblUserBadge.Text = "ADMINISTRATOR";
                    lblUserBadge.BackColor = Color.FromArgb(217, 119, 6); // Amber 600
                    btnNavUsers.Visible = true;
                }
                else
                {
                    lblUserBadge.Text = "SITE MANAGER";
                    lblUserBadge.BackColor = Color.FromArgb(2, 132, 199); // Sky 600
                    btnNavUsers.Visible = false; // Hidden for site managers!
                }
            }
            else
            {
                lblUserBadge.Text = "GUEST";
                lblUserBadge.BackColor = Color.Gray;
                btnNavUsers.Visible = false;
            }
        }

        private void BtnNavSignOut_Click(object sender, EventArgs e)
        {
            DialogResult dr = MessageBox.Show(
                "Are you sure you want to sign out?",
                "SiteTrack — Confirm Sign Out",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (dr == DialogResult.Yes)
            {
                _authController.Logout();
                this.DialogResult = DialogResult.Retry; // Signals Program.cs to loop back to login
                this.Close();
            }
        }

        public void SetCurrentUser(User user)
        {
            BaseController.SetSession(user);
            ApplyRoleRestrictions();
        }
    }
}

