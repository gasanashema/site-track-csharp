using System;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;
using SiteTrack.UI.Interfaces;

namespace SiteTrack.UI.Forms
{
    public class FormLogin : Form, ILoginView
    {
        private readonly AuthController _authController;

        private TextBox txtUsername;
        private TextBox txtPassword;
        private CheckBox chkShowPassword;
        private Button btnSignIn;
        private Label lblError;
        private Panel pnlHeader;
        private Panel pnlDemo;

        public string Username
        {
            get { return txtUsername.Text.Trim(); }
        }

        public string Password
        {
            get { return txtPassword.Text; }
        }

        public User AuthenticatedUser { get; private set; }

        public FormLogin() : this(new AuthController())
        {
        }

        public FormLogin(AuthController authController)
        {
            this._authController = authController ?? new AuthController();
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "SiteTrack — Authentication";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // 1. Header Panel
            pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 110;
            pnlHeader.BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            pnlHeader.Padding = new Padding(24, 20, 24, 16);

            Label lblAppTitle = new Label();
            lblAppTitle.Text = "SITETRACK";
            lblAppTitle.Font = new Font("Segoe UI", 18f, FontStyle.Bold);
            lblAppTitle.ForeColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
            lblAppTitle.AutoSize = true;
            lblAppTitle.Location = new Point(24, 20);

            Label lblAppSubtitle = new Label();
            lblAppSubtitle.Text = "Construction Site Manager • Desktop Edition";
            lblAppSubtitle.Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);
            lblAppSubtitle.ForeColor = Color.FromArgb(148, 163, 184); // Slate 400
            lblAppSubtitle.AutoSize = true;
            lblAppSubtitle.Location = new Point(26, 56);

            pnlHeader.Controls.Add(lblAppTitle);
            pnlHeader.Controls.Add(lblAppSubtitle);
            this.Controls.Add(pnlHeader);

            // 2. Form Content Container
            Panel pnlBody = new Panel();
            pnlBody.Dock = DockStyle.Fill;
            pnlBody.Padding = new Padding(32, 24, 32, 20);

            Label lblFormTitle = new Label();
            lblFormTitle.Text = "Sign In to Your Workspace";
            lblFormTitle.Font = new Font("Segoe UI", 13f, FontStyle.Bold);
            lblFormTitle.ForeColor = Color.FromArgb(30, 41, 59);
            lblFormTitle.AutoSize = true;
            lblFormTitle.Location = new Point(32, 125);
            this.Controls.Add(lblFormTitle);

            // Username
            Label lblUser = new Label();
            lblUser.Text = "Username or Email";
            lblUser.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblUser.ForeColor = Color.FromArgb(71, 85, 105);
            lblUser.Location = new Point(40, 165);
            lblUser.AutoSize = true;
            this.Controls.Add(lblUser);

            txtUsername = new TextBox();
            txtUsername.Location = new Point(40, 188);
            txtUsername.Size = new Size(404, 28);
            txtUsername.Font = new Font("Segoe UI", 10.5f);
            txtUsername.Text = "admin"; // Default demo fill
            this.Controls.Add(txtUsername);

            // Password
            Label lblPass = new Label();
            lblPass.Text = "Password";
            lblPass.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblPass.ForeColor = Color.FromArgb(71, 85, 105);
            lblPass.Location = new Point(40, 230);
            lblPass.AutoSize = true;
            this.Controls.Add(lblPass);

            txtPassword = new TextBox();
            txtPassword.Location = new Point(40, 253);
            txtPassword.Size = new Size(404, 28);
            txtPassword.Font = new Font("Segoe UI", 10.5f);
            txtPassword.PasswordChar = '●';
            txtPassword.Text = "Admin@123"; // Default demo fill
            txtPassword.KeyDown += delegate(object sender, KeyEventArgs e) {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    btnSignIn.PerformClick();
                }
            };
            this.Controls.Add(txtPassword);

            // Show Password
            chkShowPassword = new CheckBox();
            chkShowPassword.Text = "Show password";
            chkShowPassword.Location = new Point(40, 290);
            chkShowPassword.AutoSize = true;
            chkShowPassword.ForeColor = Color.FromArgb(100, 116, 139);
            chkShowPassword.CheckedChanged += delegate {
                txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '●';
            };
            this.Controls.Add(chkShowPassword);

