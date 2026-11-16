using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormPaymentDialog : Form
    {
        private readonly LaborController _laborController;
        private readonly ProjectController _projectController;
        private readonly int? _preselectedProjectId;
        private readonly DateTime _workDate;

        private ComboBox cmbProjects;
        private DateTimePicker dtpDate;
        private ComboBox cmbAttendance;
        private Label lblAttendanceStatus;
        private Label lblAmountOwed;
        private TextBox txtAmountPaid;
        private Label lblBalanceDue;
        private TextBox txtNotes;
        private Button btnSave;
        private Button btnCancel;
        private ErrorProvider errorProvider;

        private decimal _currentDailyRate = 0m;
        private bool _isEligibleForPayment = false;

        public FormPaymentDialog() : this(null, DateTime.Today, new LaborController(), new ProjectController())
        {
        }

        public FormPaymentDialog(int? projectId, DateTime date) : this(projectId, date, new LaborController(), new ProjectController())
        {
        }

        public FormPaymentDialog(int? projectId, DateTime date, LaborController laborController, ProjectController projectController)
        {
            this._preselectedProjectId = projectId;
            this._workDate = date;
            this._laborController = laborController ?? new LaborController();
            this._projectController = projectController ?? new ProjectController();

            InitializeComponent();
            LoadProjects();
        }

        private void InitializeComponent()
        {
            this.Text = "Daily Wage Payment — SiteTrack";
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
            pnlHeader.Padding = new Padding(20, 12, 20, 12);

            Label lblTitle = new Label();
            lblTitle.Text = "Record Daily Wage Payment";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(18, 12);
            pnlHeader.Controls.Add(lblTitle);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Business Rule 2: Payments are strictly gated to workers marked PRESENT.";
            lblSubtitle.Font = new Font("Segoe UI", 8.5f);
            lblSubtitle.ForeColor = Color.FromArgb(187, 247, 208); // Green 200
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(20, 38);
            pnlHeader.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlHeader);

            int startY = 90;
            int labelWidth = 145;
            int fieldLeft = 165;
            int fieldWidth = 345;
            int rowHeight = 44;

            // 1. Project
            Label lblProj = new Label();
            lblProj.Text = "Project:*";
            lblProj.Location = new Point(20, startY);
            lblProj.Size = new Size(labelWidth, 24);
            lblProj.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblProj.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblProj);

            cmbProjects = new ComboBox();
            cmbProjects.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProjects.Location = new Point(fieldLeft, startY - 2);
            cmbProjects.Size = new Size(fieldWidth, 26);
            cmbProjects.SelectedIndexChanged += delegate { LoadAttendanceRecords(); };
            this.Controls.Add(cmbProjects);

            // 2. Work Date
            startY += rowHeight;
            Label lblDate = new Label();
            lblDate.Text = "Attendance Date:*";
            lblDate.Location = new Point(20, startY);
            lblDate.Size = new Size(labelWidth, 24);
            lblDate.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblDate.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblDate);

            dtpDate = new DateTimePicker();
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.Value = _workDate;
            dtpDate.Location = new Point(fieldLeft, startY - 2);
            dtpDate.Size = new Size(fieldWidth, 26);
            dtpDate.ValueChanged += delegate { LoadAttendanceRecords(); };
            this.Controls.Add(dtpDate);

            // 3. Attendance Record
            startY += rowHeight;
            Label lblAtt = new Label();
            lblAtt.Text = "Attendance Record:*";
            lblAtt.Location = new Point(20, startY);
            lblAtt.Size = new Size(labelWidth, 24);
            lblAtt.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblAtt.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblAtt);

            cmbAttendance = new ComboBox();
            cmbAttendance.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAttendance.Location = new Point(fieldLeft, startY - 2);
            cmbAttendance.Size = new Size(fieldWidth, 26);
            cmbAttendance.SelectedIndexChanged += delegate { OnAttendanceSelected(); };
            this.Controls.Add(cmbAttendance);

            // Status Badge
            startY += 30;
            lblAttendanceStatus = new Label();
            lblAttendanceStatus.Text = "Select attendance record to verify Business Rule 2 eligibility.";
            lblAttendanceStatus.Location = new Point(fieldLeft, startY);
            lblAttendanceStatus.Size = new Size(fieldWidth, 20);
            lblAttendanceStatus.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblAttendanceStatus.ForeColor = Color.FromArgb(100, 116, 139);
            this.Controls.Add(lblAttendanceStatus);

            // 4. Amount Owed
            startY += 26;
            Label lblOwedTitle = new Label();
            lblOwedTitle.Text = "Daily Wage (Owed):";
            lblOwedTitle.Location = new Point(20, startY);
            lblOwedTitle.Size = new Size(labelWidth, 24);
            lblOwedTitle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblOwedTitle.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblOwedTitle);

            lblAmountOwed = new Label();
            lblAmountOwed.Text = "RWF 0.00";
            lblAmountOwed.Location = new Point(fieldLeft, startY);
            lblAmountOwed.Size = new Size(fieldWidth, 24);
            lblAmountOwed.Font = new Font("Segoe UI", 10.5f, FontStyle.Bold);
            lblAmountOwed.ForeColor = Color.FromArgb(15, 23, 42);
            this.Controls.Add(lblAmountOwed);

            // 5. Amount Paid
            startY += 30;
            Label lblPaid = new Label();
            lblPaid.Text = "Amount Paid (RWF):*";
            lblPaid.Location = new Point(20, startY);
            lblPaid.Size = new Size(labelWidth, 24);
            lblPaid.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblPaid.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblPaid);

            txtAmountPaid = new TextBox();
            txtAmountPaid.Location = new Point(fieldLeft, startY - 2);
            txtAmountPaid.Size = new Size(fieldWidth, 26);
            txtAmountPaid.TextChanged += delegate { CalculateBalance(); };
            this.Controls.Add(txtAmountPaid);

            // Balance Due Indicator
            startY += 28;
            lblBalanceDue = new Label();
            lblBalanceDue.Text = "Balance Due: RWF 0.00 (Status: PAID)";
            lblBalanceDue.Location = new Point(fieldLeft, startY);
            lblBalanceDue.Size = new Size(fieldWidth, 20);
            lblBalanceDue.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblBalanceDue.ForeColor = Color.FromArgb(22, 101, 52);
            this.Controls.Add(lblBalanceDue);

            // 6. Notes / Memo
            startY += 28;
            Label lblNotes = new Label();
            lblNotes.Text = "Notes / Receipt Ref:";
            lblNotes.Location = new Point(20, startY);
            lblNotes.Size = new Size(labelWidth, 24);
            lblNotes.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblNotes.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblNotes);

            txtNotes = new TextBox();
            txtNotes.Multiline = true;
            txtNotes.Location = new Point(fieldLeft, startY - 2);
            txtNotes.Size = new Size(fieldWidth, 45);
            this.Controls.Add(txtNotes);

            // Action Buttons
            startY += 55;
            btnSave = new Button();
            btnSave.Text = "Confirm Payment";
            btnSave.Location = new Point(fieldLeft, startY);
            btnSave.Size = new Size(165, 38);
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
            btnCancel.Location = new Point(fieldLeft + 175, startY);
            btnCancel.Size = new Size(95, 38);
            btnCancel.BackColor = Color.FromArgb(241, 245, 249);
            btnCancel.ForeColor = Color.FromArgb(51, 65, 85);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.FlatAppearance.BorderSize = 1;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnCancel.Click += delegate { this.DialogResult = DialogResult.Cancel; this.Close(); };
            this.Controls.Add(btnCancel);
        }

        private void LoadProjects()
        {
            cmbProjects.Items.Clear();
            List<Project> projs = _projectController.GetProjectsForCurrentUser();
            int selectedIdx = 0;
            for (int i = 0; i < projs.Count; i++)
            {
                cmbProjects.Items.Add(new ProjectItem(projs[i].Id, projs[i].ProjectName));
                if (_preselectedProjectId.HasValue && projs[i].Id == _preselectedProjectId.Value)
                {
                    selectedIdx = i;
                }
            }

            if (cmbProjects.Items.Count > 0)
            {
                cmbProjects.SelectedIndex = selectedIdx;
            }
        }

        private void LoadAttendanceRecords()
        {
            cmbAttendance.Items.Clear();
            lblAttendanceStatus.Text = "";
            lblAmountOwed.Text = "RWF 0.00";
            txtAmountPaid.Text = "";
            lblBalanceDue.Text = "";
            _isEligibleForPayment = false;
            _currentDailyRate = 0m;
            btnSave.Enabled = false;

            ProjectItem proj = cmbProjects.SelectedItem as ProjectItem;
            if (proj == null) return;

            List<WorkerAttendance> list = _laborController.GetAttendance(proj.Id, dtpDate.Value);
            foreach (WorkerAttendance a in list)
            {
                cmbAttendance.Items.Add(new AttendanceItem(a));
            }

            if (cmbAttendance.Items.Count > 0)
            {
                cmbAttendance.SelectedIndex = 0;
            }
            else
            {
                lblAttendanceStatus.Text = "No attendance recorded for this project and date.";
                lblAttendanceStatus.ForeColor = Color.FromArgb(220, 38, 38);
            }
        }

        private void OnAttendanceSelected()
        {
            AttendanceItem item = cmbAttendance.SelectedItem as AttendanceItem;
            if (item == null) return;

            _currentDailyRate = item.Attendance.DailyRate;
            lblAmountOwed.Text = string.Format("RWF {0:N2}", _currentDailyRate);

            if (item.Attendance.IsPresent)
            {
                _isEligibleForPayment = true;
                lblAttendanceStatus.Text = "✓ Status: PRESENT (Eligible for Wage Payment)";
                lblAttendanceStatus.ForeColor = Color.FromArgb(22, 101, 52); // Green
                txtAmountPaid.Text = _currentDailyRate.ToString("F0");
                btnSave.Enabled = true;
            }
            else
            {
                _isEligibleForPayment = false;
                lblAttendanceStatus.Text = string.Format("⚠️ Rule 2 Violation: Worker is {0}. Only PRESENT workers are eligible.", item.Attendance.AttendanceStatus);
                lblAttendanceStatus.ForeColor = Color.FromArgb(220, 38, 38); // Red
                txtAmountPaid.Text = "0";
                btnSave.Enabled = false;
            }

            CalculateBalance();
        }

        private void CalculateBalance()
        {
            decimal paid;
            if (decimal.TryParse(txtAmountPaid.Text.Trim(), out paid))
            {
                decimal balance = _currentDailyRate - paid;
                if (balance <= 0)
                {
                    lblBalanceDue.Text = "Balance Due: RWF 0.00 (Status: Fully PAID)";
                    lblBalanceDue.ForeColor = Color.FromArgb(22, 101, 52);
                }
                else
                {
                    lblBalanceDue.Text = string.Format("Balance Due: RWF {0:N2} (Status: PENDING)", balance);
                    lblBalanceDue.ForeColor = Color.FromArgb(180, 83, 9); // Amber
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();

            ProjectItem proj = cmbProjects.SelectedItem as ProjectItem;
            if (proj == null)
            {
                errorProvider.SetError(cmbProjects, "Select a project.");
                return;
            }

            AttendanceItem att = cmbAttendance.SelectedItem as AttendanceItem;
            if (att == null)
            {
                errorProvider.SetError(cmbAttendance, "Select an attendance record.");
                return;
            }

            // Enforce Business Rule 2
            if (!_isEligibleForPayment || !att.Attendance.IsPresent)
            {
                MessageBox.Show(
                    string.Format("Business Rule 2 Violation:\n\nCannot create payment record.\nWorker '{0}' was marked {1} on {2:yyyy-MM-dd}.\n\nPayments are strictly gated to workers marked PRESENT.",
                        att.Attendance.WorkerName, att.Attendance.AttendanceStatus, dtpDate.Value),
                    "Attendance-Gated Payment Blocked",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            decimal amountPaid;
            if (!decimal.TryParse(txtAmountPaid.Text.Trim(), out amountPaid) || amountPaid < 0)
            {
                errorProvider.SetError(txtAmountPaid, "Enter a valid non-negative payment amount.");
                txtAmountPaid.Focus();
                return;
            }

            if (amountPaid > _currentDailyRate)
            {
                errorProvider.SetError(txtAmountPaid, string.Format("Amount paid cannot exceed daily wage rate (RWF {0:N2}).", _currentDailyRate));
                txtAmountPaid.Focus();
                return;
            }

            string notes = txtNotes.Text.Trim();

            Result<int> res = _laborController.RecordPayment(
                proj.Id,
                att.Attendance.WorkerId,
                att.Attendance.Id,
                dtpDate.Value,
                _currentDailyRate,
                amountPaid,
                notes);

            if (!res.IsSuccess)
            {
                MessageBox.Show(res.ErrorMessage, "Payment Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(
                string.Format("Wage payment recorded successfully!\n\nWorker: {0}\nAmount Paid: RWF {1:N2}\nStatus: {2}",
                    att.Attendance.WorkerName, amountPaid, amountPaid >= _currentDailyRate ? "PAID" : "PENDING"),
                "Payment Recorded",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private class ProjectItem
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public ProjectItem(int id, string name) { this.Id = id; this.Name = name; }
            public override string ToString() { return Name; }
        }

        private class AttendanceItem
        {
            public WorkerAttendance Attendance { get; private set; }

            public AttendanceItem(WorkerAttendance att)
            {
                this.Attendance = att;
            }

            public override string ToString()
            {
                return string.Format("{0} ({1}) — {2}", Attendance.WorkerName, Attendance.WorkerTypeName, Attendance.AttendanceStatus);
            }
        }
    }
}
