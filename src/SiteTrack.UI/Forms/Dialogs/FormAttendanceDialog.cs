using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormAttendanceDialog : Form
    {
        private readonly LaborController _laborController;
        private readonly ProjectController _projectController;
        private readonly int? _preselectedProjectId;
        private readonly DateTime _workDate;

        private ComboBox cmbProjects;
        private DateTimePicker dtpDate;
        private ComboBox cmbWorkers;
        private ComboBox cmbStatus;
        private TextBox txtDescription;
        private Label lblWorkerRate;
        private Button btnSave;
        private Button btnCancel;
        private ErrorProvider errorProvider;

        public FormAttendanceDialog() : this(null, DateTime.Today, new LaborController(), new ProjectController())
        {
        }

        public FormAttendanceDialog(int? projectId, DateTime date) : this(projectId, date, new LaborController(), new ProjectController())
        {
        }

        public FormAttendanceDialog(int? projectId, DateTime date, LaborController laborController, ProjectController projectController)
        {
            this._preselectedProjectId = projectId;
            this._workDate = date;
            this._laborController = laborController ?? new LaborController();
            this._projectController = projectController ?? new ProjectController();

            InitializeComponent();
            LoadProjects();
            LoadWorkers();
        }

        private void InitializeComponent()
        {
            this.Text = "Record Daily Attendance — SiteTrack";
            this.Size = new Size(500, 480);
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
            lblTitle.Text = "Log Daily Worker Attendance";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(18, 12);
            pnlHeader.Controls.Add(lblTitle);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Only workers with PRESENT attendance can receive wage payments.";
            lblSubtitle.Font = new Font("Segoe UI", 8.5f);
            lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184); // Slate 400
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(20, 36);
            pnlHeader.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlHeader);

            int startY = 85;
            int labelWidth = 140;
            int fieldLeft = 160;
            int fieldWidth = 280;
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
            this.Controls.Add(cmbProjects);

            // 2. Work Date
            startY += rowHeight;
            Label lblDate = new Label();
            lblDate.Text = "Work Date:*";
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
            this.Controls.Add(dtpDate);

            // 3. Worker
            startY += rowHeight;
            Label lblWorker = new Label();
            lblWorker.Text = "Site Worker:*";
            lblWorker.Location = new Point(20, startY);
            lblWorker.Size = new Size(labelWidth, 24);
            lblWorker.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblWorker.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblWorker);

            cmbWorkers = new ComboBox();
            cmbWorkers.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWorkers.Location = new Point(fieldLeft, startY - 2);
            cmbWorkers.Size = new Size(fieldWidth, 26);
            cmbWorkers.SelectedIndexChanged += delegate { OnWorkerSelected(); };
            this.Controls.Add(cmbWorkers);

            // Daily Rate badge
            startY += 28;
            lblWorkerRate = new Label();
            lblWorkerRate.Text = "Daily Wage Benchmark: RWF 0.00";
            lblWorkerRate.Location = new Point(fieldLeft, startY);
            lblWorkerRate.Size = new Size(fieldWidth, 20);
            lblWorkerRate.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblWorkerRate.ForeColor = Color.FromArgb(37, 99, 235);
            this.Controls.Add(lblWorkerRate);

            // 4. Status
            startY += 28;
            Label lblStatus = new Label();
            lblStatus.Text = "Attendance Status:*";
            lblStatus.Location = new Point(20, startY);
            lblStatus.Size = new Size(labelWidth, 24);
            lblStatus.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblStatus.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblStatus);

            cmbStatus = new ComboBox();
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Items.AddRange(new object[] { "PRESENT", "ABSENT", "HALF_DAY" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.Location = new Point(fieldLeft, startY - 2);
            cmbStatus.Size = new Size(fieldWidth, 26);
            this.Controls.Add(cmbStatus);

            // 5. Work Description
            startY += rowHeight;
            Label lblDesc = new Label();
            lblDesc.Text = "Assigned Task / Note:";
            lblDesc.Location = new Point(20, startY);
            lblDesc.Size = new Size(labelWidth, 24);
            lblDesc.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblDesc.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblDesc);

            txtDescription = new TextBox();
            txtDescription.Multiline = true;
            txtDescription.Location = new Point(fieldLeft, startY - 2);
            txtDescription.Size = new Size(fieldWidth, 55);
            this.Controls.Add(txtDescription);

            // Action Buttons
            startY += 70;
            btnSave = new Button();
            btnSave.Text = "Save Attendance";
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

        private void LoadWorkers()
        {
            cmbWorkers.Items.Clear();
            List<SiteWorker> workers = _laborController.GetWorkers(null, "ACTIVE");
            foreach (SiteWorker w in workers)
            {
                cmbWorkers.Items.Add(new WorkerItem(w.Id, w.FullName, w.WorkerTypeName, w.DailyRate));
            }

            if (cmbWorkers.Items.Count > 0)
            {
                cmbWorkers.SelectedIndex = 0;
            }
        }

        private void OnWorkerSelected()
        {
            WorkerItem item = cmbWorkers.SelectedItem as WorkerItem;
            if (item != null)
            {
                lblWorkerRate.Text = string.Format("Daily Wage: RWF {0:N0} ({1})", item.DailyRate, item.TypeName);
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

            WorkerItem worker = cmbWorkers.SelectedItem as WorkerItem;
            if (worker == null)
            {
                errorProvider.SetError(cmbWorkers, "Select a worker.");
                return;
            }

            string status = cmbStatus.SelectedItem != null ? cmbStatus.SelectedItem.ToString() : "PRESENT";
            string desc = txtDescription.Text.Trim();

            Result<int> res = _laborController.RecordAttendance(proj.Id, worker.Id, dtpDate.Value, status, desc);
            if (!res.IsSuccess)
            {
                MessageBox.Show(res.ErrorMessage, "Attendance Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(
                string.Format("Attendance successfully logged!\n\nWorker: {0}\nStatus: {1}\nDate: {2:yyyy-MM-dd}",
                    worker.Name, status, dtpDate.Value),
                "Attendance Recorded",
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

        private class WorkerItem
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public string TypeName { get; private set; }
            public decimal DailyRate { get; private set; }

            public WorkerItem(int id, string name, string typeName, decimal dailyRate)
            {
                this.Id = id;
                this.Name = name;
                this.TypeName = typeName;
                this.DailyRate = dailyRate;
            }

            public override string ToString()
            {
                return string.Format("{0} ({1})", Name, TypeName);
            }
        }
    }
}
