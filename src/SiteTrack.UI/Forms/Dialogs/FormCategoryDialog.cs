using System;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormCategoryDialog : Form
    {
        private readonly MaterialController _materialController;
        private readonly MaterialCategory _existingCategory;

        private TextBox txtName;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;
        private Label lblError;

        public bool IsEditMode { get { return _existingCategory != null; } }

        public FormCategoryDialog(MaterialController materialController, MaterialCategory existingCategory = null)
        {
            this._materialController = materialController ?? new MaterialController();
            this._existingCategory = existingCategory;
            InitializeComponent();
            PopulateData();
        }

        private void InitializeComponent()
        {
            this.Text = IsEditMode ? "SiteTrack — Edit Category" : "SiteTrack — Add Material Category";
            this.Size = new Size(560, 620);
            this.MinimumSize = new Size(560, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            Label lblTitle = new Label();
            lblTitle.Text = IsEditMode ? "Edit Material Category" : "New Material Category";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, 24);
            lblTitle.AutoSize = true;
            this.Controls.Add(lblTitle);

            Label lblName = new Label();
            lblName.Text = "Category Name *";
            lblName.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblName.ForeColor = Color.FromArgb(71, 85, 105);
            lblName.Location = new Point(24, 75);
            lblName.AutoSize = true;
            this.Controls.Add(lblName);

            txtName = new TextBox();
            txtName.Location = new Point(24, 98);
            txtName.Size = new Size(490, 26);
            this.Controls.Add(txtName);

            Label lblDesc = new Label();
            lblDesc.Text = "Description & Purpose";
            lblDesc.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblDesc.ForeColor = Color.FromArgb(71, 85, 105);
            lblDesc.Location = new Point(24, 145);
            lblDesc.AutoSize = true;
            this.Controls.Add(lblDesc);

            txtDescription = new TextBox();
            txtDescription.Multiline = true;
            txtDescription.Location = new Point(24, 168);
            txtDescription.Size = new Size(490, 80);
            this.Controls.Add(txtDescription);

            lblError = new Label();
            lblError.Location = new Point(24, 265);
            lblError.Size = new Size(490, 30);
            lblError.ForeColor = Color.FromArgb(220, 38, 38);
            lblError.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblError.Visible = false;
            this.Controls.Add(lblError);

            btnSave = new Button();
            btnSave.Text = IsEditMode ? "Save Changes" : "Create Category";
            btnSave.Location = new Point(290, 310);
            btnSave.Size = new Size(130, 38);
            btnSave.BackColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(430, 310);
            btnCancel.Size = new Size(84, 38);
            btnCancel.BackColor = Color.FromArgb(226, 232, 240);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void PopulateData()
        {
            if (_existingCategory != null)
            {
                txtName.Text = _existingCategory.CategoryName;
                txtDescription.Text = _existingCategory.Description;
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            if (IsEditMode)
            {
                Result res = _materialController.UpdateCategory(_existingCategory.Id, txtName.Text, txtDescription.Text);
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
                Result<int> res = _materialController.CreateCategory(txtName.Text, txtDescription.Text);
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
