using System;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormUserDialog : Form
    {
        private readonly UserController _userController;
        private readonly User _existingUser;

        private TextBox txtFullName;
        private TextBox txtUsername;
        private TextBox txtEmail;
        private TextBox txtPassword;
        private ComboBox cmbRole;
        private TextBox txtPhone;
        private Button btnSave;
        private Button btnCancel;
        private Label lblError;

        public bool IsEditMode { get { return _existingUser != null; } }

        public FormUserDialog(UserController userController, User existingUser = null)
        {
            this._userController = userController ?? new UserController();
            this._existingUser = existingUser;
            InitializeComponent();
            PopulateData();
        }

        private void InitializeComponent()
        {
            this.Text = IsEditMode ? "SiteTrack — Edit User" : "SiteTrack — Add New User";
            this.Size = new Size(460, 480);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            int y = 20;

            Label lblTitle = new Label();
            lblTitle.Text = IsEditMode ? "Edit User Account" : "Create New User Account";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, y);
            lblTitle.AutoSize = true;
            this.Controls.Add(lblTitle);
            y += 40;

            // Full Name
            AddLabel("Full Name *", 24, y);
            y += 20;
            txtFullName = new TextBox();
            txtFullName.Location = new Point(24, y);
            txtFullName.Size = new Size(396, 26);
            this.Controls.Add(txtFullName);
            y += 35;

            // Username
            AddLabel("Username *", 24, y);
            y += 20;
            txtUsername = new TextBox();
            txtUsername.Location = new Point(24, y);
            txtUsername.Size = new Size(396, 26);
            this.Controls.Add(txtUsername);
            y += 35;

            // Email
            AddLabel("Email Address *", 24, y);
            y += 20;
            txtEmail = new TextBox();
            txtEmail.Location = new Point(24, y);
            txtEmail.Size = new Size(396, 26);
            this.Controls.Add(txtEmail);
            y += 35;

            // Password
            AddLabel(IsEditMode ? "Password (leave blank to keep current)" : "Password * (min 6 characters)", 24, y);
            y += 20;
            txtPassword = new TextBox();
            txtPassword.Location = new Point(24, y);
            txtPassword.Size = new Size(396, 26);
            txtPassword.PasswordChar = '●';
            this.Controls.Add(txtPassword);
            y += 35;

            // Role
            AddLabel("System Role *", 24, y);
            y += 20;
            cmbRole = new ComboBox();
            cmbRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbRole.Items.AddRange(new object[] { "SiteManager", "Administrator" });
            cmbRole.SelectedIndex = 0;
            cmbRole.Location = new Point(24, y);
            cmbRole.Size = new Size(190, 26);
            this.Controls.Add(cmbRole);

            // Phone
            Label lblPhone = new Label();
            lblPhone.Text = "Phone Number";
            lblPhone.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblPhone.ForeColor = Color.FromArgb(71, 85, 105);
            lblPhone.Location = new Point(230, y - 20);
            lblPhone.AutoSize = true;
            this.Controls.Add(lblPhone);

            txtPhone = new TextBox();
            txtPhone.Location = new Point(230, y);
            txtPhone.Size = new Size(190, 26);
            this.Controls.Add(txtPhone);
            y += 40;

            // Error Label
            lblError = new Label();
            lblError.Location = new Point(24, y);
            lblError.Size = new Size(396, 35);
            lblError.ForeColor = Color.FromArgb(220, 38, 38);
            lblError.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblError.Visible = false;
            this.Controls.Add(lblError);
            y += 40;

            // Action Buttons
            btnSave = new Button();
            btnSave.Text = IsEditMode ? "Update User" : "Create User";
            btnSave.Location = new Point(220, y);
            btnSave.Size = new Size(110, 36);
            btnSave.BackColor = Color.FromArgb(37, 99, 235);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(340, y);
            btnCancel.Size = new Size(80, 36);
            btnCancel.BackColor = Color.FromArgb(226, 232, 240);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void AddLabel(string text, int x, int y)
        {
            Label lbl = new Label();
            lbl.Text = text;
            lbl.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lbl.ForeColor = Color.FromArgb(71, 85, 105);
            lbl.Location = new Point(x, y);
            lbl.AutoSize = true;
            this.Controls.Add(lbl);
        }

        private void PopulateData()
        {
            if (_existingUser != null)
            {
                txtFullName.Text = _existingUser.FullName;
                txtUsername.Text = _existingUser.Username;
                txtEmail.Text = _existingUser.Email;
                txtPhone.Text = _existingUser.Phone;
                cmbRole.SelectedItem = _existingUser.Role;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            if (IsEditMode)
            {
                Result res = _userController.UpdateUser(
                    _existingUser.Id,
                    txtFullName.Text,
                    txtUsername.Text,
                    txtEmail.Text,
                    cmbRole.SelectedItem != null ? cmbRole.SelectedItem.ToString() : "SiteManager",
                    txtPhone.Text,
                    txtPassword.Text
                );

                if (!res.IsSuccess)
                {
                    lblError.Text = res.ErrorMessage;
                    lblError.Visible = true;
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
            else
            {
                Result<int> res = _userController.CreateUser(
                    txtFullName.Text,
                    txtUsername.Text,
                    txtEmail.Text,
                    txtPassword.Text,
                    cmbRole.SelectedItem != null ? cmbRole.SelectedItem.ToString() : "SiteManager",
                    txtPhone.Text
                );

                if (!res.IsSuccess)
                {
                    lblError.Text = res.ErrorMessage;
                    lblError.Visible = true;
                }
                else
                {
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }
    }
}

