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
    public class UcMaterialUsage : UserControl
    {
        private readonly StockController _stockController;
        private readonly ProjectController _projectController;

        private ComboBox cmbProjects;
        private DateTimePicker dtpFrom;
        private DateTimePicker dtpTo;
        private TextBox txtSearch;
        private Button btnRecordUsage;
        private Button btnRefresh;
        private DataGridView gridUsage;

        private Label lblMetricCount;
        private Label lblMetricCost;
        private Label lblMetricMaterials;

        private List<MaterialUsage> _loadedUsages = new List<MaterialUsage>();

        public UcMaterialUsage() : this(new StockController(), new ProjectController())
        {
        }

        public UcMaterialUsage(StockController stockController, ProjectController projectController)
        {
            this._stockController = stockController ?? new StockController();
            this._projectController = projectController ?? new ProjectController();

            InitializeComponent();
            LoadProjects();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Segoe UI", 9.5f);
            this.Padding = new Padding(16);

            // 1. Top Filter Bar
            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 60;
            pnlTop.BackColor = Color.FromArgb(241, 245, 249);
            pnlTop.Padding = new Padding(12);

            Label lblProj = new Label();
            lblProj.Text = "Project:";
            lblProj.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblProj.ForeColor = Color.FromArgb(30, 41, 59);
            lblProj.Location = new Point(12, 18);
            lblProj.AutoSize = true;
            pnlTop.Controls.Add(lblProj);

            cmbProjects = new ComboBox();
            cmbProjects.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProjects.Location = new Point(70, 14);
            cmbProjects.Size = new Size(260, 26);
            cmbProjects.SelectedIndexChanged += delegate { LoadUsages(); };
            pnlTop.Controls.Add(cmbProjects);

            Label lblFrom = new Label();
            lblFrom.Text = "From:";
            lblFrom.Location = new Point(345, 18);
            lblFrom.AutoSize = true;
            pnlTop.Controls.Add(lblFrom);

            dtpFrom = new DateTimePicker();
            dtpFrom.Format = DateTimePickerFormat.Short;
            dtpFrom.Value = DateTime.Today.AddDays(-30);
            dtpFrom.Location = new Point(390, 14);
            dtpFrom.Size = new Size(110, 26);
            dtpFrom.ValueChanged += delegate { LoadUsages(); };
            pnlTop.Controls.Add(dtpFrom);

            Label lblTo = new Label();
            lblTo.Text = "To:";
            lblTo.Location = new Point(510, 18);
            lblTo.AutoSize = true;
            pnlTop.Controls.Add(lblTo);

            dtpTo = new DateTimePicker();
            dtpTo.Format = DateTimePickerFormat.Short;
            dtpTo.Value = DateTime.Today.AddDays(1);
            dtpTo.Location = new Point(535, 14);
            dtpTo.Size = new Size(110, 26);
            dtpTo.ValueChanged += delegate { LoadUsages(); };
            pnlTop.Controls.Add(dtpTo);

            btnRecordUsage = new Button();
            btnRecordUsage.Text = "+ Record Daily Usage (OUT)";
            btnRecordUsage.Location = new Point(660, 10);
            btnRecordUsage.Size = new Size(190, 36);
            btnRecordUsage.BackColor = Color.FromArgb(185, 28, 28); // Red 700
            btnRecordUsage.ForeColor = Color.White;
            btnRecordUsage.FlatStyle = FlatStyle.Flat;
            btnRecordUsage.FlatAppearance.BorderSize = 0;
            btnRecordUsage.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnRecordUsage.Cursor = Cursors.Hand;
            btnRecordUsage.Click += BtnRecordUsage_Click;
            pnlTop.Controls.Add(btnRecordUsage);

            btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Location = new Point(860, 10);
            btnRefresh.Size = new Size(80, 36);
            btnRefresh.BackColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 1;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.Click += delegate { LoadUsages(); };
            pnlTop.Controls.Add(btnRefresh);

            this.Controls.Add(pnlTop);

            // 2. Metrics Bar
            Panel pnlMetrics = new Panel();
            pnlMetrics.Dock = DockStyle.Top;
            pnlMetrics.Height = 70;
            pnlMetrics.BackColor = Color.White;
            pnlMetrics.Padding = new Padding(0, 10, 0, 10);

            Panel card1 = CreateMetricCard("Usage Entries", out lblMetricCount, 0, Color.FromArgb(235, 86, 18));
            Panel card2 = CreateMetricCard("Total Consumption Value", out lblMetricCost, 210, Color.FromArgb(185, 28, 28), 240);
            Panel card3 = CreateMetricCard("Distinct Materials", out lblMetricMaterials, 460, Color.FromArgb(15, 23, 42));

            pnlMetrics.Controls.Add(card1);
            pnlMetrics.Controls.Add(card2);
            pnlMetrics.Controls.Add(card3);
            this.Controls.Add(pnlMetrics);

            // 3. Search & Grid
            Panel pnlSearch = new Panel();
            pnlSearch.Dock = DockStyle.Top;
            pnlSearch.Height = 44;
            pnlSearch.Padding = new Padding(0, 4, 0, 8);

            Label lblSearch = new Label();
            lblSearch.Text = "Search Activity / Material:";
            lblSearch.Location = new Point(0, 8);
            lblSearch.AutoSize = true;
            pnlSearch.Controls.Add(lblSearch);

            txtSearch = new TextBox();
            txtSearch.Location = new Point(170, 5);
            txtSearch.Size = new Size(300, 26);
            txtSearch.TextChanged += delegate { FilterUsages(); };
            pnlSearch.Controls.Add(txtSearch);
            this.Controls.Add(pnlSearch);

            // 4. Data Grid
            gridUsage = new DataGridView();
            gridUsage.Dock = DockStyle.Fill;
            gridUsage.BackgroundColor = Color.White;
            gridUsage.BorderStyle = BorderStyle.None;
            gridUsage.RowHeadersVisible = false;
            gridUsage.AllowUserToAddRows = false;
            gridUsage.AllowUserToDeleteRows = false;
            gridUsage.ReadOnly = true;
            gridUsage.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            gridUsage.MultiSelect = false;
            gridUsage.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            gridUsage.RowTemplate.Height = 36;
            gridUsage.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 250, 252);
            gridUsage.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(241, 245, 249);
            gridUsage.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            gridUsage.ColumnHeadersHeight = 40;
            gridUsage.EnableHeadersVisualStyles = false;
            gridUsage.DefaultCellStyle.SelectionBackColor = Color.FromArgb(254, 237, 213);
            gridUsage.DefaultCellStyle.SelectionForeColor = Color.FromArgb(15, 23, 42);

            gridUsage.Columns.Add("Date", "Usage Date");
            gridUsage.Columns["Date"].Width = 105;
            gridUsage.Columns.Add("Material", "Material Item");
            gridUsage.Columns["Material"].Width = 180;
            gridUsage.Columns.Add("Quantity", "Quantity Used");
            gridUsage.Columns["Quantity"].Width = 120;
            gridUsage.Columns.Add("UnitPrice", "Avg Unit Cost (RWF)");
            gridUsage.Columns["UnitPrice"].Width = 140;
            gridUsage.Columns.Add("TotalCost", "Total Value (RWF)");
            gridUsage.Columns["TotalCost"].Width = 150;
            gridUsage.Columns.Add("Activity", "Site Activity / Task");
            gridUsage.Columns.Add("RecordedBy", "Recorded By");
            gridUsage.Columns["RecordedBy"].Width = 120;

            this.Controls.Add(gridUsage);

            pnlSearch.BringToFront();
            pnlMetrics.BringToFront();
            pnlTop.BringToFront();
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
            else
            {
                btnRecordUsage.Enabled = false;
            }
        }

        private int? GetSelectedProjectId()
        {
            ProjectItem item = cmbProjects.SelectedItem as ProjectItem;
            return item != null ? (int?)item.Id : null;
        }

        public void LoadUsages()
        {
            int? projId = GetSelectedProjectId();
            if (!projId.HasValue) return;

            DateTime from = dtpFrom.Value.Date;
            DateTime to = dtpTo.Value.Date.AddDays(1).AddSeconds(-1);

            _loadedUsages = _stockController.GetUsages(projId.Value, from, to);
            FilterUsages();
        }

        private void FilterUsages()
        {
            gridUsage.Rows.Clear();
            string search = txtSearch != null ? txtSearch.Text.Trim().ToLowerInvariant() : "";

            decimal totalCost = 0m;
            HashSet<int> distinctMats = new HashSet<int>();
            int count = 0;

            foreach (MaterialUsage u in _loadedUsages)
            {
                bool matches = string.IsNullOrEmpty(search) ||
                               (u.MaterialName != null && u.MaterialName.ToLowerInvariant().Contains(search)) ||
                               (u.ActivityDescription != null && u.ActivityDescription.ToLowerInvariant().Contains(search));

                if (!matches) continue;

                count++;
                totalCost += u.TotalCost;
                distinctMats.Add(u.MaterialId);

                gridUsage.Rows.Add(
                    u.UsageDate.ToString("yyyy-MM-dd"),
                    string.Format("{0} ({1})", u.MaterialName, u.Unit),
                    string.Format("{0:N2} {1}", u.QuantityUsed, u.Unit),
                    string.Format("{0:N2}", u.UnitPrice),
                    string.Format("{0:N2}", u.TotalCost),
                    u.ActivityDescription,
                    u.RecordedByName ?? "Site Mgr"
                );
            }

            lblMetricCount.Text = count.ToString();
            lblMetricCost.Text = string.Format("RWF {0:N2}", totalCost);
            lblMetricMaterials.Text = distinctMats.Count.ToString();
        }

        private void BtnRecordUsage_Click(object sender, EventArgs e)
        {
            int? projId = GetSelectedProjectId();
            using (FormUsageDialog dlg = new FormUsageDialog(projId, _stockController, _projectController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadUsages();
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
    }
}
