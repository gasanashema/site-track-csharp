using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;
using SiteTrack.UI.Export;

namespace SiteTrack.UI.Controls
{
    public class UcReports : UserControl
    {
        private readonly ReportController _reportController;
        private readonly ProjectController _projectController;

        private ComboBox cmbReportType;
        private ComboBox cmbProjects;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private Button btnGenerate;
        private Button btnExportCsv;
        private Button btnPrintPdf;

        private Label lblMetric1;
        private Label lblMetric2;
        private Label lblMetric3;

        private DataGridView gridReport;

        public UcReports() : this(new ReportController(), new ProjectController())
        {
        }

        public UcReports(ReportController reportController, ProjectController projectController)
        {
            this._reportController = reportController ?? new ReportController();
            this._projectController = projectController ?? new ProjectController();

            InitializeComponent();
            LoadProjects();
            GenerateSelectedReport();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Segoe UI", 9.5f);
            this.Padding = new Padding(16);

            // 1. Filter & Options Top Bar
            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 65;
            pnlTop.BackColor = Color.FromArgb(241, 245, 249);
            pnlTop.Padding = new Padding(12);

            Label lblType = new Label();
            lblType.Text = "Report:";
            lblType.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblType.ForeColor = Color.FromArgb(30, 41, 59);
            lblType.Location = new Point(12, 18);
            lblType.AutoSize = true;
            pnlTop.Controls.Add(lblType);

            cmbReportType = new ComboBox();
            cmbReportType.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbReportType.Items.AddRange(new object[] {
                "Material Consumption & Expenditure",
                "Labor Wages & Attendance Summary",
                "Stock Movement Audit Ledger (IN/OUT/ADJ)",
                "Executive Project Cost & Progress Overview"
            });
            cmbReportType.SelectedIndex = 0;
            cmbReportType.Location = new Point(72, 14);
            cmbReportType.Size = new Size(270, 26);
            cmbReportType.SelectedIndexChanged += delegate { GenerateSelectedReport(); };
            pnlTop.Controls.Add(cmbReportType);

            Label lblProj = new Label();
            lblProj.Text = "Project:";
            lblProj.Location = new Point(350, 18);
            lblProj.AutoSize = true;
            pnlTop.Controls.Add(lblProj);

            cmbProjects = new ComboBox();
            cmbProjects.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProjects.Location = new Point(400, 14);
            cmbProjects.Size = new Size(200, 26);
            cmbProjects.SelectedIndexChanged += delegate { GenerateSelectedReport(); };
            pnlTop.Controls.Add(cmbProjects);

            Label lblFrom = new Label();
            lblFrom.Text = "From:";
            lblFrom.Location = new Point(610, 18);
            lblFrom.AutoSize = true;
            pnlTop.Controls.Add(lblFrom);

            dtpFrom = new DateTimePicker();
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.Value = DateTime.Today.AddDays(-60);
            dtpFrom.Location = new Point(650, 14);
            dtpFrom.Size = new Size(95, 26);
            pnlTop.Controls.Add(dtpFrom);

            Label lblTo = new Label();
            lblTo.Text = "To:";
            lblTo.Location = new Point(755, 18);
            lblTo.AutoSize = true;
            pnlTop.Controls.Add(lblTo);

            dtpTo = new DateTimePicker();
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.Value = DateTime.Today.AddDays(1);
            dtpTo.Location = new Point(780, 14);
            dtpTo.Size = new Size(95, 26);
            pnlTop.Controls.Add(dtpTo);

            btnGenerate = new Button();
            btnGenerate.Text = "Generate";
            btnGenerate.Location = new Point(885, 10);
            btnGenerate.Size = new Size(80, 36);
            btnGenerate.BackColor = Color.FromArgb(37, 99, 235); // Blue 600
            btnGenerate.ForeColor = Color.White;
            btnGenerate.FlatStyle = FlatStyle.Flat;
            btnGenerate.FlatAppearance.BorderSize = 0;
            btnGenerate.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnGenerate.Cursor = Cursors.Hand;
            btnGenerate.Click += delegate { GenerateSelectedReport(); };
            pnlTop.Controls.Add(btnGenerate);

            btnExportCsv = new Button();
            btnExportCsv.Text = "Export CSV";
            btnExportCsv.Location = new Point(975, 10);
            btnExportCsv.Size = new Size(95, 36);
            btnExportCsv.BackColor = Color.FromArgb(22, 101, 52); // Green 800
            btnExportCsv.ForeColor = Color.White;
            btnExportCsv.FlatStyle = FlatStyle.Flat;
            btnExportCsv.FlatAppearance.BorderSize = 0;
            btnExportCsv.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnExportCsv.Cursor = Cursors.Hand;
            btnExportCsv.Click += BtnExportCsv_Click;
            pnlTop.Controls.Add(btnExportCsv);

            btnPrintPdf = new Button();
            btnPrintPdf.Text = "Print / PDF";
            btnPrintPdf.Location = new Point(1080, 10);
            btnPrintPdf.Size = new Size(95, 36);
            btnPrintPdf.BackColor = Color.FromArgb(30, 41, 59); // Slate 800
            btnPrintPdf.ForeColor = Color.White;
            btnPrintPdf.FlatStyle = FlatStyle.Flat;
            btnPrintPdf.FlatAppearance.BorderSize = 0;
            btnPrintPdf.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnPrintPdf.Cursor = Cursors.Hand;
            btnPrintPdf.Click += BtnPrintPdf_Click;
            pnlTop.Controls.Add(btnPrintPdf);

            this.Controls.Add(pnlTop);

            // 2. Metrics Summary Bar
            Panel pnlMetrics = new Panel();
            pnlMetrics.Dock = DockStyle.Top;
            pnlMetrics.Height = 70;
            pnlMetrics.BackColor = Color.White;
            pnlMetrics.Padding = new Padding(0, 10, 0, 10);

            Panel card1 = CreateMetricCard("Metric 1", out lblMetric1, 0, Color.FromArgb(37, 99, 235));
            Panel card2 = CreateMetricCard("Metric 2", out lblMetric2, 220, Color.FromArgb(22, 101, 52), 260);
            Panel card3 = CreateMetricCard("Metric 3", out lblMetric3, 490, Color.FromArgb(185, 28, 28), 260);

            pnlMetrics.Controls.Add(card1);
            pnlMetrics.Controls.Add(card2);
            pnlMetrics.Controls.Add(card3);
            this.Controls.Add(pnlMetrics);

            // 3. Data Grid
            gridReport = new DataGridView();
            gridReport.Dock = DockStyle.Fill;
            gridReport.BackgroundColor = Color.White;
            gridReport.BorderStyle = BorderStyle.None;
            gridReport.RowHeadersVisible = false;
            gridReport.AllowUserToAddRows = false;
            gridReport.AllowUserToDeleteRows = false;
            gridReport.ReadOnly = true;
            gridReport.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridReport.MultiSelect = false;
            gridReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridReport.RowTemplate.Height = 36;
            gridReport.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            gridReport.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            gridReport.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            gridReport.ColumnHeadersHeight = 40;
            gridReport.EnableHeadersVisualStyles = false;

            this.Controls.Add(gridReport);

            pnlMetrics.BringToFront();
            pnlTop.BringToFront();
        }

