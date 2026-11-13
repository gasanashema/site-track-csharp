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
    public class UcUsers : UserControl
    {
        private readonly UserController _userController;

        private DataGridView gridUsers;
        private TextBox txtSearch;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnToggleStatus;
        private Button btnRefresh;
        private Label lblTotal;

        public UcUsers() : this(new UserController())
        {
        }

        public UcUsers(UserController userController)
        {
            this._userController = userController ?? new UserController();
            InitializeComponent();
            LoadUsers();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Segoe UI", 9.5f);
            this.Padding = new Padding(20);

            // 1. Header & Toolbar Panel
            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 55;
            pnlTop.BackColor = Color.Transparent;

            btnAdd = new Button();
            btnAdd.Text = "+ Add User";
            btnAdd.Location = new Point(0, 8);
            btnAdd.Size = new Size(110, 36);
            btnAdd.BackColor = Color.FromArgb(37, 99, 235);
            btnAdd.ForeColor = Color.White;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnAdd.Cursor = Cursors.Hand;
            btnAdd.Click += BtnAdd_Click;
            pnlTop.Controls.Add(btnAdd);

            btnEdit = new Button();
            btnEdit.Text = "Edit User";
            btnEdit.Location = new Point(120, 8);
            btnEdit.Size = new Size(95, 36);
            btnEdit.BackColor = Color.FromArgb(241, 245, 249);
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.FlatAppearance.BorderSize = 1;
            btnEdit.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnEdit.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnEdit.Cursor = Cursors.Hand;
            btnEdit.Click += BtnEdit_Click;
            pnlTop.Controls.Add(btnEdit);

            btnToggleStatus = new Button();
            btnToggleStatus.Text = "Toggle Status";
            btnToggleStatus.Location = new Point(225, 8);
            btnToggleStatus.Size = new Size(115, 36);
            btnToggleStatus.BackColor = Color.FromArgb(241, 245, 249);
            btnToggleStatus.FlatStyle = FlatStyle.Flat;
            btnToggleStatus.FlatAppearance.BorderSize = 1;
            btnToggleStatus.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnToggleStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnToggleStatus.Cursor = Cursors.Hand;
            btnToggleStatus.Click += BtnToggleStatus_Click;
            pnlTop.Controls.Add(btnToggleStatus);

            btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Location = new Point(350, 8);
            btnRefresh.Size = new Size(80, 36);
            btnRefresh.BackColor = Color.FromArgb(241, 245, 249);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 1;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.Click += delegate { LoadUsers(); };
            pnlTop.Controls.Add(btnRefresh);

            // Search
            Label lblSearch = new Label();
            lblSearch.Text = "Search:";
            lblSearch.Location = new Point(460, 16);
            lblSearch.AutoSize = true;
            lblSearch.ForeColor = Color.FromArgb(100, 116, 139);
            pnlTop.Controls.Add(lblSearch);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(515, 12);
            txtSearch.Size = new Size(200, 26);
            txtSearch.TextChanged += delegate { FilterUsers(); };
            pnlTop.Controls.Add(txtSearch);

            lblTotal = new Label();
            lblTotal.Text = "Total: 0 users";
            lblTotal.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotal.Location = new Point(pnlTop.Width - 140, 16);
            lblTotal.AutoSize = true;
            lblTotal.ForeColor = Color.FromArgb(100, 116, 139);
            pnlTop.Controls.Add(lblTotal);

            this.Controls.Add(pnlTop);

            // 2. Data Grid
            gridUsers = new DataGridView();
            gridUsers.Dock = DockStyle.Fill;
            gridUsers.BackgroundColor = Color.White;
            gridUsers.BorderStyle = BorderStyle.None;
            gridUsers.RowHeadersVisible = false;
            gridUsers.AllowUserToAddRows = false;
            gridUsers.AllowUserToDeleteRows = false;
            gridUsers.ReadOnly = true;
            gridUsers.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridUsers.MultiSelect = false;
            gridUsers.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridUsers.RowTemplate.Height = 36;
            gridUsers.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            gridUsers.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            gridUsers.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            gridUsers.ColumnHeadersHeight = 40;
            gridUsers.EnableHeadersVisualStyles = false;

            gridUsers.Columns.Add("Id", "ID");
            gridUsers.Columns["Id"].Width = 50;
            gridUsers.Columns.Add("FullName", "Full Name");
            gridUsers.Columns.Add("Username", "Username");
            gridUsers.Columns.Add("Email", "Email");
            gridUsers.Columns.Add("Role", "System Role");
            gridUsers.Columns.Add("Phone", "Phone");
            gridUsers.Columns.Add("Status", "Status");

            gridUsers.CellFormatting += GridUsers_CellFormatting;
            this.Controls.Add(gridUsers);

            gridUsers.BringToFront();
        }

        private void GridUsers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (gridUsers.Columns[e.ColumnIndex].Name == "Status" && e.Value != null)
            {
                string status = e.Value.ToString();
                if (string.Equals(status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52); // Green 800
                    e.CellStyle.BackColor = Color.FromArgb(220, 252, 231); // Green 100
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
                else
                {
                    e.CellStyle.ForeColor = Color.FromArgb(153, 27, 27); // Red 800
                    e.CellStyle.BackColor = Color.FromArgb(254, 226, 226); // Red 100
                    e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            }
        }

        private List<User> _cachedUsers = new List<User>();

        private void LoadUsers()
        {
            _cachedUsers = _userController.GetAllUsers();
            FilterUsers();
        }

        private void FilterUsers()
        {
            gridUsers.Rows.Clear();
            string query = txtSearch.Text.Trim().ToLowerInvariant();

            foreach (User u in _cachedUsers)
            {
                if (string.IsNullOrEmpty(query) ||
                    u.FullName.ToLowerInvariant().Contains(query) ||
                    u.Username.ToLowerInvariant().Contains(query) ||
                    u.Email.ToLowerInvariant().Contains(query) ||
                    u.Role.ToLowerInvariant().Contains(query))
                {
                    gridUsers.Rows.Add(u.Id, u.FullName, u.Username, u.Email, u.Role, u.Phone, u.Status);
                }
            }

            lblTotal.Text = string.Format("Total: {0} users", gridUsers.Rows.Count);
        }

        private User GetSelectedUser()
        {
            if (gridUsers.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(gridUsers.SelectedRows[0].Cells["Id"].Value);
            return _userController.GetUserById(id);
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            using (FormUserDialog dlg = new FormUserDialog(_userController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadUsers();
                }
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            User selected = GetSelectedUser();
            if (selected == null)
            {
                MessageBox.Show("Please select a user to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FormUserDialog dlg = new FormUserDialog(_userController, selected))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadUsers();
                }
            }
        }

        private void BtnToggleStatus_Click(object sender, EventArgs e)
        {
            User selected = GetSelectedUser();
            if (selected == null)
            {
                MessageBox.Show("Please select a user to toggle status.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Result res = _userController.ToggleUserStatus(selected.Id);
            if (!res.IsSuccess)
            {
                MessageBox.Show(res.ErrorMessage, "Action Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else
            {
                LoadUsers();
            }
        }
    }
}