            // Error Label
            lblError = new Label();
            lblError.Location = new Point(40, 320);
            lblError.Size = new Size(404, 40);
            lblError.ForeColor = Color.FromArgb(220, 38, 38); // Red 600
            lblError.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblError.Visible = false;
            this.Controls.Add(lblError);

            // Sign In Button
            btnSignIn = new Button();
            btnSignIn.Text = "Sign In";
            btnSignIn.Location = new Point(40, 368);
            btnSignIn.Size = new Size(404, 44);
            btnSignIn.BackColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
            btnSignIn.ForeColor = Color.White;
            btnSignIn.FlatStyle = FlatStyle.Flat;
            btnSignIn.FlatAppearance.BorderSize = 0;
            btnSignIn.Font = new Font("Segoe UI", 11f, FontStyle.Bold);
            btnSignIn.Cursor = Cursors.Hand;
            btnSignIn.Click += BtnSignIn_Click;
            this.Controls.Add(btnSignIn);

            // Demo Fill Bar
            pnlDemo = new Panel();
            pnlDemo.Location = new Point(40, 430);
            pnlDemo.Size = new Size(404, 85);
            pnlDemo.BackColor = Color.FromArgb(241, 245, 249);
            pnlDemo.BorderStyle = BorderStyle.FixedSingle;

            Label lblDemoHint = new Label();
            lblDemoHint.Text = "Quick Login (AUCA Evaluation Credentials):";
            lblDemoHint.Font = new Font("Segoe UI", 8f, FontStyle.Bold);
            lblDemoHint.ForeColor = Color.FromArgb(71, 85, 105);
            lblDemoHint.Location = new Point(8, 8);
            lblDemoHint.AutoSize = true;
            pnlDemo.Controls.Add(lblDemoHint);

            Button btnDemoAdmin = new Button();
            btnDemoAdmin.Text = "Admin (admin)";
            btnDemoAdmin.Location = new Point(10, 34);
            btnDemoAdmin.Size = new Size(185, 34);
            btnDemoAdmin.BackColor = Color.FromArgb(254, 237, 213); // Light Orange tint
            btnDemoAdmin.ForeColor = Color.FromArgb(194, 65, 12);   // Deep Orange
            btnDemoAdmin.FlatStyle = FlatStyle.Flat;
            btnDemoAdmin.FlatAppearance.BorderSize = 0;
            btnDemoAdmin.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnDemoAdmin.Click += delegate {
                txtUsername.Text = "admin";
                txtPassword.Text = "Admin@123";
                ClearError();
            };
            pnlDemo.Controls.Add(btnDemoAdmin);

            Button btnDemoSiteMgr = new Button();
            btnDemoSiteMgr.Text = "Site Mgr (sitemgr)";
            btnDemoSiteMgr.Location = new Point(205, 34);
            btnDemoSiteMgr.Size = new Size(185, 34);
            btnDemoSiteMgr.BackColor = Color.FromArgb(226, 232, 240);
            btnDemoSiteMgr.FlatStyle = FlatStyle.Flat;
            btnDemoSiteMgr.FlatAppearance.BorderSize = 0;
            btnDemoSiteMgr.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            btnDemoSiteMgr.Click += delegate {
                txtUsername.Text = "sitemgr";
                txtPassword.Text = "Admin@123";
                ClearError();
            };
            pnlDemo.Controls.Add(btnDemoSiteMgr);

            this.Controls.Add(pnlDemo);
        }

        private void BtnSignIn_Click(object sender, EventArgs e)
        {
            ClearError();

            if (string.IsNullOrEmpty(this.Username))
            {
                ShowError("Please enter your username.");
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(this.Password))
            {
                ShowError("Please enter your password.");
                txtPassword.Focus();
                return;
            }

            SetLoading(true);

            Result<User> result = _authController.Login(this.Username, this.Password);

            SetLoading(false);

            if (!result.IsSuccess)
            {
                ShowError(result.ErrorMessage);
                txtPassword.SelectAll();
                txtPassword.Focus();
            }
            else
            {
                this.AuthenticatedUser = result.Value;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        public void ShowError(string message)
        {
            lblError.Text = message;
            lblError.Visible = true;
        }

        public void ClearError()
        {
            lblError.Text = string.Empty;
            lblError.Visible = false;
        }

        public void SetLoading(bool isLoading)
        {
            btnSignIn.Enabled = !isLoading;
            btnSignIn.Text = isLoading ? "Authenticating..." : "Sign In";
        }
    }
}