        private Panel CreateMetricCard(string label, out Label lblValue, int left, Color accent, int width = 210)
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
            lblValue.Text = "-";
            lblValue.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblValue.ForeColor = accent;
            lblValue.Location = new Point(10, 23);
            lblValue.AutoSize = true;
            card.Controls.Add(lblValue);

            return card;
        }

        private void LoadProjects()
        {
            cmbProjects.Items.Clear();
            List<Project> projs = _projectController.GetProjectsForCurrentUser();
            foreach (Project p in projs)
            {
                cmbProjects.Items.Add(new ProjectItem(p.Id, p.ProjectName));
            }

            if (cmbProjects.Items.Count > 0)
            {
                cmbProjects.SelectedIndex = 0;
            }
        }

        private int? GetSelectedProjectId()
        {
            ProjectItem item = cmbProjects.SelectedItem as ProjectItem;
            return item != null ? (int?)item.Id : null;
        }

        public void GenerateSelectedReport()
        {
            int reportIdx = cmbReportType.SelectedIndex;
            int? projId = GetSelectedProjectId();

            if (!projId.HasValue && reportIdx != 3) return;

            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date;

            gridReport.Columns.Clear();
            gridReport.Rows.Clear();

            switch (reportIdx)
            {
                case 0: // Material Consumption
                    GenerateMaterialConsumption(projId.Value, from, to);
                    break;
                case 1: // Labor Wages
                    GenerateLaborWages(projId.Value, from, to);
                    break;
                case 2: // Stock Movement Audit
                    GenerateStockMovements(projId.Value, from, to);
                    break;
                case 3: // Executive Overview
                    GenerateExecutiveOverview();
                    break;
            }
        }

