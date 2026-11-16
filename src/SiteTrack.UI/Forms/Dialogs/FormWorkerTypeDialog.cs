using System;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormWorkerTypeDialog : Form
    {
        private readonly LaborController _laborController;
        private readonly int? _typeId;

        private TextBox txtTypeName;
        private TextBox txtDefaultRate;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;
        private ErrorProvider errorProvider;

        public FormWorkerTypeDialog() : this(null, new LaborController())
        {
        }

        public FormWorkerTypeDialog(int? typeId, LaborController laborController)
        {
            this._typeId = typeId;
            this._laborController = laborController ?? new LaborController();

            InitializeComponent();

            if (_typeId.HasValue)
            {
                LoadWorkerType();
            }
        }

        private void InitializeComponent()
        {
            this.Text = _typeId.HasValue ? "Edit Worker Category — SiteTrack" : "Add Worker Category — SiteTrack";
            this.Size = new Size(560, 620);
            this.MinimumSize = new Size(560, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            errorProvider = new ErrorProvider();
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            // Header Banner
            Panel pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 75;
            pnlHeader.BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            pnlHeader.Padding = new Padding(20, 12, 20, 12);

            Label lblTitle = new Label();
            lblTitle.Text = _typeId.HasValue ? "Edit Worker Skill Category" : "New Worker Skill Category";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(18, 12);
            pnlHeader.Controls.Add(lblTitle);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Standard daily labor wage benchmarks for construction trades.";
            lblSubtitle.Font = new Font("Segoe UI", 8.5f);
            lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184); // Slate 400
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(20, 36);
            pnlHeader.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlHeader);

            int startY = 90;
            int labelWidth = 140;
            int fieldLeft = 160;
            int fieldWidth = 345;
            int rowHeight = 44;

            // 1. Category Name
            Label lblName = new Label();
            lblName.Text = "Category Name:*";
            lblName.Location = new Point(20, startY);
            lblName.Size = new Size(labelWidth, 24);
            lblName.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblName.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblName);

            txtTypeName = new TextBox();
            txtTypeName.Location = new Point(fieldLeft, startY - 2);
            txtTypeName.Size = new Size(fieldWidth, 26);
            this.Controls.Add(txtTypeName);

            // 2. Default Daily Rate
            startY += rowHeight;
            Label lblRate = new Label();
            lblRate.Text = "Default Rate (RWF):*";
            lblRate.Location = new Point(20, startY);
            lblRate.Size = new Size(labelWidth, 24);
            lblRate.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblRate.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblRate);

            txtDefaultRate = new TextBox();
            txtDefaultRate.Location = new Point(fieldLeft, startY - 2);
            txtDefaultRate.Size = new Size(fieldWidth, 26);
            txtDefaultRate.Text = "5000";
            this.Controls.Add(txtDefaultRate);

            // 3. Description
            startY += rowHeight;
            Label lblDesc = new Label();
            lblDesc.Text = "Description:";
            lblDesc.Location = new Point(20, startY);
            lblDesc.Size = new Size(labelWidth, 24);
            lblDesc.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblDesc.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblDesc);

            txtDescription = new TextBox();
            txtDescription.Multiline = true;
            txtDescription.Location = new Point(fieldLeft, startY - 2);
            txtDescription.Size = new Size(fieldWidth, 50);
            this.Controls.Add(txtDescription);

            // Action Buttons
            startY += 65;
            btnSave = new Button();
            btnSave.Text = "Save Category";
            btnSave.Location = new Point(fieldLeft, startY);
            btnSave.Size = new Size(150, 38);
            btnSave.BackColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(fieldLeft + 160, startY);
            btnCancel.Size = new Size(95, 38);
            btnCancel.BackColor = Color.FromArgb(241, 245, 249);
            btnCancel.ForeColor = Color.FromArgb(51, 65, 85);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnCancel.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void LoadWorkerType()
        {
            WorkerType wt = _laborController.GetWorkerType(_typeId.Value);
            if (wt != null)
            {
                txtTypeName.Text = wt.TypeName;
                txtDefaultRate.Text = wt.DefaultDailyRate.ToString("F0");
                txtDescription.Text = wt.Description ?? "";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();

            string name = txtTypeName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                errorProvider.SetError(txtTypeName, "Category name is required.");
                txtTypeName.Focus();
                return;
            }

            decimal rate;
            if (!decimal.TryParse(txtDefaultRate.Text.Trim(), out rate) || rate < 0)
            {
                errorProvider.SetError(txtDefaultRate, "Enter a valid non-negative daily wage rate.");
                txtDefaultRate.Focus();
                return;
            }

            string desc = txtDescription.Text.Trim();

            if (_typeId.HasValue)
            {
                Result res = _laborController.UpdateWorkerType(_typeId.Value, name, rate, desc);
                if (!res.IsSuccess)
                {
                    MessageBox.Show(res.ErrorMessage, "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                Result<int> res = _laborController.CreateWorkerType(name, rate, desc);
                if (!res.IsSuccess)
                {
                    MessageBox.Show(res.ErrorMessage, "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
