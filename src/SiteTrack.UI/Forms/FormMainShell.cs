using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core.Models;
using SiteTrack.Data.Repositories;
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
            this.Size = new Size(1280, 800);
            this.MinimumSize = new Size(1024, 650);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.WindowState = FormWindowState.Maximized;
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
            lblBrand.ForeColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
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

            pnlNavButtons.AutoScroll = true;
            pnlSidebar.Controls.Add(pnlNavButtons);

            // Establish deterministic WinForms docking order inside sidebar (reverse z-order):
            // 1. pnlBrand docks Top 1st (claims Y: 0..85 for the logo)
            // 2. btnNavSignOut docks Bottom 2nd (claims Y: Bottom-44..Bottom)
            // 3. pnlNavButtons docks Fill 3rd (fills Y: 85..Bottom-44 strictly below the brand logo)
            pnlSidebar.Controls.SetChildIndex(pnlBrand, 2);
            pnlSidebar.Controls.SetChildIndex(btnNavSignOut, 1);
            pnlSidebar.Controls.SetChildIndex(pnlNavButtons, 0);

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
            pnlHeader.Resize += delegate {
                lblUserBadge.Location = new Point(pnlHeader.ClientSize.Width - lblUserBadge.Width - 24, 20);
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
            pnlContent.Padding = new Padding(0); // View controls manage their internal padding
            pnlContent.AutoScroll = true;
            this.Controls.Add(pnlContent);

            // Establish deterministic WinForms docking hierarchy (reverse z-order):
            // 1. pnlSidebar docks Left (claims full left edge from top to bottom)
            // 2. statusStrip docks Bottom (claims bottom from sidebar to right edge)
            // 3. pnlHeader docks Top (claims top from sidebar to right edge)
            // 4. pnlContent docks Fill (occupies remaining rectangle between sidebar, header, and statusStrip)
            this.Controls.SetChildIndex(pnlSidebar, 3);
            this.Controls.SetChildIndex(statusStrip, 2);
            this.Controls.SetChildIndex(pnlHeader, 1);
            this.Controls.SetChildIndex(pnlContent, 0);
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
                    b.BackColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
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

            if (string.Equals(module, "Labor & Attendance", StringComparison.OrdinalIgnoreCase))
            {
                return new Controls.UcLabor();
            }

            if (string.Equals(module, "Site Activities", StringComparison.OrdinalIgnoreCase))
            {
                return new Controls.UcActivities();
            }

            if (string.Equals(module, "Reports & Export", StringComparison.OrdinalIgnoreCase))
            {
                return new Controls.UcReports();
            }

            if (string.Equals(module, "Dashboard", StringComparison.OrdinalIgnoreCase))
            {
                return CreateDashboardView();
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

        private Control CreateDashboardView()
        {
            Panel root = new Panel();
            root.Dock = DockStyle.Fill;
            root.BackColor = Color.FromArgb(248, 250, 252);
            root.Padding = new Padding(20);
            root.AutoScroll = true;

            // Repositories for live metrics
            ProjectRepository projRepo = new ProjectRepository();
            MaterialRepository matRepo = new MaterialRepository();
            StockRepository stockRepo = new StockRepository();
            LaborRepository labRepo = new LaborRepository();

            List<Project> projects = BaseController.IsAdmin
                ? projRepo.GetAll()
                : (BaseController.CurrentUserId.HasValue ? projRepo.GetByManager(BaseController.CurrentUserId.Value) : new List<Project>());

            int materialCount = matRepo.GetAllMaterials(null).Count;
            int workerCount = labRepo.GetAllWorkers(null, null).Count;
            int lowStockCount = 0;
            foreach (Project p in projects)
            {
                List<ProjectMaterialStock> stocks = stockRepo.GetStockByProject(p.Id);
                lowStockCount += stocks.FindAll(delegate(ProjectMaterialStock s) { return s.IsLowStock; }).Count;
            }

            // 1. Welcome Header Banner
            Panel pnlWelcome = new Panel();
            pnlWelcome.Dock = DockStyle.Top;
            pnlWelcome.Height = 75;
            pnlWelcome.BackColor = Color.White;
            pnlWelcome.BorderStyle = BorderStyle.FixedSingle;
            pnlWelcome.Padding = new Padding(20, 12, 20, 12);

            Label lblWelcome = new Label();
            lblWelcome.Text = string.Format("Welcome back, {0}",
                BaseController.CurrentUser != null ? BaseController.CurrentUser.FullName : "User");
            lblWelcome.Font = new Font("Segoe UI", 12.5f, FontStyle.Bold);
            lblWelcome.ForeColor = Color.FromArgb(15, 23, 42);
            lblWelcome.AutoSize = true;
            lblWelcome.Location = new Point(18, 12);

            Label lblWelcomeSub = new Label();
            lblWelcomeSub.Text = string.Format("Role: {0} • {1:dddd, dd MMMM yyyy} • {2} Active Sites Under Management",
                BaseController.CurrentUser != null ? BaseController.CurrentUser.Role : "Guest",
                DateTime.Now,
                projects.Count);
            lblWelcomeSub.Font = new Font("Segoe UI", 8.5f);
            lblWelcomeSub.ForeColor = Color.FromArgb(100, 116, 139);
            lblWelcomeSub.AutoSize = true;
            lblWelcomeSub.Location = new Point(20, 38);

            pnlWelcome.Controls.Add(lblWelcome);
            pnlWelcome.Controls.Add(lblWelcomeSub);

            // 2. Metrics Bar (4 Responsive Cards)
            TableLayoutPanel tlpMetrics = new TableLayoutPanel();
            tlpMetrics.Dock = DockStyle.Top;
            tlpMetrics.Height = 110;
            tlpMetrics.ColumnCount = 4;
            tlpMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpMetrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tlpMetrics.Padding = new Padding(0, 12, 0, 12);

            tlpMetrics.Controls.Add(CreateKpiCard("Active Sites", projects.Count.ToString(), "Managed Construction Sites", Color.FromArgb(235, 86, 18)), 0, 0);
            tlpMetrics.Controls.Add(CreateKpiCard("Master Materials", materialCount.ToString(), "Catalog Items Registered", Color.FromArgb(13, 148, 136)), 1, 0);
            tlpMetrics.Controls.Add(CreateKpiCard("Low Stock Alerts", lowStockCount.ToString(), lowStockCount > 0 ? "Requires Reorder Action" : "Stock Levels Healthy", lowStockCount > 0 ? Color.FromArgb(220, 38, 38) : Color.FromArgb(22, 163, 74)), 2, 0);
            tlpMetrics.Controls.Add(CreateKpiCard("Site Craftsmen", workerCount.ToString(), "Registered Site Workers", Color.FromArgb(124, 58, 237)), 3, 0);

            // 3. Quick Action Shortcut Bar
            Panel pnlActions = new Panel();
            pnlActions.Dock = DockStyle.Top;
            pnlActions.Height = 50;
            pnlActions.Padding = new Padding(0, 4, 0, 8);

            Button btnActProjects = CreateQuickActionButton("📁 Multi-Site Registry", delegate { NavigateTo("Projects"); });
            Button btnActStock = CreateQuickActionButton("📦 Materials & Stock", delegate { NavigateTo("Materials & Stock"); });
            Button btnActLabor = CreateQuickActionButton("👷 Attendance & Wages", delegate { NavigateTo("Labor & Attendance"); });
            Button btnActProgress = CreateQuickActionButton("📈 Site Activities", delegate { NavigateTo("Site Activities"); });
            Button btnActReports = CreateQuickActionButton("📊 Reports & Export", delegate { NavigateTo("Reports & Export"); });

            btnActProjects.Location = new Point(0, 6);
            btnActStock.Location = new Point(175, 6);
            btnActLabor.Location = new Point(350, 6);
            btnActProgress.Location = new Point(530, 6);
            btnActReports.Location = new Point(700, 6);

            pnlActions.Controls.Add(btnActProjects);
            pnlActions.Controls.Add(btnActStock);
            pnlActions.Controls.Add(btnActLabor);
            pnlActions.Controls.Add(btnActProgress);
            pnlActions.Controls.Add(btnActReports);

            // 4. Projects Table Panel
            Panel pnlGridSection = new Panel();
            pnlGridSection.Dock = DockStyle.Fill;
            pnlGridSection.BackColor = Color.White;
            pnlGridSection.BorderStyle = BorderStyle.FixedSingle;
            pnlGridSection.Padding = new Padding(16);

            Label lblGridTitle = new Label();
            lblGridTitle.Text = "Active Construction Sites (Operational Overview)";
            lblGridTitle.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            lblGridTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblGridTitle.Dock = DockStyle.Top;
            lblGridTitle.Height = 28;
            pnlGridSection.Controls.Add(lblGridTitle);

            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowTemplate.Height = 36;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            grid.ColumnHeadersHeight = 36;
            grid.EnableHeadersVisualStyles = false;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 237, 213);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

            grid.Columns.Add("Id", "ID");
            grid.Columns["Id"].Width = 45;
            grid.Columns.Add("ProjectName", "Project Name");
            grid.Columns.Add("Location", "Site Location");
            grid.Columns.Add("Status", "Status");
            grid.Columns.Add("StartDate", "Start Date");
            grid.Columns.Add("EndDate", "Expected Completion");

            foreach (Project p in projects)
            {
                grid.Rows.Add(
                    p.Id,
                    p.ProjectName,
                    p.Location,
                    p.Status,
                    p.StartDate.ToString("dd MMM yyyy"),
                    p.ExpectedEndDate.HasValue ? p.ExpectedEndDate.Value.ToString("dd MMM yyyy") : "-"
                );
            }

            pnlGridSection.Controls.Add(grid);
            grid.BringToFront();

            // Assemble into root container with correct docking hierarchy
            root.Controls.Add(pnlGridSection);
            root.Controls.Add(pnlActions);
            root.Controls.Add(tlpMetrics);
            root.Controls.Add(pnlWelcome);

            pnlWelcome.SendToBack();
            tlpMetrics.SendToBack();
            pnlActions.SendToBack();
            pnlGridSection.BringToFront();

            return root;
        }

        private Panel CreateKpiCard(string title, string value, string subtitle, Color accent)
        {
            Panel card = new Panel();
            card.Dock = DockStyle.Fill;
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;
            card.Padding = new Padding(14, 10, 14, 10);
            card.Margin = new Padding(4);

            Panel accentBar = new Panel();
            accentBar.Dock = DockStyle.Left;
            accentBar.Width = 4;
            accentBar.BackColor = accent;
            card.Controls.Add(accentBar);

            Label lblVal = new Label();
            lblVal.Text = value;
            lblVal.Font = new Font("Segoe UI", 15f, FontStyle.Bold);
            lblVal.ForeColor = Color.FromArgb(15, 23, 42);
            lblVal.Location = new Point(14, 6);
            lblVal.AutoSize = true;
            card.Controls.Add(lblVal);

            Label lblTit = new Label();
            lblTit.Text = title;
            lblTit.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblTit.ForeColor = Color.FromArgb(71, 85, 105);
            lblTit.Location = new Point(16, 36);
            lblTit.AutoSize = true;
            card.Controls.Add(lblTit);

            Label lblSub = new Label();
            lblSub.Text = subtitle;
            lblSub.Font = new Font("Segoe UI", 8f);
            lblSub.ForeColor = Color.FromArgb(148, 163, 184);
            lblSub.Location = new Point(16, 54);
            lblSub.AutoSize = true;
            card.Controls.Add(lblSub);

            return card;
        }

        private Button CreateQuickActionButton(string text, EventHandler onClick)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Size = new Size(165, 34);
            btn.BackColor = Color.White;
            btn.ForeColor = Color.FromArgb(30, 41, 59);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btn.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;
            btn.Click += onClick;
            return btn;
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
                    lblUserBadge.BackColor = Color.FromArgb(194, 65, 12); // Deep Amber/Orange 700
                    btnNavUsers.Visible = true;
                }
                else
                {
                    lblUserBadge.Text = "SITE MANAGER";
                    lblUserBadge.BackColor = Color.FromArgb(235, 86, 18); // Main Brand Orange #eb5612
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