        private void GenerateMaterialConsumption(int projId, DateTime from, DateTime to)
        {
            List<MaterialConsumptionReportItem> items = _reportController.GetMaterialConsumptionReport(projId, from, to);

            gridReport.Columns.Add("Date", "Usage Date");
            gridReport.Columns["Date"].Width = 105;
            gridReport.Columns.Add("Material", "Material Item");
            gridReport.Columns.Add("Category", "Category");
            gridReport.Columns.Add("Quantity", "Quantity Used");
            gridReport.Columns["Quantity"].Width = 110;
            gridReport.Columns.Add("UnitPrice", "Avg Cost (RWF)");
            gridReport.Columns["UnitPrice"].Width = 130;
            gridReport.Columns.Add("TotalCost", "Total Value (RWF)");
            gridReport.Columns["TotalCost"].Width = 140;
            gridReport.Columns.Add("Activity", "Site Task Description");
            gridReport.Columns.Add("RecordedBy", "Recorded By");

            decimal totalCost = 0m;
            decimal totalQty = 0m;

            foreach (MaterialConsumptionReportItem i in items)
            {
                totalCost += i.TotalCost;
                totalQty += i.QuantityUsed;

                gridReport.Rows.Add(
                    i.UsageDate.ToString("yyyy-MM-dd"),
                    i.MaterialName,
                    i.CategoryName,
                    string.Format("{0:N2} {1}", i.QuantityUsed, i.Unit),
                    string.Format("{0:N2}", i.UnitPrice),
                    string.Format("{0:N2}", i.TotalCost),
                    i.ActivityDescription,
                    i.RecordedByName
                );
            }

            lblMetric1.Text = items.Count.ToString();
            lblMetric2.Text = string.Format("RWF {0:N2}", totalCost);
            lblMetric3.Text = string.Format("{0:N2} Units", totalQty);
        }

        private void GenerateLaborWages(int projId, DateTime from, DateTime to)
        {
            List<LaborWageReportItem> items = _reportController.GetLaborWageSummaryReport(projId, from, to);

            gridReport.Columns.Add("Date", "Work Date");
            gridReport.Columns["Date"].Width = 105;
            gridReport.Columns.Add("Worker", "Worker Name");
            gridReport.Columns.Add("Category", "Trade Category");
            gridReport.Columns.Add("Rate", "Daily Rate (RWF)");
            gridReport.Columns["Rate"].Width = 120;
            gridReport.Columns.Add("Owed", "Owed (RWF)");
            gridReport.Columns["Owed"].Width = 130;
            gridReport.Columns.Add("Paid", "Paid (RWF)");
            gridReport.Columns["Paid"].Width = 130;
            gridReport.Columns.Add("Balance", "Balance Due (RWF)");
            gridReport.Columns["Balance"].Width = 130;
            gridReport.Columns.Add("Status", "Status");
            gridReport.Columns["Status"].Width = 100;
            gridReport.Columns.Add("DisbursedBy", "Disbursed By");

            decimal totalPaid = 0m;
            decimal totalDue = 0m;

            foreach (LaborWageReportItem i in items)
            {
                totalPaid += i.AmountPaid;
                totalDue += i.BalanceDue;

                gridReport.Rows.Add(
                    i.WorkDate.ToString("yyyy-MM-dd"),
                    i.WorkerName,
                    i.WorkerTypeName,
                    string.Format("{0:N2}", i.DailyRate),
                    string.Format("{0:N2}", i.AmountOwed),
                    string.Format("{0:N2}", i.AmountPaid),
                    string.Format("{0:N2}", i.BalanceDue),
                    i.PaymentStatus,
                    i.PaidByName
                );
            }

            lblMetric1.Text = items.Count.ToString();
            lblMetric2.Text = string.Format("RWF {0:N2}", totalPaid);
            lblMetric3.Text = string.Format("RWF {0:N2}", totalDue);
        }

