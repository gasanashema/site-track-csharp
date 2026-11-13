using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;
using SiteTrack.UI.Forms.Dialogs;

namespace SiteTrack.UI.Controls
{
    public class UcActivities : UserControl
    {
        private readonly ActivityController _activityController;
        private readonly ProjectController _projectController;

        private ComboBox cmbProjects;
        private TextBox txtSearch;
        private Button btnLogActivity;
        private Button btnRefresh;

        private ProgressBar prgOverall;
        private Label lblOverallPercent;
        private Label lblMetricActivitiesCount;
        private Label lblMetricLatestDate;

        private DataGridView gridActivities;
        private List<ProjectActivity> _loadedActivities = new List<ProjectActivity>();

        public UcActivities() : this(new ActivityController(), new ProjectController())
        {
        }

        public UcActivities(ActivityController activityController, ProjectController projectController)
        {
            this._activityController = activityController ?? new ActivityController();
            this._projectController = projectController ?? new ProjectController();

            InitializeComponent();
            LoadProjects();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Segoe UI", 9.5f);
            this.Padding = new Padding(16);

            // 1. Top Control Bar
            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 55;
            pnlTop.BackColor = Color.FromArgb(241, 245, 249);
            pnlTop.Padding = new Padding(12);

            Label lblProj = new Label();
            lblProj.Text = "Project:";
            lblProj.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblProj.ForeColor = Color.FromArgb(30, 41, 59);
            lblProj.Location = new Point(12, 16);
            lblProj.AutoSize = true;
            pnlTop.Controls.Add(lblProj);

            cmbProjects = new ComboBox();
            cmbProjects.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProjects.Location = new Point(70, 12);
            cmbProjects.Size = new Size(320, 26);
            cmbProjects.SelectedIndexChanged += delegate { LoadActivities(); };
            pnlTop.Controls.Add(cmbProjects);

            Label lblSearch = new Label();
            lblSearch.Text = "Search:";
            lblSearch.Location = new Point(405, 16);
            lblSearch.AutoSize = true;
            pnlTop.Controls.Add(lblSearch);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(460, 12);
            txtSearch.Size = new Size(180, 26);
            txtSearch.TextChanged += delegate { FilterActivities(); };
            pnlTop.Controls.Add(txtSearch);

            btnLogActivity = new Button();
            btnLogActivity.Text = "+ Log Site Activity";
            btnLogActivity.Location = new Point(655, 8);
            btnLogActivity.Size = new Size(160, 36);
            btnLogActivity.BackColor = Color.FromArgb(37, 99, 235); // Blue 600
            btnLogActivity.ForeColor = Color.White;
            btnLogActivity.FlatStyle = FlatStyle.Flat;
            btnLogActivity.FlatAppearance.BorderSize = 0;
            btnLogActivity.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnLogActivity.Cursor = Cursors.Hand;
            btnLogActivity.Click += BtnLogActivity_Click;
            pnlTop.Controls.Add(btnLogActivity);

            btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Location = new Point(825, 8);
            btnRefresh.Size = new Size(80, 36);
            btnRefresh.BackColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 1;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.Click += delegate { LoadActivities(); };
            pnlTop.Controls.Add(btnRefresh);

            this.Controls.Add(pnlTop);

            // 2. Overall Progress & Metrics Bar
            Panel pnlProgressBanner = new Panel();
            pnlProgressBanner.Dock = DockStyle.Top;
            pnlProgressBanner.Height = 85;
            pnlProgressBanner.BackColor = Color.FromArgb(248, 250, 252);
            pnlProgressBanner.BorderStyle = BorderStyle.FixedSingle;
            pnlProgressBanner.Padding = new Padding(16, 12, 16, 12);

            Label lblProgHeader = new Label();
            lblProgHeader.Text = "OVERALL SITE PROGRESS MILESTONE";
            lblProgHeader.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblProgHeader.ForeColor = Color.FromArgb(100, 116, 139);
            lblProgHeader.Location = new Point(16, 12);
            lblProgHeader.AutoSize = true;
            pnlProgressBanner.Controls.Add(lblProgHeader);

            prgOverall = new ProgressBar();
            prgOverall.Minimum = 0;
            prgOverall.Maximum = 100;
            prgOverall.Value = 0;
            prgOverall.Location = new Point(16, 36);
            prgOverall.Size = new Size(380, 26);
            pnlProgressBanner.Controls.Add(prgOverall);

            lblOverallPercent = new Label();
            lblOverallPercent.Text = "0%";
            lblOverallPercent.Font = new Font("Segoe UI", 16f, FontStyle.Bold);
            lblOverallPercent.ForeColor = Color.FromArgb(37, 99, 235);
            lblOverallPercent.Location = new Point(405, 30);
            lblOverallPercent.AutoSize = true;
            pnlProgressBanner.Controls.Add(lblOverallPercent);

            // Metric Cards inside Banner
            Panel cardCount = CreateSmallMetricCard("Logged Milestones", out lblMetricActivitiesCount, 520);
            Panel cardDate = CreateSmallMetricCard("Last Activity Date", out lblMetricLatestDate, 720);
            pnlProgressBanner.Controls.Add(cardCount);
            pnlProgressBanner.Controls.Add(cardDate);

            this.Controls.Add(pnlProgressBanner);

            // 3. Grid
            gridActivities = new DataGridView();
            gridActivities.Dock = DockStyle.Fill;
            gridActivities.BackgroundColor = Color.White;
            gridActivities.BorderStyle = BorderStyle.None;
            gridActivities.RowHeadersVisible = false;
            gridActivities.AllowUserToAddRows = false;
            gridActivities.AllowUserToDeleteRows = false;
            gridActivities.ReadOnly = true;
            gridActivities.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridActivities.MultiSelect = false;
            gridActivities.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridActivities.RowTemplate.Height = 38;
            gridActivities.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            gridActivities.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            gridActivities.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            gridActivities.ColumnHeadersHeight = 40;
            gridActivities.EnableHeadersVisualStyles = false;

            gridActivities.Columns.Add("Date", "Activity Date");
            gridActivities.Columns["Date"].Width = 110;
            gridActivities.Columns.Add("Title", "Milestone / Task Title");
            gridActivities.Columns["Title"].Width = 220;
            gridActivities.Columns.Add("Progress", "Reported Progress");
            gridActivities.Columns["Progress"].Width = 140;
            gridActivities.Columns.Add("Description", "Scope / Details");
            gridActivities.Columns.Add("RecordedBy", "Recorded By");
            gridActivities.Columns["RecordedBy"].Width = 130;

            gridActivities.CellFormatting += GridActivities_CellFormatting;
            this.Controls.Add(gridActivities);

            pnlProgressBanner.BringToFront();
            pnlTop.BringToFront();
        }

