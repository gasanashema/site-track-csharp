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
    public class UcProjects : UserControl
    {
        private readonly ProjectController _projectController;

        private DataGridView gridProjects;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnManageManagers;
        private Button btnRefresh;
        private Label lblTotal;

        public UcProjects() : this(new ProjectController())
        {
        }

        public UcProjects(ProjectController projectController)
        {
            this._projectController = projectController ?? new ProjectController();
            InitializeComponent();
            ApplyRoleRestrictions();
            LoadProjects();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Segoe UI", 9.5f);
            this.Padding = new Padding(20);

            // 1. Toolbar Panel
            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 55;
            pnlTop.BackColor = Color.Transparent;

            btnAdd = new Button();
            btnAdd.Text = "+ New Project";
            btnAdd.Location = new Point(0, 8);
            btnAdd.Size = new Size(115, 36);
            btnAdd.BackColor = Color.FromArgb(37, 99, 235);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += BtnAdd_Click;
            pnlTop.Controls.Add(btnAdd);

            btnEdit = new Button();
            btnEdit.Text = "Edit Project";
            btnEdit.Location = new Point(125, 8);
            btnEdit.Size = new Size(95, 36);
            btnEdit.BackColor = Color.FromArgb(241, 245, 249);
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 1;
            btnEdit.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnEdit.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Click += BtnEdit_Click;
            pnlTop.Controls.Add(btnEdit);

            btnDelete = new Button();
            btnDelete.Text = "Delete";
            btnDelete.Location = new Point(230, 8);
            btnDelete.Size = new Size(80, 36);
            btnDelete.BackColor = Color.FromArgb(241, 245, 249);
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.FlatAppearance.BorderSize = 1;
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnDelete.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnDelete.Cursor = Cursors.Hand;
            btnDelete.Click += BtnDelete_Click;
            pnlTop.Controls.Add(btnDelete);

            btnManageManagers = new Button();
            btnManageManagers.Text = "Site Managers";
            btnManageManagers.Location = new Point(320, 8);
            btnManageManagers.Size = new Size(125, 36);
            btnManageManagers.BackColor = Color.FromArgb(241, 245, 249);
            btnManageManagers.FlatStyle = FlatStyle.Flat;
            btnManageManagers.FlatAppearance.BorderSize = 1;
            btnManageManagers.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnManageManagers.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnManageManagers.Cursor = Cursors.Hand;
            btnManageManagers.Click += BtnManageManagers_Click;
            pnlTop.Controls.Add(btnManageManagers);

            btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Location = new Point(455, 8);
            btnRefresh.Size = new Size(80, 36);
            btnRefresh.BackColor = Color.FromArgb(241, 245, 249);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 1;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.Click += delegate { LoadProjects(); };
            pnlTop.Controls.Add(btnRefresh);

            // Search
            Label lblSearch = new Label();
            lblSearch.Text = "Search:";
            lblSearch.Location = new Point(560, 16);
            lblSearch.AutoSize = true;
            lblSearch.ForeColor = Color.FromArgb(100, 116, 139);
            pnlTop.Controls.Add(lblSearch);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(615, 12);
            txtSearch.Size = new Size(200, 26);
            txtSearch.TextChanged += delegate { FilterProjects(); };
            pnlTop.Controls.Add(txtSearch);

            lblTotal = new Label();
            lblTotal.Text = "Total: 0 projects";
            lblTotal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotal.Location = new Point(pnlTop.Width - 160, 16);
            lblTotal.AutoSize = true;
            lblTotal.ForeColor = Color.FromArgb(100, 116, 139);
            pnlTop.Controls.Add(lblTotal);

            this.Controls.Add(pnlTop);

            // 2. Data Grid
            gridProjects = new DataGridView();
            gridProjects.Dock = DockStyle.Fill;
            gridProjects.BackgroundColor = Color.White;
            gridProjects.BorderStyle = BorderStyle.None;
            gridProjects.RowHeadersVisible = false;
            gridProjects.AllowUserToAddRows = false;
            gridProjects.AllowUserToDeleteRows = false;
            gridProjects.ReadOnly = true;
            gridProjects.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridProjects.MultiSelect = false;
            gridProjects.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridProjects.RowTemplate.Height = 36;
            gridProjects.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            gridProjects.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            gridProjects.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            gridProjects.ColumnHeadersHeight = 40;
            gridProjects.EnableHeadersVisualStyles = false;

            gridProjects.Columns.Add("Id", "ID");
            gridProjects.Columns["Id"].Width = 45;
            gridProjects.Columns.Add("ProjectName", "Project Name");
            gridProjects.Columns.Add("Location", "Site Location");
            gridProjects.Columns.Add("Status", "Status");
            gridProjects.Columns["Status"].Width = 110;
            gridProjects.Columns.Add("StartDate", "Start Date");
            gridProjects.Columns["StartDate"].Width = 100;
            gridProjects.Columns.Add("EndDate", "Expected End");
            gridProjects.Columns["EndDate"].Width = 100;
            gridProjects.Columns.Add("Managers", "Assigned Site Managers");

            gridProjects.CellFormatting += GridProjects_CellFormatting;
            this.Controls.Add(gridProjects);

            gridProjects.BringToFront();
        }

        private void ApplyRoleRestrictions()
        {
            if (!BaseController.IsAdmin)
            {
                // Site managers can view their scoped projects, but cannot create or delete projects or modify manager assignments
                btnAdd.Visible = false;
                btnDelete.Visible = false;
                btnManageManagers.Visible = false;
                btnEdit.Text = "View Details";
            }
        }

        private void GridProjects_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridProjects.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                switch (status)
                {
                    case "Ongoing":
                        e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52); // Green 800
                        e.CellStyle.BackColor = Color.FromArgb(220, 252, 231); // Green 100
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        break;
                    case "Planning":
                        e.CellStyle.ForeColor = Color.FromArgb(30, 64, 175); // Blue 800
                        e.CellStyle.BackColor = Color.FromArgb(219, 234, 254); // Blue 100
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        break;
                    case "Completed":
                        e.CellStyle.ForeColor = Color.FromArgb(71, 85, 105);
                        e.CellStyle.BackColor = Color.FromArgb(241, 245, 249);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        break;
                    case "Cancelled":
                        e.CellStyle.ForeColor = Color.FromArgb(153, 27, 27); // Red 800
                        e.CellStyle.BackColor = Color.FromArgb(254, 226, 226); // Red 100
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                        break;
                }
            }
        }

        private List<Project> _cachedProjects = new List<Project>();

        private void LoadProjects()
        {
            _cachedProjects = _projectController.GetProjectsForCurrentUser();
            FilterProjects();
        }

        private void FilterProjects()
        {
            gridProjects.Rows.Clear();
            string query = txtSearch.Text.Trim().ToLowerInvariant();

            foreach (Project p in _cachedProjects)
            {
                if (string.IsNullOrEmpty(query) ||
                    p.ProjectName.ToLowerInvariant().Contains(query) ||
                    p.Location.ToLowerInvariant().Contains(query) ||
                    p.Status.ToLowerInvariant().Contains(query))
                {
                    List<ProjectManager> pms = _projectController.GetProjectManagers(p.Id);
                    List<string> managerNames = new List<string>();
                    foreach (ProjectManager pm in pms)
                    {
                        managerNames.Add(pm.ManagerName);
                    }
                    string managersDisplay = managerNames.Count > 0 ? string.Join(", ", managerNames.ToArray()) : "None assigned";

                    gridProjects.Rows.Add(
                        p.Id,
                        p.ProjectName,
                        p.Location,
                        p.Status,
                        p.StartDate.ToString("yyyy-MM-dd"),
                        p.ExpectedEndDate.HasValue ? p.ExpectedEndDate.Value.ToString("yyyy-MM-dd") : "Open-ended",
                        managersDisplay
                    );
                }
            }

            lblTotal.Text = string.Format("Total: {0} projects", gridProjects.Rows.Count);
        }

        private Project GetSelectedProject()
        {
            if (gridProjects.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(gridProjects.SelectedRows[0].Cells["Id"].Value);
            return _projectController.GetProjectById(id);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (FormProjectDialog dlg = new FormProjectDialog(_projectController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadProjects();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            Project selected = GetSelectedProject();
            if (selected == null)
            {
                MessageBox.Show("Please select a project to view/edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FormProjectDialog dlg = new FormProjectDialog(_projectController, selected))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadProjects();
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            Project selected = GetSelectedProject();
            if (selected == null)
            {
                MessageBox.Show("Please select a project to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult dr = MessageBox.Show(
                string.Format("Are you sure you want to delete project '{0}'?", selected.ProjectName),
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dr == DialogResult.Yes)
            {
                Result res = _projectController.DeleteProject(selected.Id);
                if (!res.IsSuccess)
                {
                    MessageBox.Show(res.ErrorMessage, "Deletion Blocked (Business Rule 5)", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    LoadProjects();
                }
            }
        }

        private void BtnManageManagers_Click(object sender, EventArgs e)
        {
            Project selected = GetSelectedProject();
            if (selected == null)
            {
                MessageBox.Show("Please select a project first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FormAssignManagerDialog dlg = new FormAssignManagerDialog(selected, _projectController))
            {
                dlg.ShowDialog();
                LoadProjects();
            }
        }
    }
}