        private void GenerateStockMovements(int projId, DateTime from, DateTime to)
        {
            List<StockMovementReportItem> items = _reportController.GetStockMovementAuditReport(projId, from, to);

            gridReport.Columns.Add("Date", "Movement Date");
            gridReport.Columns["Date"].Width = 105;
            gridReport.Columns.Add("Type", "Type");
            gridReport.Columns["Type"].Width = 90;
            gridReport.Columns.Add("Material", "Material Item");
            gridReport.Columns.Add("Quantity", "Quantity");
            gridReport.Columns["Quantity"].Width = 100;
            gridReport.Columns.Add("UnitPrice", "Unit Price (RWF)");
            gridReport.Columns["UnitPrice"].Width = 130;
            gridReport.Columns.Add("TotalPrice", "Total Cost (RWF)");
            gridReport.Columns["TotalPrice"].Width = 140;
            gridReport.Columns.Add("Description", "Description / Reference");
            gridReport.Columns.Add("RecordedBy", "Recorded By");

            decimal totalIn = 0m;
            decimal totalOut = 0m;

            foreach (StockMovementReportItem i in items)
            {
                if (i.MovementType == "IN") totalIn += i.TotalPrice;
                if (i.MovementType == "OUT") totalOut += i.TotalPrice;

                gridReport.Rows.Add(
                    i.MovementDate.ToString("yyyy-MM-dd"),
                    i.MovementType,
                    i.MaterialName,
                    string.Format("{0:N2} {1}", i.Quantity, i.Unit),
                    string.Format("{0:N2}", i.UnitPrice),
                    string.Format("{0:N2}", i.TotalPrice),
                    i.Description,
                    i.RecordedByName
                );
            }

            lblMetric1.Text = items.Count.ToString();
            lblMetric2.Text = string.Format("Inflows: RWF {0:N2}", totalIn);
            lblMetric3.Text = string.Format("Outflows: RWF {0:N2}", totalOut);
        }

