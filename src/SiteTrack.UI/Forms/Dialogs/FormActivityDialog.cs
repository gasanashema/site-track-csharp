using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormActivityDialog : Form
    {
        private readonly ActivityController _activityController;
        private readonly ProjectController _projectController;
        private readonly int? _preselectedProjectId;

        private ComboBox cmbProjects;
        private DateTimePicker dtpDate;
        private TextBox txtTitle;
        private NumericUpDown numProgress;
        private ProgressBar prgPreview;
        private Label lblProgressPreview;
        private TextBox txtDescription;
        private Button btnSave;
        private Button btnCancel;
        private ErrorProvider errorProvider;

        public FormActivityDialog() : this(null, new ActivityController(), new ProjectController())
        {
        }

        public FormActivityDialog(int? projectId) : this(projectId, new ActivityController(), new ProjectController())
        {
        }

        public FormActivityDialog(int? projectId, ActivityController activityController, ProjectController projectController)
        {
            this._preselectedProjectId = projectId;
            this._activityController = activityController ?? new ActivityController();
            this._projectController = projectController ?? new ProjectController();

            InitializeComponent();
            LoadProjects();
        }

        private void InitializeComponent()
        {
            this.Text = "Log Site Activity & Progress — SiteTrack";
            this.Size = new Size(500, 520);
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
            pnlHeader.Height = 70;
            pnlHeader.BackColor = Color.FromArgb(15, 23, 42); // Slate 900
            pnlHeader.Padding = new Padding(20, 12, 20, 12);

            Label lblTitle = new Label();
            lblTitle.Text = "Record Daily Site Activity & Milestone";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(18, 12);
            pnlHeader.Controls.Add(lblTitle);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Technical Rule 5: Progress percentage is constrained strictly between 0 and 100.";
            lblSubtitle.Font = new Font("Segoe UI", 8.5f);
            lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184); // Slate 400
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(20, 38);
            pnlHeader.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlHeader);

            int startY = 85;
            int labelWidth = 145;
            int fieldLeft = 165;
            int fieldWidth = 285;
            int rowHeight = 44;

            // 1. Project
            Label lblProj = new Label();
            lblProj.Text = "Target Project:*";
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

            // 2. Activity Date
            startY += rowHeight;
            Label lblDate = new Label();
            lblDate.Text = "Activity Date:*";
            lblDate.Location = new Point(20, startY);
            lblDate.Size = new Size(labelWidth, 24);
            lblDate.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblDate.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblDate);

            dtpDate = new DateTimePicker();
            dtpDate.Format = DateTimePickerFormat.Short;
            dtpDate.Value = DateTime.Today;
            dtpDate.Location = new Point(fieldLeft, startY - 2);
            dtpDate.Size = new Size(fieldWidth, 26);
            this.Controls.Add(dtpDate);

            // 3. Activity Title
            startY += rowHeight;
            Label lblActTitle = new Label();
            lblActTitle.Text = "Activity Title:*";
            lblActTitle.Location = new Point(20, startY);
            lblActTitle.Size = new Size(labelWidth, 24);
            lblActTitle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblActTitle.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblActTitle);

            txtTitle = new TextBox();
            txtTitle.Location = new Point(fieldLeft, startY - 2);
            txtTitle.Size = new Size(fieldWidth, 26);
            this.Controls.Add(txtTitle);

            // 4. Progress Percentage
            startY += rowHeight;
            Label lblProg = new Label();
            lblProg.Text = "Progress (%):*";
            lblProg.Location = new Point(20, startY);
            lblProg.Size = new Size(labelWidth, 24);
            lblProg.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblProg.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblProg);

            numProgress = new NumericUpDown();
            numProgress.Minimum = 0;
            numProgress.Maximum = 100;
            numProgress.Value = 10;
            numProgress.Location = new Point(fieldLeft, startY - 2);
            numProgress.Size = new Size(80, 26);
            numProgress.ValueChanged += delegate { UpdateProgressPreview(); };
            this.Controls.Add(numProgress);

            prgPreview = new ProgressBar();
            prgPreview.Minimum = 0;
            prgPreview.Maximum = 100;
            prgPreview.Value = 10;
            prgPreview.Location = new Point(fieldLeft + 90, startY);
            prgPreview.Size = new Size(130, 22);
            this.Controls.Add(prgPreview);

            lblProgressPreview = new Label();
            lblProgressPreview.Text = "10%";
            lblProgressPreview.Location = new Point(fieldLeft + 225, startY + 2);
            lblProgressPreview.Size = new Size(50, 20);
            lblProgressPreview.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblProgressPreview.ForeColor = Color.FromArgb(37, 99, 235);
            this.Controls.Add(lblProgressPreview);

            // 5. Detailed Description
            startY += rowHeight;
            Label lblDesc = new Label();
            lblDesc.Text = "Description / Notes:";
            lblDesc.Location = new Point(20, startY);
            lblDesc.Size = new Size(labelWidth, 24);
            lblDesc.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblDesc.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblDesc);

            txtDescription = new TextBox();
            txtDescription.Multiline = true;
            txtDescription.Location = new Point(fieldLeft, startY - 2);
            txtDescription.Size = new Size(fieldWidth, 75);
            txtDescription.ScrollBars = ScrollBars.Vertical;
            this.Controls.Add(txtDescription);

            // Action Buttons
            startY += 90;
            btnSave = new Button();
            btnSave.Text = "Log Activity";
            btnSave.Location = new Point(fieldLeft, startY);
            btnSave.Size = new Size(150, 36);
            btnSave.BackColor = Color.FromArgb(37, 99, 235); // Blue 600
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

        private void UpdateProgressPreview()
        {
            int val = (int)numProgress.Value;
            if (val < 0) val = 0;
            if (val > 100) val = 100;

            prgPreview.Value = val;
            lblProgressPreview.Text = string.Format("{0}%", val);
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

            string title = txtTitle.Text.Trim();
            if (string.IsNullOrEmpty(title))
            {
                errorProvider.SetError(txtTitle, "Activity title is required.");
                txtTitle.Focus();
                return;
            }

            int progress = (int)numProgress.Value;
            // Technical Rule 5 check
            if (progress < 0 || progress > 100)
            {
                MessageBox.Show("Technical Rule 5 Violation:\n\nProgress percentage must be an integer between 0 and 100 inclusive.",
                    "Invalid Progress",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                numProgress.Focus();
                return;
            }

            string desc = txtDescription.Text.Trim();

            Result<int> res = _activityController.RecordActivity(proj.Id, title, desc, dtpDate.Value, progress);
            if (!res.IsSuccess)
            {
                MessageBox.Show(res.ErrorMessage, "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(
                string.Format("Site activity logged successfully!\n\nProject: {0}\nActivity: {1}\nReported Progress: {2}%",
                    proj.Name, title, progress),
                "Activity Recorded",
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
    }
}
