using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormWorkerDialog : Form
    {
        private readonly LaborController _laborController;
        private readonly int? _workerId;

        private TextBox txtFullName;
        private TextBox txtPhone;
        private ComboBox cmbWorkerType;
        private TextBox txtDailyRate;
        private ComboBox cmbStatus;
        private Button btnSave;
        private Button btnCancel;
        private ErrorProvider errorProvider;

        public FormWorkerDialog() : this(null, new LaborController())
        {
        }

        public FormWorkerDialog(int? workerId, LaborController laborController)
        {
            this._workerId = workerId;
            this._laborController = laborController ?? new LaborController();

            InitializeComponent();
            LoadWorkerTypes();

            if (_workerId.HasValue)
            {
                LoadWorker();
            }
        }

        private void InitializeComponent()
        {
            this.Text = _workerId.HasValue ? "Edit Site Worker — SiteTrack" : "Register Site Worker — SiteTrack";
            this.Size = new Size(480, 420);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.White;
            this.Font = new Font("Segoe UI", 9.5f);

            errorProvider = new ErrorProvider();
            errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;

            // Header Banner
            Panel pnlHeader = new Panel();
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Height = 65;
            pnlHeader.BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            pnlHeader.Padding = new Padding(20, 12, 20, 12);

            Label lblTitle = new Label();
            lblTitle.Text = _workerId.HasValue ? "Edit Site Worker Profile" : "Register New Site Worker";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(18, 12);
            pnlHeader.Controls.Add(lblTitle);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Daily site laborers (Aba-byizi), masons, and specialized construction craftsmen.";
            lblSubtitle.Font = new Font("Segoe UI", 8.5f);
            lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184); // Slate 400
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(20, 36);
            pnlHeader.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlHeader);

            int startY = 85;
            int labelWidth = 140;
            int fieldLeft = 160;
            int fieldWidth = 270;
            int rowHeight = 44;

            // 1. Full Name
            Label lblName = new Label();
            lblName.Text = "Full Name:*";
            lblName.Location = new Point(20, startY);
            lblName.Size = new Size(labelWidth, 24);
            lblName.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblName.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblName);

            txtFullName = new TextBox();
            txtFullName.Location = new Point(fieldLeft, startY - 2);
            txtFullName.Size = new Size(fieldWidth, 26);
            this.Controls.Add(txtFullName);

            // 2. Skill Category
            startY += rowHeight;
            Label lblType = new Label();
            lblType.Text = "Skill Category:*";
            lblType.Location = new Point(20, startY);
            lblType.Size = new Size(labelWidth, 24);
            lblType.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblType.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblType);

            cmbWorkerType = new ComboBox();
            cmbWorkerType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWorkerType.Location = new Point(fieldLeft, startY - 2);
            cmbWorkerType.Size = new Size(fieldWidth, 26);
            cmbWorkerType.SelectedIndexChanged += delegate {
                TypeItem item = cmbWorkerType.SelectedItem as TypeItem;
                if (item != null && !_workerId.HasValue)
                {
                    txtDailyRate.Text = item.DefaultRate.ToString("F0");
                }
            };
            this.Controls.Add(cmbWorkerType);

            // 3. Phone
            startY += rowHeight;
            Label lblPhone = new Label();
            lblPhone.Text = "Phone Number:";
            lblPhone.Location = new Point(20, startY);
            lblPhone.Size = new Size(labelWidth, 24);
            lblPhone.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblPhone.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblPhone);

            txtPhone = new TextBox();
            txtPhone.Location = new Point(fieldLeft, startY - 2);
            txtPhone.Size = new Size(fieldWidth, 26);
            this.Controls.Add(txtPhone);

            // 4. Daily Wage Rate
            startY += rowHeight;
            Label lblRate = new Label();
            lblRate.Text = "Daily Rate (RWF):*";
            lblRate.Location = new Point(20, startY);
            lblRate.Size = new Size(labelWidth, 24);
            lblRate.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblRate.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblRate);

            txtDailyRate = new TextBox();
            txtDailyRate.Location = new Point(fieldLeft, startY - 2);
            txtDailyRate.Size = new Size(fieldWidth, 26);
            this.Controls.Add(txtDailyRate);

            // 5. Status
            startY += rowHeight;
            Label lblStatus = new Label();
            lblStatus.Text = "Status:*";
            lblStatus.Location = new Point(20, startY);
            lblStatus.Size = new Size(labelWidth, 24);
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblStatus);

            cmbStatus = new ComboBox();
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Items.AddRange(new object[] { "ACTIVE", "INACTIVE" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.Location = new Point(fieldLeft, startY - 2);
            cmbStatus.Size = new Size(fieldWidth, 26);
            this.Controls.Add(cmbStatus);

            // Action Buttons
            startY += 55;
            btnSave = new Button();
            btnSave.Text = _workerId.HasValue ? "Update Worker" : "Register Worker";
            btnSave.Location = new Point(fieldLeft, startY);
            btnSave.Size = new Size(150, 36);
            btnSave.BackColor = Color.FromArgb(22, 101, 52); // Green 800
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
            btnCancel.Size = new Size(95, 36);
            btnCancel.BackColor = Color.FromArgb(241, 245, 249);
            btnCancel.ForeColor = Color.FromArgb(51, 65, 85);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnCancel.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void LoadWorkerTypes()
        {
            cmbWorkerType.Items.Clear();
            List<WorkerType> types = _laborController.GetWorkerTypes();
            foreach (WorkerType t in types)
            {
                cmbWorkerType.Items.Add(new TypeItem(t.Id, t.TypeName, t.DefaultDailyRate));
            }
            if (cmbWorkerType.Items.Count > 0)
            {
                cmbWorkerType.SelectedIndex = 0;
            }
        }

        private void LoadWorker()
        {
            SiteWorker worker = _laborController.GetWorker(_workerId.Value);
            if (worker != null)
            {
                txtFullName.Text = worker.FullName;
                txtPhone.Text = worker.Phone ?? "";
                txtDailyRate.Text = worker.DailyRate.ToString("F0");

                for (int i = 0; i < cmbWorkerType.Items.Count; i++)
                {
                    TypeItem item = cmbWorkerType.Items[i] as TypeItem;
                    if (item != null && item.Id == worker.WorkerTypeId)
                    {
                        cmbWorkerType.SelectedIndex = i;
                        break;
                    }
                }

                cmbStatus.SelectedItem = worker.Status ?? "ACTIVE";
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();

            string fullName = txtFullName.Text.Trim();
            if (string.IsNullOrEmpty(fullName))
            {
                errorProvider.SetError(txtFullName, "Worker full name is required.");
                txtFullName.Focus();
                return;
            }

            TypeItem typeItem = cmbWorkerType.SelectedItem as TypeItem;
            if (typeItem == null)
            {
                errorProvider.SetError(cmbWorkerType, "Select a skill category.");
                return;
            }

            decimal rate;
            if (!decimal.TryParse(txtDailyRate.Text.Trim(), out rate) || rate < 0)
            {
                errorProvider.SetError(txtDailyRate, "Enter a valid non-negative daily wage rate.");
                txtDailyRate.Focus();
                return;
            }

            string phone = txtPhone.Text.Trim();
            string status = cmbStatus.SelectedItem != null ? cmbStatus.SelectedItem.ToString() : "ACTIVE";

            if (_workerId.HasValue)
            {
                Result res = _laborController.UpdateWorker(_workerId.Value, typeItem.Id, fullName, phone, rate, status);
                if (!res.IsSuccess)
                {
                    MessageBox.Show(res.ErrorMessage, "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            else
            {
                Result<int> res = _laborController.CreateWorker(typeItem.Id, fullName, phone, rate, status);
                if (!res.IsSuccess)
                {
                    MessageBox.Show(res.ErrorMessage, "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private class TypeItem
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public decimal DefaultRate { get; private set; }

            public TypeItem(int id, string name, decimal rate)
            {
                this.Id = id;
                this.Name = name;
                this.DefaultRate = rate;
            }

            public override string ToString()
            {
                return string.Format("{0} (Default: RWF {1:N0})", Name, DefaultRate);
            }
        }
    }
}
