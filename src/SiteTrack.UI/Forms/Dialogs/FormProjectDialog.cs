using System;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormProjectDialog : Form
    {
        private readonly ProjectController _projectController;
        private readonly Project _existingProject;

        private TextBox txtName;
        private TextBox txtLocation;
        private TextBox txtDescription;
        private DateTimePicker dtpStart;
        private DateTimePicker dtpEnd;
        private CheckBox chkHasEndDate;
        private ComboBox cmbStatus;
        private Button btnSave;
        private Button btnCancel;
        private Label lblError;

        public bool IsEditMode { get { return _existingProject != null; } }

        public FormProjectDialog(ProjectController projectController, Project existingProject = null)
        {
            this._projectController = projectController ?? new ProjectController();
            this._existingProject = existingProject;
            InitializeComponent();
            PopulateData();
        }

        private void InitializeComponent()
        {
            this.Text = IsEditMode ? "SiteTrack — Edit Project" : "SiteTrack — Create New Project";
            this.Size = new Size(520, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            int y = 20;

            Label lblTitle = new Label();
            lblTitle.Text = IsEditMode ? "Edit Construction Project" : "Register Construction Project";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, y);
            lblTitle.AutoSize = true;
            this.Controls.Add(lblTitle);
            y += 40;

            // Project Name
            AddLabel("Project Name *", 24, y);
            y += 20;
            txtName = new TextBox();
            txtName.Location = new Point(24, y);
            txtName.Size = new Size(456, 26);
            this.Controls.Add(txtName);
            y += 35;

            // Location
            AddLabel("Site Location *", 24, y);
            y += 20;
            txtLocation = new TextBox();
            txtLocation.Location = new Point(24, y);
            txtLocation.Size = new Size(456, 26);
            this.Controls.Add(txtLocation);
            y += 35;

            // Description
            AddLabel("Description & Scope", 24, y);
            y += 20;
            txtDescription = new TextBox();
            txtDescription.Multiline = true;
            txtDescription.Location = new Point(24, y);
            txtDescription.Size = new Size(456, 55);
            this.Controls.Add(txtDescription);
            y += 65;

            // Start Date
            AddLabel("Start Date *", 24, y);
            dtpStart = new DateTimePicker();
            dtpStart.Format = DateTimePickerFormat.Short;
            dtpStart.Location = new Point(24, y + 20);
            dtpStart.Size = new Size(200, 26);
            this.Controls.Add(dtpStart);

            // Status
            AddLabel("Project Status *", 260, y);
            cmbStatus = new ComboBox();
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Items.AddRange(new object[] { "Planning", "Ongoing", "Completed", "Cancelled" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.Location = new Point(260, y + 20);
            cmbStatus.Size = new Size(220, 26);
            this.Controls.Add(cmbStatus);
            y += 60;

            // End Date with enable checkbox
            chkHasEndDate = new CheckBox();
            chkHasEndDate.Text = "Expected End Date";
            chkHasEndDate.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            chkHasEndDate.ForeColor = Color.FromArgb(71, 85, 105);
            chkHasEndDate.Location = new Point(24, y);
            chkHasEndDate.AutoSize = true;
            chkHasEndDate.Checked = true;
            chkHasEndDate.CheckedChanged += delegate {
                dtpEnd.Enabled = chkHasEndDate.Checked;
            };
            this.Controls.Add(chkHasEndDate);

            dtpEnd = new DateTimePicker();
            dtpEnd.Format = DateTimePickerFormat.Short;
            dtpEnd.Location = new Point(24, y + 22);
            dtpEnd.Size = new Size(200, 26);
            dtpEnd.Value = DateTime.Today.AddMonths(6);
            this.Controls.Add(dtpEnd);
            y += 65;

            // Error Label
            lblError = new Label();
            lblError.Location = new Point(24, y);
            lblError.Size = new Size(456, 35);
            lblError.ForeColor = Color.FromArgb(220, 38, 38);
            lblError.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblError.Visible = false;
            this.Controls.Add(lblError);
            y += 40;

            // Action Buttons
            btnSave = new Button();
            btnSave.Text = IsEditMode ? "Save Changes" : "Create Project";
            btnSave.Location = new Point(260, y);
            btnSave.Size = new Size(120, 36);
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
            btnCancel.Location = new Point(390, y);
            btnCancel.Size = new Size(90, 36);
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
            if (_existingProject != null)
            {
                txtName.Text = _existingProject.ProjectName;
                txtLocation.Text = _existingProject.Location;
                txtDescription.Text = _existingProject.Description;
                dtpStart.Value = _existingProject.StartDate;
                cmbStatus.SelectedItem = _existingProject.Status;

                if (_existingProject.ExpectedEndDate.HasValue)
                {
                    chkHasEndDate.Checked = true;
                    dtpEnd.Value = _existingProject.ExpectedEndDate.Value;
                }
                else
                {
                    chkHasEndDate.Checked = false;
                    dtpEnd.Enabled = false;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            DateTime? endDate = chkHasEndDate.Checked ? (DateTime?)dtpEnd.Value.Date : null;
            string status = cmbStatus.SelectedItem != null ? cmbStatus.SelectedItem.ToString() : "Planning";

            if (IsEditMode)
            {
                Result res = _projectController.UpdateProject(
                    _existingProject.Id,
                    txtName.Text,
                    txtLocation.Text,
                    txtDescription.Text,
                    dtpStart.Value.Date,
                    endDate,
                    status
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
                Result<int> res = _projectController.CreateProject(
                    txtName.Text,
                    txtLocation.Text,
                    txtDescription.Text,
                    dtpStart.Value.Date,
                    endDate,
                    status
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