        private void GenerateExecutiveOverview()
        {
            List<ProjectExecutiveSummary> summaries = _reportController.GetExecutiveDashboardSummaries();

            gridReport.Columns.Add("Project", "Construction Project");
            gridReport.Columns.Add("Location", "Site Location");
            gridReport.Columns.Add("Status", "Project Status");
            gridReport.Columns["Status"].Width = 100;
            gridReport.Columns.Add("Progress", "Milestone Progress");
            gridReport.Columns["Progress"].Width = 130;
            gridReport.Columns.Add("Purchases", "Materials Purchased (RWF)");
            gridReport.Columns["Purchases"].Width = 160;
            gridReport.Columns.Add("UsageCost", "Materials Used (RWF)");
            gridReport.Columns["UsageCost"].Width = 150;
            gridReport.Columns.Add("WagesPaid", "Labor Wages Paid (RWF)");
            gridReport.Columns["WagesPaid"].Width = 160;
            gridReport.Columns.Add("WagesDue", "Labor Owed (RWF)");
            gridReport.Columns["WagesDue"].Width = 140;
            gridReport.Columns.Add("TotalCost", "Total Disbursed (RWF)");
            gridReport.Columns["TotalCost"].Width = 160;
            gridReport.Columns.Add("LowStock", "Low Stock");
            gridReport.Columns["LowStock"].Width = 85;

            decimal grandExpenditure = 0m;
            int totalLowStock = 0;

            foreach (ProjectExecutiveSummary s in summaries)
            {
                grandExpenditure += s.TotalProjectExpenditure;
                totalLowStock += s.LowStockAlertsCount;

                gridReport.Rows.Add(
                    s.ProjectName,
                    s.Location,
                    s.Status,
                    string.Format("{0}%", s.CurrentProgress),
                    string.Format("{0:N2}", s.TotalMaterialPurchases),
                    string.Format("{0:N2}", s.TotalMaterialUsageCost),
                    string.Format("{0:N2}", s.TotalLaborWagesPaid),
                    string.Format("{0:N2}", s.OutstandingLaborLiabilities),
                    string.Format("{0:N2}", s.TotalProjectExpenditure),
                    s.LowStockAlertsCount > 0 ? string.Format("⚠️ {0}", s.LowStockAlertsCount) : "0"
                );
            }

            lblMetric1.Text = summaries.Count.ToString();
            lblMetric2.Text = string.Format("RWF {0:N2}", grandExpenditure);
            lblMetric3.Text = string.Format("⚠️ {0} Low Stock Alerts", totalLowStock);
        }

        private void BtnExportCsv_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no report data to export.", "Empty Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                string reportName = cmbReportType.SelectedItem.ToString().Replace(" ", "_").Replace("/", "_");
                sfd.Filter = "CSV Spreadsheet (*.csv)|*.csv|All Files (*.*)|*.*";
                sfd.FileName = string.Format("SiteTrack_{0}_{1:yyyyMMdd_HHmm}.csv", reportName, DateTime.Now);

                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    List<string> headers = new List<string>();
                    foreach (DataGridViewColumn col in gridReport.Columns)
                    {
                        headers.Add(col.HeaderText);
                    }

                    List<List<string>> rows = new List<List<string>>();
                    foreach (DataGridViewRow row in gridReport.Rows)
                    {
                        List<string> cells = new List<string>();
                        for (int i = 0; i < gridReport.Columns.Count; i++)
                        {
                            object val = row.Cells[i].Value;
                            cells.Add(val != null ? val.ToString() : "");
                        }
                        rows.Add(cells);
                    }

                    string csvContent = CsvExporter.FormatCsv(headers, rows);
                    CsvExporter.SaveToFile(sfd.FileName, csvContent);

                    MessageBox.Show(
                        string.Format("Report exported successfully to CSV!\n\nFile: {0}\nRows: {1}",
                            Path.GetFileName(sfd.FileName), rows.Count),
                        "CSV Export Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
        }

        private void BtnPrintPdf_Click(object sender, EventArgs e)
        {
            if (gridReport.Rows.Count == 0)
            {
                MessageBox.Show("There is no report data to print.", "Empty Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string title = cmbReportType.SelectedItem.ToString();
            string project = cmbProjects.SelectedItem != null ? cmbProjects.SelectedItem.ToString() : "All Projects";
            string dateRange = string.Format("{0:yyyy-MM-dd} to {1:yyyy-MM-dd}", dtpFrom.Value, dtpTo.Value);

            List<string> headers = new List<string>();
            foreach (DataGridViewColumn col in gridReport.Columns)
            {
                headers.Add(col.HeaderText);
            }

            List<List<string>> rows = new List<List<string>>();
            foreach (DataGridViewRow row in gridReport.Rows)
            {
                List<string> cells = new List<string>();
                for (int i = 0; i < gridReport.Columns.Count; i++)
                {
                    object val = row.Cells[i].Value;
                    cells.Add(val != null ? val.ToString() : "");
                }
                rows.Add(cells);
            }

            PrintReportHelper helper = new PrintReportHelper(title, project, dateRange, headers, rows);
            helper.ShowPrintPreview(this);
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

