using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormAssignManagerDialog : Form
    {
        private readonly ProjectController _projectController;
        private readonly UserController _userController;
        private readonly Project _project;

        private ListBox lstManagers;
        private ComboBox cmbAvailableUsers;
        private Button btnAssign;
        private Button btnRemove;
        private Button btnClose;
        private Label lblError;

        public FormAssignManagerDialog(Project project, ProjectController projectController = null, UserController userController = null)
        {
            this._project = project;
            this._projectController = projectController ?? new ProjectController();
            this._userController = userController ?? new UserController();
            InitializeComponent();
            LoadData();
        }

        private void InitializeComponent()
        {
            this.Text = "SiteTrack — Manage Site Managers";
            this.Size = new Size(560, 620);
            this.MinimumSize = new Size(560, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            Label lblTitle = new Label();
            lblTitle.Text = string.Format("Managers for: {0}", _project != null ? _project.ProjectName : "");
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, 20);
            lblTitle.Size = new Size(500, 28);
            this.Controls.Add(lblTitle);

            Label lblList = new Label();
            lblList.Text = "Assigned Site Managers:";
            lblList.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblList.ForeColor = Color.FromArgb(71, 85, 105);
            lblList.Location = new Point(24, 55);
            lblList.AutoSize = true;
            this.Controls.Add(lblList);

            lstManagers = new ListBox();
            lstManagers.Location = new Point(24, 78);
            lstManagers.Size = new Size(500, 210);
            lstManagers.Font = new Font("Segoe UI", 10f);
            this.Controls.Add(lstManagers);

            btnRemove = new Button();
            btnRemove.Text = "Remove Selected Manager";
            btnRemove.Location = new Point(24, 298);
            btnRemove.Size = new Size(200, 36);
            btnRemove.BackColor = Color.FromArgb(254, 226, 226); // Red 100
            btnRemove.ForeColor = Color.FromArgb(185, 28, 28);   // Red 700
            btnRemove.FlatStyle = FlatStyle.Flat;
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnRemove.Cursor = Cursors.Hand;
            btnRemove.Click += BtnRemove_Click;
            this.Controls.Add(btnRemove);

            // Add New Manager Section
            GroupBox grpAdd = new GroupBox();
            grpAdd.Text = "Assign New Site Manager";
            grpAdd.Location = new Point(24, 350);
            grpAdd.Size = new Size(500, 95);
            grpAdd.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            grpAdd.ForeColor = Color.FromArgb(71, 85, 105);

            cmbAvailableUsers = new ComboBox();
            cmbAvailableUsers.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAvailableUsers.Location = new Point(16, 36);
            cmbAvailableUsers.Size = new Size(310, 26);
            cmbAvailableUsers.Font = new Font("Segoe UI", 9.5f);
            grpAdd.Controls.Add(cmbAvailableUsers);

            btnAssign = new Button();
            btnAssign.Text = "Assign Manager";
            btnAssign.Location = new Point(340, 32);
            btnAssign.Size = new Size(140, 34);
            btnAssign.BackColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
            btnAssign.ForeColor = Color.White;
            btnAssign.FlatStyle = FlatStyle.Flat;
            btnAssign.FlatAppearance.BorderSize = 0;
            btnAssign.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnAssign.Cursor = Cursors.Hand;
            btnAssign.Click += BtnAssign_Click;
            grpAdd.Controls.Add(btnAssign);

            this.Controls.Add(grpAdd);

            lblError = new Label();
            lblError.Location = new Point(24, 465);
            lblError.Size = new Size(380, 30);
            lblError.ForeColor = Color.FromArgb(220, 38, 38);
            lblError.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblError.Visible = false;
            this.Controls.Add(lblError);

            btnClose = new Button();
            btnClose.Text = "Close";
            btnClose.Location = new Point(420, 465);
            btnClose.Size = new Size(104, 38);
            btnClose.BackColor = Color.FromArgb(226, 232, 240);
            btnClose.FlatStyle = FlatStyle.Flat;
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnClose.Cursor = Cursors.Hand;
            btnClose.Click += delegate { this.Close(); };
            this.Controls.Add(btnClose);
        }

        private void LoadData()
        {
            if (_project == null) return;

            lstManagers.Items.Clear();
            List<ProjectManager> managers = _projectController.GetProjectManagers(_project.Id);
            List<int> assignedUserIds = new List<int>();

            foreach (ProjectManager pm in managers)
            {
                assignedUserIds.Add(pm.UserId);
                lstManagers.Items.Add(new ManagerItem(pm.UserId, string.Format("{0} ({1})", pm.ManagerName, pm.ManagerEmail)));
            }

            cmbAvailableUsers.Items.Clear();
            List<User> allUsers = _userController.GetAllUsers();
            foreach (User u in allUsers)
            {
                if (u.IsActive && !assignedUserIds.Contains(u.Id))
                {
                    cmbAvailableUsers.Items.Add(new ManagerItem(u.Id, string.Format("{0} [{1}]", u.FullName, u.Role)));
                }
            }

            if (cmbAvailableUsers.Items.Count > 0)
            {
                cmbAvailableUsers.SelectedIndex = 0;
                btnAssign.Enabled = true;
            }
            else
            {
                btnAssign.Enabled = false;
            }
        }

        private void BtnAssign_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            ManagerItem selected = cmbAvailableUsers.SelectedItem as ManagerItem;
            if (selected == null) return;

            Result res = _projectController.AssignManager(_project.Id, selected.UserId);
            if (!res.IsSuccess)
            {
                lblError.Text = res.ErrorMessage;
                lblError.Visible = true;
            }
            else
            {
                LoadData();
            }
        }

        private void BtnRemove_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            ManagerItem selected = lstManagers.SelectedItem as ManagerItem;
            if (selected == null)
            {
                lblError.Text = "Please select a manager to remove.";
                lblError.Visible = true;
                return;
            }

            Result res = _projectController.RemoveManager(_project.Id, selected.UserId);
            if (!res.IsSuccess)
            {
                lblError.Text = res.ErrorMessage;
                lblError.Visible = true;
            }
            else
            {
                LoadData();
            }
        }

        private class ManagerItem
        {
            public int UserId { get; private set; }
            public string Display { get; private set; }

            public ManagerItem(int id, string display)
            {
                this.UserId = id;
                this.Display = display;
            }

            public override string ToString()
            {
                return Display;
            }
        }
    }
}