        private Panel CreateSmallMetricCard(string label, out Label lblValue, int left)
        {
            Panel card = new Panel();
            card.Location = new Point(left, 15);
            card.Size = new Size(180, 52);
            card.BackColor = Color.White;
            card.BorderStyle = BorderStyle.FixedSingle;

            Label lblTitle = new Label();
            lblTitle.Text = label.ToUpperInvariant();
            lblTitle.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblTitle.Location = new Point(10, 6);
            lblTitle.AutoSize = true;
            card.Controls.Add(lblTitle);

            lblValue = new Label();
            lblValue.Text = "-";
            lblValue.Font = new Font("Segoe UI", 11.5f, FontStyle.Bold);
            lblValue.ForeColor = Color.FromArgb(15, 23, 42);
            lblValue.Location = new Point(10, 24);
            lblValue.AutoSize = true;
            card.Controls.Add(lblValue);

            return card;
        }

        private void LoadProjects()
        {
            cmbProjects.Items.Clear();
            List<Project> projs = _projectController.GetProjectsForCurrentUser();
            foreach (Project p in projs)
            {
                cmbProjects.Items.Add(new ProjectItem(p.Id, p.ProjectName));
            }

            if (cmbProjects.Items.Count > 0)
            {
                cmbProjects.SelectedIndex = 0;
            }
            else
            {
                btnLogActivity.Enabled = false;
            }
        }

        private int? GetSelectedProjectId()
        {
            ProjectItem item = cmbProjects.SelectedItem as ProjectItem;
            return item != null ? (int?)item.Id : null;
        }

        public void LoadActivities()
        {
            int? projId = GetSelectedProjectId();
            if (!projId.HasValue) return;

            _loadedActivities = _activityController.GetActivities(projId.Value);

            int latestProgress = _activityController.GetLatestProgress(projId.Value);
            prgOverall.Value = Math.Min(100, Math.Max(0, latestProgress));
            lblOverallPercent.Text = string.Format("{0}%", latestProgress);

            lblMetricActivitiesCount.Text = _loadedActivities.Count.ToString();
            lblMetricLatestDate.Text = _loadedActivities.Count > 0
                ? _loadedActivities[0].ActivityDate.ToString("yyyy-MM-dd")
                : "None";

            FilterActivities();
        }

        private void FilterActivities()
        {
            gridActivities.Rows.Clear();
            string search = txtSearch != null ? txtSearch.Text.Trim().ToLowerInvariant() : "";

            foreach (ProjectActivity a in _loadedActivities)
            {
                if (!string.IsNullOrEmpty(search))
                {
                    bool match = (a.ActivityTitle != null && a.ActivityTitle.ToLowerInvariant().Contains(search)) ||
                                 (a.ActivityDescription != null && a.ActivityDescription.ToLowerInvariant().Contains(search));
                    if (!match) continue;
                }

                gridActivities.Rows.Add(
                    a.ActivityDate.ToString("yyyy-MM-dd"),
                    a.ActivityTitle,
                    string.Format("{0}%", a.ProgressPercentage),
                    a.ActivityDescription ?? "-",
                    a.RecordedByName ?? "Site Mgr"
                );
            }
        }

        private void GridActivities_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && gridActivities.Columns[e.ColumnIndex].Name == "Progress")
            {
                object val = e.Value;
                if (val != null)
                {
                    e.CellStyle.ForeColor = Color.FromArgb(37, 99, 235); // Blue
                    e.CellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                }
            }
        }

        private void BtnLogActivity_Click(object sender, EventArgs e)
        {
            int? projId = GetSelectedProjectId();
            using (FormActivityDialog dlg = new FormActivityDialog(projId, _activityController, _projectController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadActivities();
                }
            }
        }

        private class ProjectItem
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public ProjectItem(int id, string name) { this.Id = id; this.Name = name; }
            public override string ToString() { return Name; }
        }
    }
}
