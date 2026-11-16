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
    public class UcLabor : UserControl
    {
        private readonly LaborController _laborController;
        private readonly ProjectController _projectController;

        private TabControl tabControl;

        // Tab 1: Attendance
        private TabPage tabAttendance;
        private ComboBox cmbProjectsAtt;
        private DateTimePicker dtpAttDate;
        private Button btnRecordAttendance;
        private Button btnPayPresentWorker;
        private Button btnRefreshAtt;
        private DataGridView gridAttendance;
        private Label lblMetricAttTotal;
        private Label lblMetricAttPresent;
        private Label lblMetricAttLiability;

        // Tab 2: Worker Registry
        private TabPage tabWorkers;
        private ComboBox cmbWorkerTypeFilter;
        private ComboBox cmbWorkerStatusFilter;
        private TextBox txtSearchWorkers;
        private Button btnNewWorker;
        private Button btnRefreshWorkers;
        private DataGridView gridWorkers;

        // Tab 3: Payments Ledger
        private TabPage tabPayments;
        private ComboBox cmbProjectsPay;
        private DateTimePicker dtpPayFrom;
        private DateTimePicker dtpPayTo;
        private ComboBox cmbPayStatusFilter;
        private Button btnRefreshPay;
        private DataGridView gridPayments;
        private Label lblMetricPayCount;
        private Label lblMetricPayPaid;
        private Label lblMetricPayDue;

        // Tab 4: Worker Types
        private TabPage tabTypes;
        private Button btnNewType;
        private Button btnRefreshTypes;
        private DataGridView gridTypes;

        public UcLabor() : this(new LaborController(), new ProjectController())
        {
        }

        public UcLabor(LaborController laborController, ProjectController projectController)
        {
            this._laborController = laborController ?? new LaborController();
            this._projectController = projectController ?? new ProjectController();

            InitializeComponent();
            LoadProjects();
            LoadWorkerTypeFilters();
            LoadAttendanceData();
            LoadWorkersData();
            LoadPaymentsData();
            LoadTypesData();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Segoe UI", 9.5f);
            this.Padding = new Padding(16);

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // Initialize all 4 tabs
            InitAttendanceTab();
            InitWorkersTab();
            InitPaymentsTab();
            InitTypesTab();

            this.Controls.Add(tabControl);
        }

        // ==========================================
        // 1. Attendance Tab
        // ==========================================
        private void InitAttendanceTab()
        {
            tabAttendance = new TabPage("Daily Attendance & Wage Allocation");
            tabAttendance.Padding = new Padding(12);
            tabAttendance.BackColor = Color.White;

            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 55;
            pnlTop.BackColor = Color.FromArgb(241, 245, 249);
            pnlTop.Padding = new Padding(12);

            Label lblProj = new Label();
            lblProj.Text = "Project:";
            lblProj.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblProj.ForeColor = Color.FromArgb(30, 41, 59);
            lblProj.Location = new Point(12, 16);
            lblProj.AutoSize = true;
            pnlTop.Controls.Add(lblProj);

            cmbProjectsAtt = new ComboBox();
            cmbProjectsAtt.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProjectsAtt.Location = new Point(70, 12);
            cmbProjectsAtt.Size = new Size(260, 26);
            cmbProjectsAtt.SelectedIndexChanged += delegate { LoadAttendanceData(); };
            pnlTop.Controls.Add(cmbProjectsAtt);

            Label lblDate = new Label();
            lblDate.Text = "Work Date:";
            lblDate.Location = new Point(345, 16);
            lblDate.AutoSize = true;
            pnlTop.Controls.Add(lblDate);

            dtpAttDate = new DateTimePicker();
            dtpAttDate.Format = DateTimePickerFormat.Short;
            dtpAttDate.Value = DateTime.Today;
            dtpAttDate.Location = new Point(420, 12);
            dtpAttDate.Size = new Size(110, 26);
            dtpAttDate.ValueChanged += delegate { LoadAttendanceData(); };
            pnlTop.Controls.Add(dtpAttDate);

            btnRecordAttendance = new Button();
            btnRecordAttendance.Text = "+ Log Attendance";
            btnRecordAttendance.Location = new Point(545, 8);
            btnRecordAttendance.Size = new Size(140, 36);
            btnRecordAttendance.BackColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
            btnRecordAttendance.ForeColor = Color.White;
            btnRecordAttendance.FlatStyle = FlatStyle.Flat;
            btnRecordAttendance.FlatAppearance.BorderSize = 0;
            btnRecordAttendance.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnRecordAttendance.Cursor = Cursors.Hand;
            btnRecordAttendance.Click += BtnRecordAttendance_Click;
            pnlTop.Controls.Add(btnRecordAttendance);

            btnPayPresentWorker = new Button();
            btnPayPresentWorker.Text = "$ Pay Present Worker";
            btnPayPresentWorker.Location = new Point(695, 8);
            btnPayPresentWorker.Size = new Size(160, 36);
            btnPayPresentWorker.BackColor = Color.FromArgb(22, 101, 52); // Green 800
            btnPayPresentWorker.ForeColor = Color.White;
            btnPayPresentWorker.FlatStyle = FlatStyle.Flat;
            btnPayPresentWorker.FlatAppearance.BorderSize = 0;
            btnPayPresentWorker.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnPayPresentWorker.Cursor = Cursors.Hand;
            btnPayPresentWorker.Click += BtnPayPresentWorker_Click;
            pnlTop.Controls.Add(btnPayPresentWorker);

            btnRefreshAtt = new Button();
            btnRefreshAtt.Text = "Refresh";
            btnRefreshAtt.Location = new Point(865, 8);
            btnRefreshAtt.Size = new Size(80, 36);
            btnRefreshAtt.BackColor = Color.White;
            btnRefreshAtt.FlatStyle = FlatStyle.Flat;
            btnRefreshAtt.FlatAppearance.BorderSize = 1;
            btnRefreshAtt.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefreshAtt.Click += delegate { LoadAttendanceData(); };
            pnlTop.Controls.Add(btnRefreshAtt);

            tabAttendance.Controls.Add(pnlTop);

            // Metrics Bar
            Panel pnlMetrics = new Panel();
            pnlMetrics.Dock = DockStyle.Top;
            pnlMetrics.Height = 70;
            pnlMetrics.BackColor = Color.White;
            pnlMetrics.Padding = new Padding(0, 10, 0, 10);

            Panel card1 = CreateMetricCard("Total Workers Logged", out lblMetricAttTotal, 0, Color.FromArgb(235, 86, 18));
            Panel card2 = CreateMetricCard("Present Workers", out lblMetricAttPresent, 210, Color.FromArgb(22, 101, 52));
            Panel card3 = CreateMetricCard("Daily Wage Liability", out lblMetricAttLiability, 420, Color.FromArgb(15, 23, 42), 240);

            pnlMetrics.Controls.Add(card1);
            pnlMetrics.Controls.Add(card2);
            pnlMetrics.Controls.Add(card3);
            tabAttendance.Controls.Add(pnlMetrics);

            // Grid
            gridAttendance = CreateStyledGrid();
            gridAttendance.Columns.Add("WorkerName", "Worker Name");
            gridAttendance.Columns.Add("SkillCategory", "Skill Category");
            gridAttendance.Columns.Add("Phone", "Phone Number");
            gridAttendance.Columns["Phone"].Width = 120;
            gridAttendance.Columns.Add("DailyRate", "Daily Rate (RWF)");
            gridAttendance.Columns["DailyRate"].Width = 130;
            gridAttendance.Columns.Add("Status", "Attendance Status");
            gridAttendance.Columns["Status"].Width = 140;
            gridAttendance.Columns.Add("Description", "Task / Activity Note");
            gridAttendance.Columns.Add("Eligibility", "Rule 2 Gate");
            gridAttendance.Columns["Eligibility"].Width = 130;

            gridAttendance.CellFormatting += GridAttendance_CellFormatting;
            tabAttendance.Controls.Add(gridAttendance);

            pnlMetrics.BringToFront();
            pnlTop.BringToFront();

            tabControl.TabPages.Add(tabAttendance);
        }

        // ==========================================
        // 2. Worker Registry Tab
        // ==========================================
        private void InitWorkersTab()
        {
            tabWorkers = new TabPage("Worker Registry (Aba-byizi & Craftsmen)");
            tabWorkers.Padding = new Padding(12);
            tabWorkers.BackColor = Color.White;

            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 55;
            pnlTop.BackColor = Color.FromArgb(241, 245, 249);
            pnlTop.Padding = new Padding(12);

            Label lblType = new Label();
            lblType.Text = "Category:";
            lblType.Location = new Point(12, 16);
            lblType.AutoSize = true;
            pnlTop.Controls.Add(lblType);

            cmbWorkerTypeFilter = new ComboBox();
            cmbWorkerTypeFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWorkerTypeFilter.Location = new Point(80, 12);
            cmbWorkerTypeFilter.Size = new Size(180, 26);
            cmbWorkerTypeFilter.SelectedIndexChanged += delegate { LoadWorkersData(); };
            pnlTop.Controls.Add(cmbWorkerTypeFilter);

            Label lblStatus = new Label();
            lblStatus.Text = "Status:";
            lblStatus.Location = new Point(275, 16);
            lblStatus.AutoSize = true;
            pnlTop.Controls.Add(lblStatus);

            cmbWorkerStatusFilter = new ComboBox();
            cmbWorkerStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbWorkerStatusFilter.Items.AddRange(new object[] { "ALL", "ACTIVE", "INACTIVE" });
            cmbWorkerStatusFilter.SelectedIndex = 0;
            cmbWorkerStatusFilter.Location = new Point(325, 12);
            cmbWorkerStatusFilter.Size = new Size(100, 26);
            cmbWorkerStatusFilter.SelectedIndexChanged += delegate { LoadWorkersData(); };
            pnlTop.Controls.Add(cmbWorkerStatusFilter);

            Label lblSearch = new Label();
            lblSearch.Text = "Search:";
            lblSearch.Location = new Point(440, 16);
            lblSearch.AutoSize = true;
            pnlTop.Controls.Add(lblSearch);

            txtSearchWorkers = new TextBox();
            txtSearchWorkers.Location = new Point(495, 12);
            txtSearchWorkers.Size = new Size(160, 26);
            txtSearchWorkers.TextChanged += delegate { LoadWorkersData(); };
            pnlTop.Controls.Add(txtSearchWorkers);

            btnNewWorker = new Button();
            btnNewWorker.Text = "+ Register Worker";
            btnNewWorker.Location = new Point(670, 8);
            btnNewWorker.Size = new Size(150, 36);
            btnNewWorker.BackColor = Color.FromArgb(22, 101, 52); // Green 800
            btnNewWorker.ForeColor = Color.White;
            btnNewWorker.FlatStyle = FlatStyle.Flat;
            btnNewWorker.FlatAppearance.BorderSize = 0;
            btnNewWorker.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnNewWorker.Cursor = Cursors.Hand;
            btnNewWorker.Click += BtnNewWorker_Click;
            pnlTop.Controls.Add(btnNewWorker);

            btnRefreshWorkers = new Button();
            btnRefreshWorkers.Text = "Refresh";
            btnRefreshWorkers.Location = new Point(830, 8);
            btnRefreshWorkers.Size = new Size(80, 36);
            btnRefreshWorkers.BackColor = Color.White;
            btnRefreshWorkers.FlatStyle = FlatStyle.Flat;
            btnRefreshWorkers.FlatAppearance.BorderSize = 1;
            btnRefreshWorkers.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefreshWorkers.Click += delegate { LoadWorkersData(); };
            pnlTop.Controls.Add(btnRefreshWorkers);

            tabWorkers.Controls.Add(pnlTop);

            gridWorkers = CreateStyledGrid();
            gridWorkers.Columns.Add("Id", "ID");
            gridWorkers.Columns["Id"].Width = 50;
            gridWorkers.Columns.Add("FullName", "Full Name");
            gridWorkers.Columns.Add("WorkerType", "Skill Category");
            gridWorkers.Columns.Add("Phone", "Phone Number");
            gridWorkers.Columns["Phone"].Width = 130;
            gridWorkers.Columns.Add("DailyRate", "Daily Wage Rate (RWF)");
            gridWorkers.Columns["DailyRate"].Width = 150;
            gridWorkers.Columns.Add("Status", "Status");
            gridWorkers.Columns["Status"].Width = 100;

            gridWorkers.CellDoubleClick += GridWorkers_CellDoubleClick;
            gridWorkers.CellFormatting += GridWorkers_CellFormatting;
            tabWorkers.Controls.Add(gridWorkers);

            pnlTop.BringToFront();

            tabControl.TabPages.Add(tabWorkers);
        }

        // ==========================================
        // 3. Payments Ledger Tab
        // ==========================================
        private void InitPaymentsTab()
        {
            tabPayments = new TabPage("Wage Payments Ledger");
            tabPayments.Padding = new Padding(12);
            tabPayments.BackColor = Color.White;

            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 55;
            pnlTop.BackColor = Color.FromArgb(241, 245, 249);
            pnlTop.Padding = new Padding(12);

            Label lblProj = new Label();
            lblProj.Text = "Project:";
            lblProj.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblProj.ForeColor = Color.FromArgb(30, 41, 59);
            lblProj.Location = new Point(12, 16);
            lblProj.AutoSize = true;
            pnlTop.Controls.Add(lblProj);

            cmbProjectsPay = new ComboBox();
            cmbProjectsPay.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProjectsPay.Location = new Point(70, 12);
            cmbProjectsPay.Size = new Size(240, 26);
            cmbProjectsPay.SelectedIndexChanged += delegate { LoadPaymentsData(); };
            pnlTop.Controls.Add(cmbProjectsPay);

            Label lblFrom = new Label();
            lblFrom.Text = "From:";
            lblFrom.Location = new Point(320, 16);
            lblFrom.AutoSize = true;
            pnlTop.Controls.Add(lblFrom);

            dtpPayFrom = new DateTimePicker();
            dtpPayFrom.Format = DateTimePickerFormat.Short;
            dtpPayFrom.Value = DateTime.Today.AddDays(-30);
            dtpPayFrom.Location = new Point(360, 12);
            dtpPayFrom.Size = new Size(100, 26);
            dtpPayFrom.ValueChanged += delegate { LoadPaymentsData(); };
            pnlTop.Controls.Add(dtpPayFrom);

            Label lblTo = new Label();
            lblTo.Text = "To:";
            lblTo.Location = new Point(470, 16);
            lblTo.AutoSize = true;
            pnlTop.Controls.Add(lblTo);

            dtpPayTo = new DateTimePicker();
            dtpPayTo.Format = DateTimePickerFormat.Short;
            dtpPayTo.Value = DateTime.Today.AddDays(1);
            dtpPayTo.Location = new Point(495, 12);
            dtpPayTo.Size = new Size(100, 26);
            dtpPayTo.ValueChanged += delegate { LoadPaymentsData(); };
            pnlTop.Controls.Add(dtpPayTo);

            Label lblStatus = new Label();
            lblStatus.Text = "Status:";
            lblStatus.Location = new Point(605, 16);
            lblStatus.AutoSize = true;
            pnlTop.Controls.Add(lblStatus);

            cmbPayStatusFilter = new ComboBox();
            cmbPayStatusFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPayStatusFilter.Items.AddRange(new object[] { "ALL", "PAID", "PENDING" });
            cmbPayStatusFilter.SelectedIndex = 0;
            cmbPayStatusFilter.Location = new Point(650, 12);
            cmbPayStatusFilter.Size = new Size(95, 26);
            cmbPayStatusFilter.SelectedIndexChanged += delegate { LoadPaymentsData(); };
            pnlTop.Controls.Add(cmbPayStatusFilter);

            btnRefreshPay = new Button();
            btnRefreshPay.Text = "Refresh";
            btnRefreshPay.Location = new Point(760, 8);
            btnRefreshPay.Size = new Size(80, 36);
            btnRefreshPay.BackColor = Color.White;
            btnRefreshPay.FlatStyle = FlatStyle.Flat;
            btnRefreshPay.FlatAppearance.BorderSize = 1;
            btnRefreshPay.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefreshPay.Click += delegate { LoadPaymentsData(); };
            pnlTop.Controls.Add(btnRefreshPay);

            tabPayments.Controls.Add(pnlTop);

            // Metrics Bar
            Panel pnlMetrics = new Panel();
            pnlMetrics.Dock = DockStyle.Top;
            pnlMetrics.Height = 70;
            pnlMetrics.BackColor = Color.White;
            pnlMetrics.Padding = new Padding(0, 10, 0, 10);

            Panel card1 = CreateMetricCard("Payment Vouchers", out lblMetricPayCount, 0, Color.FromArgb(235, 86, 18));
            Panel card2 = CreateMetricCard("Total Disbursed (Paid)", out lblMetricPayPaid, 210, Color.FromArgb(22, 101, 52), 240);
            Panel card3 = CreateMetricCard("Outstanding Balance", out lblMetricPayDue, 460, Color.FromArgb(220, 38, 38), 240);

            pnlMetrics.Controls.Add(card1);
            pnlMetrics.Controls.Add(card2);
            pnlMetrics.Controls.Add(card3);
            tabPayments.Controls.Add(pnlMetrics);

            // Grid
            gridPayments = CreateStyledGrid();
            gridPayments.Columns.Add("Date", "Work Date");
            gridPayments.Columns["Date"].Width = 100;
            gridPayments.Columns.Add("WorkerName", "Worker Name");
            gridPayments.Columns.Add("DailyRate", "Daily Rate (RWF)");
            gridPayments.Columns["DailyRate"].Width = 120;
            gridPayments.Columns.Add("AmountOwed", "Amount Owed (RWF)");
            gridPayments.Columns["AmountOwed"].Width = 135;
            gridPayments.Columns.Add("AmountPaid", "Amount Paid (RWF)");
            gridPayments.Columns["AmountPaid"].Width = 135;
            gridPayments.Columns.Add("BalanceDue", "Balance Due (RWF)");
            gridPayments.Columns["BalanceDue"].Width = 135;
            gridPayments.Columns.Add("Status", "Payment Status");
            gridPayments.Columns["Status"].Width = 110;
            gridPayments.Columns.Add("Notes", "Notes / Vouchers");
            gridPayments.Columns.Add("PaidBy", "Disbursed By");
            gridPayments.Columns["PaidBy"].Width = 110;

            gridPayments.CellFormatting += GridPayments_CellFormatting;
            tabPayments.Controls.Add(gridPayments);

            pnlMetrics.BringToFront();
            pnlTop.BringToFront();

            tabControl.TabPages.Add(tabPayments);
        }

        // ==========================================
        // 4. Worker Types Tab
        // ==========================================
        private void InitTypesTab()
        {
            tabTypes = new TabPage("Skill Categories & Standard Rates");
            tabTypes.Padding = new Padding(12);
            tabTypes.BackColor = Color.White;

            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 55;
            pnlTop.BackColor = Color.FromArgb(241, 245, 249);
            pnlTop.Padding = new Padding(12);

            btnNewType = new Button();
            btnNewType.Text = "+ Add Skill Category";
            btnNewType.Location = new Point(12, 8);
            btnNewType.Size = new Size(160, 36);
            btnNewType.BackColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
            btnNewType.ForeColor = Color.White;
            btnNewType.FlatStyle = FlatStyle.Flat;
            btnNewType.FlatAppearance.BorderSize = 0;
            btnNewType.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnNewType.Cursor = Cursors.Hand;
            btnNewType.Click += BtnNewType_Click;
            pnlTop.Controls.Add(btnNewType);

            btnRefreshTypes = new Button();
            btnRefreshTypes.Text = "Refresh";
            btnRefreshTypes.Location = new Point(185, 8);
            btnRefreshTypes.Size = new Size(80, 36);
            btnRefreshTypes.BackColor = Color.White;
            btnRefreshTypes.FlatStyle = FlatStyle.Flat;
            btnRefreshTypes.FlatAppearance.BorderSize = 1;
            btnRefreshTypes.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefreshTypes.Click += delegate { LoadTypesData(); };
            pnlTop.Controls.Add(btnRefreshTypes);

            tabTypes.Controls.Add(pnlTop);

            gridTypes = CreateStyledGrid();
            gridTypes.Columns.Add("Id", "ID");
            gridTypes.Columns["Id"].Width = 60;
            gridTypes.Columns.Add("TypeName", "Category Name");
            gridTypes.Columns.Add("DefaultDailyRate", "Standard Daily Rate (RWF)");
            gridTypes.Columns["DefaultDailyRate"].Width = 180;
            gridTypes.Columns.Add("Description", "Trade Scope / Description");

            gridTypes.CellDoubleClick += GridTypes_CellDoubleClick;
            tabTypes.Controls.Add(gridTypes);

            pnlTop.BringToFront();

            if (!BaseController.IsAdmin)
            {
                btnNewType.Visible = false;
            }

            tabControl.TabPages.Add(tabTypes);
        }

        private Panel CreateMetricCard(string label, out Label lblValue, int left, Color accent, int width = 190)
        {
            Panel card = new Panel();
            card.Location = new Point(left, 5);
            card.Size = new Size(width, 50);
            card.BackColor = Color.FromArgb(248, 250, 252);
            card.BorderStyle = BorderStyle.FixedSingle;

            Label lblTitle = new Label();
            lblTitle.Text = label.ToUpperInvariant();
            lblTitle.Font = new Font("Segoe UI", 7.5f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(100, 116, 139);
            lblTitle.Location = new Point(10, 6);
            lblTitle.AutoSize = true;
            card.Controls.Add(lblTitle);

            lblValue = new Label();
            lblValue.Text = "0";
            lblValue.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblValue.ForeColor = accent;
            lblValue.Location = new Point(10, 23);
            lblValue.AutoSize = true;
            card.Controls.Add(lblValue);

            return card;
        }

        private DataGridView CreateStyledGrid()
        {
            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.BackgroundColor = Color.White;
            grid.BorderStyle = BorderStyle.None;
            grid.RowHeadersVisible = false;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowTemplate.Height = 36;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            grid.ColumnHeadersHeight = 40;
            grid.EnableHeadersVisualStyles = false;
            grid.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 237, 213);
            grid.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);
            return grid;
        }

        // ==========================================
        // Data Loaders
        // ==========================================

        private void LoadProjects()
        {
            cmbProjectsAtt.Items.Clear();
            cmbProjectsPay.Items.Clear();

            List<Project> projs = _projectController.GetProjectsForCurrentUser();
            foreach (Project p in projs)
            {
                cmbProjectsAtt.Items.Add(new ProjectItem(p.Id, p.ProjectName));
                cmbProjectsPay.Items.Add(new ProjectItem(p.Id, p.ProjectName));
            }

            if (cmbProjectsAtt.Items.Count > 0)
            {
                cmbProjectsAtt.SelectedIndex = 0;
                cmbProjectsPay.SelectedIndex = 0;
            }
        }

        private void LoadWorkerTypeFilters()
        {
            cmbWorkerTypeFilter.Items.Clear();
            cmbWorkerTypeFilter.Items.Add(new FilterTypeItem(null, "All Categories"));

            List<WorkerType> types = _laborController.GetWorkerTypes();
            foreach (WorkerType t in types)
            {
                cmbWorkerTypeFilter.Items.Add(new FilterTypeItem(t.Id, t.TypeName));
            }
            cmbWorkerTypeFilter.SelectedIndex = 0;
        }

        public void LoadAttendanceData()
        {
            gridAttendance.Rows.Clear();

            ProjectItem proj = cmbProjectsAtt.SelectedItem as ProjectItem;
            if (proj == null) return;

            DateTime date = dtpAttDate.Value.Date;
            List<WorkerAttendance> list = _laborController.GetAttendance(proj.Id, date);

            int total = list.Count;
            int present = 0;
            decimal liability = 0m;

            foreach (WorkerAttendance a in list)
            {
                if (a.IsPresent)
                {
                    present++;
                    liability += a.DailyRate;
                }

                string gate = a.IsPresent ? "✓ Eligible" : "✗ Gated (Blocked)";

                gridAttendance.Rows.Add(
                    a.WorkerName,
                    a.WorkerTypeName,
                    a.WorkerPhone ?? "N/A",
                    string.Format("{0:N2}", a.DailyRate),
                    a.AttendanceStatus,
                    a.WorkDescription ?? "General Site Labor",
                    gate
                );
            }

            lblMetricAttTotal.Text = total.ToString();
            lblMetricAttPresent.Text = present.ToString();
            lblMetricAttLiability.Text = string.Format("RWF {0:N2}", liability);
        }

        public void LoadWorkersData()
        {
            gridWorkers.Rows.Clear();

            FilterTypeItem typeFilter = cmbWorkerTypeFilter.SelectedItem as FilterTypeItem;
            int? typeId = typeFilter != null ? typeFilter.Id : null;

            string statusFilter = cmbWorkerStatusFilter.SelectedItem != null ? cmbWorkerStatusFilter.SelectedItem.ToString() : "ALL";
            if (statusFilter == "ALL") statusFilter = null;

            string search = txtSearchWorkers != null ? txtSearchWorkers.Text.Trim().ToLowerInvariant() : "";

            List<SiteWorker> workers = _laborController.GetWorkers(typeId, statusFilter);
            foreach (SiteWorker w in workers)
            {
                if (!string.IsNullOrEmpty(search))
                {
                    bool match = (w.FullName != null && w.FullName.ToLowerInvariant().Contains(search)) ||
                                 (w.Phone != null && w.Phone.Contains(search)) ||
                                 (w.WorkerTypeName != null && w.WorkerTypeName.ToLowerInvariant().Contains(search));
                    if (!match) continue;
                }

                gridWorkers.Rows.Add(
                    w.Id,
                    w.FullName,
                    w.WorkerTypeName,
                    w.Phone ?? "N/A",
                    string.Format("{0:N2}", w.DailyRate),
                    w.Status
                );
            }
        }

        public void LoadPaymentsData()
        {
            gridPayments.Rows.Clear();

            ProjectItem proj = cmbProjectsPay.SelectedItem as ProjectItem;
            if (proj == null) return;

            DateTime from = dtpPayFrom.Value.Date;
            DateTime to = dtpPayTo.Value.Date.AddDays(1).AddSeconds(-1);

            string statusFilter = cmbPayStatusFilter.SelectedItem != null ? cmbPayStatusFilter.SelectedItem.ToString() : "ALL";
            if (statusFilter == "ALL") statusFilter = null;

            List<WorkerPayment> payments = _laborController.GetPayments(proj.Id, from, to, statusFilter);

            int count = payments.Count;
            decimal totalPaid = 0m;
            decimal totalDue = 0m;

            foreach (WorkerPayment p in payments)
            {
                totalPaid += p.AmountPaid;
                totalDue += p.BalanceDue;

                gridPayments.Rows.Add(
                    p.WorkDate.ToString("yyyy-MM-dd"),
                    p.WorkerName,
                    string.Format("{0:N2}", p.DailyRate),
                    string.Format("{0:N2}", p.AmountOwed),
                    string.Format("{0:N2}", p.AmountPaid),
                    string.Format("{0:N2}", p.BalanceDue),
                    p.PaymentStatus,
                    p.Notes ?? "-",
                    p.PaidByName ?? "Cashier"
                );
            }

            lblMetricPayCount.Text = count.ToString();
            lblMetricPayPaid.Text = string.Format("RWF {0:N2}", totalPaid);
            lblMetricPayDue.Text = string.Format("RWF {0:N2}", totalDue);
        }

        public void LoadTypesData()
        {
            gridTypes.Rows.Clear();
            List<WorkerType> types = _laborController.GetWorkerTypes();
            foreach (WorkerType t in types)
            {
                gridTypes.Rows.Add(
                    t.Id,
                    t.TypeName,
                    string.Format("{0:N2}", t.DefaultDailyRate),
                    t.Description ?? ""
                );
            }
        }

        // ==========================================
        // Event Handlers
        // ==========================================

        private void BtnRecordAttendance_Click(object sender, EventArgs e)
        {
            ProjectItem proj = cmbProjectsAtt.SelectedItem as ProjectItem;
            int? projId = proj != null ? (int?)proj.Id : null;

            using (FormAttendanceDialog dlg = new FormAttendanceDialog(projId, dtpAttDate.Value, _laborController, _projectController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadAttendanceData();
                }
            }
        }

        private void BtnPayPresentWorker_Click(object sender, EventArgs e)
        {
            ProjectItem proj = cmbProjectsAtt.SelectedItem as ProjectItem;
            int? projId = proj != null ? (int?)proj.Id : null;

            using (FormPaymentDialog dlg = new FormPaymentDialog(projId, dtpAttDate.Value, _laborController, _projectController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadAttendanceData();
                    LoadPaymentsData();
                }
            }
        }

        private void BtnNewWorker_Click(object sender, EventArgs e)
        {
            using (FormWorkerDialog dlg = new FormWorkerDialog(null, _laborController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadWorkersData();
                }
            }
        }

        private void GridWorkers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                int workerId = Convert.ToInt32(gridWorkers.Rows[e.RowIndex].Cells["Id"].Value);
                using (FormWorkerDialog dlg = new FormWorkerDialog(workerId, _laborController))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        LoadWorkersData();
                    }
                }
            }
        }

        private void BtnNewType_Click(object sender, EventArgs e)
        {
            using (FormWorkerTypeDialog dlg = new FormWorkerTypeDialog(null, _laborController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadTypesData();
                    LoadWorkerTypeFilters();
                }
            }
        }

        private void GridTypes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && BaseController.IsAdmin)
            {
                int typeId = Convert.ToInt32(gridTypes.Rows[e.RowIndex].Cells["Id"].Value);
                using (FormWorkerTypeDialog dlg = new FormWorkerTypeDialog(typeId, _laborController))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        LoadTypesData();
                        LoadWorkerTypeFilters();
                    }
                }
            }
        }

        private void GridAttendance_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && gridAttendance.Columns[e.ColumnIndex].Name == "Status")
            {
                object val = e.Value;
                if (val != null)
                {
                    string s = val.ToString();
                    if (s == "PRESENT")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52); // Green
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                    else if (s == "ABSENT")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38); // Red
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9); // Amber
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                }
            }
        }

        private void GridWorkers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && gridWorkers.Columns[e.ColumnIndex].Name == "Status")
            {
                object val = e.Value;
                if (val != null)
                {
                    string s = val.ToString();
                    if (s == "ACTIVE")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                }
            }
        }

        private void GridPayments_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && gridPayments.Columns[e.ColumnIndex].Name == "Status")
            {
                object val = e.Value;
                if (val != null)
                {
                    string s = val.ToString();
                    if (s == "PAID")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                    else
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9);
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                }
            }
        }

        private class ProjectItem
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public ProjectItem(int id, string name) { this.Id = id; this.Name = name; }
            public override string ToString() { return Name; }
        }

        private class FilterTypeItem
        {
            public int? Id { get; private set; }
            public string Name { get; private set; }
            public FilterTypeItem(int? id, string name) { this.Id = id; this.Name = name; }
            public override string ToString() { return Name; }
        }
    }
}
