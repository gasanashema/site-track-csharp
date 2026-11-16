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
    public class UcStock : UserControl
    {
        private readonly StockController _stockController;
        private readonly ProjectController _projectController;

        private ComboBox cmbProjects;
        private TabControl tabControl;
        private TabPage tabBalances;
        private TabPage tabPurchases;
        private TabPage tabUsages;
        private TabPage tabMovements;

        // Balances tab
        private DataGridView gridStock;
        private Button btnRecordPurchase;
        private Button btnRecordUsage;
        private Button btnAdjustStock;
        private Button btnRefresh;
        private Label lblMetricItems;
        private Label lblMetricLowStock;
        private Label lblMetricValuation;

        // Purchases tab
        private DataGridView gridPurchases;

        // Usages tab
        private DataGridView gridUsages;

        // Movements tab
        private DataGridView gridMovements;

        public UcStock() : this(new StockController(), new ProjectController())
        {
        }

        public UcStock(StockController stockController, ProjectController projectController)
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

            // 1. Project Selector Top Bar
            Panel pnlTop = new Panel();
            pnlTop.Dock = DockStyle.Top;
            pnlTop.Height = 55;
            pnlTop.BackColor = Color.FromArgb(241, 245, 249);
            pnlTop.Padding = new Padding(12);

            Label lblSelectProj = new Label();
            lblSelectProj.Text = "Active Project:";
            lblSelectProj.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblSelectProj.ForeColor = Color.FromArgb(30, 41, 59);
            lblSelectProj.Location = new Point(12, 16);
            lblSelectProj.AutoSize = true;
            pnlTop.Controls.Add(lblSelectProj);

            cmbProjects = new ComboBox();
            cmbProjects.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProjects.Location = new Point(115, 12);
            cmbProjects.Size = new Size(320, 26);
            cmbProjects.SelectedIndexChanged += delegate { LoadProjectData(); };
            pnlTop.Controls.Add(cmbProjects);

            btnRecordPurchase = new Button();
            btnRecordPurchase.Text = "+ Purchase (IN)";
            btnRecordPurchase.Location = new Point(450, 8);
            btnRecordPurchase.Size = new Size(130, 36);
            btnRecordPurchase.BackColor = Color.FromArgb(22, 101, 52); // Green 800
            btnRecordPurchase.ForeColor = Color.White;
            btnRecordPurchase.FlatStyle = FlatStyle.Flat;
            btnRecordPurchase.FlatAppearance.BorderSize = 0;
            btnRecordPurchase.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnRecordPurchase.Cursor = Cursors.Hand;
            btnRecordPurchase.Click += BtnRecordPurchase_Click;
            pnlTop.Controls.Add(btnRecordPurchase);

            btnRecordUsage = new Button();
            btnRecordUsage.Text = "- Usage (OUT)";
            btnRecordUsage.Location = new Point(590, 8);
            btnRecordUsage.Size = new Size(125, 36);
            btnRecordUsage.BackColor = Color.FromArgb(185, 28, 28); // Red 700
            btnRecordUsage.ForeColor = Color.White;
            btnRecordUsage.FlatStyle = FlatStyle.Flat;
            btnRecordUsage.FlatAppearance.BorderSize = 0;
            btnRecordUsage.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnRecordUsage.Cursor = Cursors.Hand;
            btnRecordUsage.Click += BtnRecordUsage_Click;
            pnlTop.Controls.Add(btnRecordUsage);

            btnAdjustStock = new Button();
            btnAdjustStock.Text = "Adjust Stock";
            btnAdjustStock.Location = new Point(725, 8);
            btnAdjustStock.Size = new Size(120, 36);
            btnAdjustStock.BackColor = Color.FromArgb(30, 41, 59); // Slate 800
            btnAdjustStock.ForeColor = Color.White;
            btnAdjustStock.FlatStyle = FlatStyle.Flat;
            btnAdjustStock.FlatAppearance.BorderSize = 0;
            btnAdjustStock.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnAdjustStock.Cursor = Cursors.Hand;
            btnAdjustStock.Click += BtnAdjustStock_Click;
            pnlTop.Controls.Add(btnAdjustStock);

            btnRefresh = new Button();
            btnRefresh.Text = "Refresh";
            btnRefresh.Location = new Point(855, 8);
            btnRefresh.Size = new Size(80, 36);
            btnRefresh.BackColor = Color.White;
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.FlatAppearance.BorderSize = 1;
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefresh.Click += delegate { LoadProjectData(); };
            pnlTop.Controls.Add(btnRefresh);

            this.Controls.Add(pnlTop);

            // 2. Metrics Bar
            Panel pnlMetrics = new Panel();
            pnlMetrics.Dock = DockStyle.Top;
            pnlMetrics.Height = 70;
            pnlMetrics.BackColor = Color.White;
            pnlMetrics.Padding = new Padding(0, 10, 0, 10);

            Panel card1 = CreateMetricCard("Items Stocked", out lblMetricItems, 0, Color.FromArgb(235, 86, 18));
            Panel card2 = CreateMetricCard("Low Stock Alerts", out lblMetricLowStock, 210, Color.FromArgb(220, 38, 38));
            Panel card3 = CreateMetricCard("Total Stock Valuation", out lblMetricValuation, 420, Color.FromArgb(15, 23, 42), 260);

            pnlMetrics.Controls.Add(card1);
            pnlMetrics.Controls.Add(card2);
            pnlMetrics.Controls.Add(card3);
            this.Controls.Add(pnlMetrics);

            // 3. Tab Control
            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // --- Tab 1: Current Stock Balances ---
            tabBalances = new TabPage("Current Stock Balances");
            tabBalances.Padding = new Padding(12);
            tabBalances.BackColor = Color.White;

            gridStock = CreateStyledGrid();
            gridStock.Columns.Add("MaterialName", "Material Name");
            gridStock.Columns.Add("CategoryName", "Category");
            gridStock.Columns.Add("Unit", "Unit");
            gridStock.Columns["Unit"].Width = 70;
            gridStock.Columns.Add("QuantityAvailable", "Available Qty");
            gridStock.Columns["QuantityAvailable"].Width = 110;
            gridStock.Columns.Add("MinimumQuantity", "Min Qty Alert");
            gridStock.Columns["MinimumQuantity"].Width = 110;
            gridStock.Columns.Add("AverageUnitPrice", "Avg Unit Price (RWF)");
            gridStock.Columns["AverageUnitPrice"].Width = 150;
            gridStock.Columns.Add("TotalStockValue", "Total Value (RWF)");
            gridStock.Columns["TotalStockValue"].Width = 150;
            gridStock.Columns.Add("Status", "Stock Status");
            gridStock.Columns["Status"].Width = 120;

            gridStock.CellFormatting += GridStock_CellFormatting;
            tabBalances.Controls.Add(gridStock);
            tabControl.TabPages.Add(tabBalances);

            // --- Tab 2: Purchases History ---
            tabPurchases = new TabPage("Purchases (Inflow Ledger)");
            tabPurchases.Padding = new Padding(12);
            tabPurchases.BackColor = Color.White;

            gridPurchases = CreateStyledGrid();
            gridPurchases.Columns.Add("Date", "Purchase Date");
            gridPurchases.Columns["Date"].Width = 100;
            gridPurchases.Columns.Add("MaterialName", "Material Item");
            gridPurchases.Columns.Add("Supplier", "Supplier / Vendor");
            gridPurchases.Columns.Add("Quantity", "Quantity");
            gridPurchases.Columns["Quantity"].Width = 90;
            gridPurchases.Columns.Add("UnitPrice", "Unit Price (RWF)");
            gridPurchases.Columns["UnitPrice"].Width = 130;
            gridPurchases.Columns.Add("TotalPrice", "Total Cost (RWF)");
            gridPurchases.Columns["TotalPrice"].Width = 140;
            gridPurchases.Columns.Add("RecordedBy", "Recorded By");

            tabPurchases.Controls.Add(gridPurchases);
            tabControl.TabPages.Add(tabPurchases);

            // --- Tab 3: Daily Material Usage ---
            tabUsages = new TabPage("Daily Material Usage (Outflow Ledger)");
            tabUsages.Padding = new Padding(12);
            tabUsages.BackColor = Color.White;

            gridUsages = CreateStyledGrid();
            gridUsages.Columns.Add("Date", "Usage Date");
            gridUsages.Columns["Date"].Width = 100;
            gridUsages.Columns.Add("MaterialName", "Material Item");
            gridUsages.Columns.Add("Quantity", "Quantity Used");
            gridUsages.Columns["Quantity"].Width = 110;
            gridUsages.Columns.Add("UnitPrice", "Unit Cost (RWF)");
            gridUsages.Columns["UnitPrice"].Width = 130;
            gridUsages.Columns.Add("TotalCost", "Total Value (RWF)");
            gridUsages.Columns["TotalCost"].Width = 140;
            gridUsages.Columns.Add("Activity", "Site Activity / Task");
            gridUsages.Columns.Add("RecordedBy", "Recorded By");

            tabUsages.Controls.Add(gridUsages);
            tabControl.TabPages.Add(tabUsages);

            // --- Tab 4: Stock Movements Audit Trail ---
            tabMovements = new TabPage("Stock Movement Audit Trail (Read-Only)");
            tabMovements.Padding = new Padding(12);
            tabMovements.BackColor = Color.White;

            gridMovements = CreateStyledGrid();
            gridMovements.Columns.Add("Type", "Type");
            gridMovements.Columns["Type"].Width = 90;
            gridMovements.Columns.Add("Date", "Movement Date");
            gridMovements.Columns["Date"].Width = 105;
            gridMovements.Columns.Add("Material", "Material Item");
            gridMovements.Columns.Add("Quantity", "Quantity");
            gridMovements.Columns["Quantity"].Width = 95;
            gridMovements.Columns.Add("UnitPrice", "Unit Price (RWF)");
            gridMovements.Columns["UnitPrice"].Width = 130;
            gridMovements.Columns.Add("TotalPrice", "Total Price (RWF)");
            gridMovements.Columns["TotalPrice"].Width = 140;
            gridMovements.Columns.Add("Description", "Description / Reference");
            gridMovements.Columns.Add("RecordedBy", "Recorded By");

            gridMovements.CellFormatting += GridMovements_CellFormatting;
            tabMovements.Controls.Add(gridMovements);
            tabControl.TabPages.Add(tabMovements);

            this.Controls.Add(tabControl);

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

        private void GridStock_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = gridStock.Rows[e.RowIndex];
                object valStatus = row.Cells["Status"].Value;
                if (valStatus != null && valStatus.ToString().Contains("LOW STOCK"))
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 243, 199); // Amber 100
                    row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(245, 158, 11);
                    if (gridStock.Columns[e.ColumnIndex].Name == "Status")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(180, 83, 9); // Amber 700
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                }
            }
        }

        private void GridMovements_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && gridMovements.Columns[e.ColumnIndex].Name == "Type")
            {
                object val = e.Value;
                if (val != null)
                {
                    string str = val.ToString();
                    if (str == "IN")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(22, 101, 52); // Green
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                    else if (str == "OUT")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(185, 28, 28); // Red
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                    else if (str == "ADJUSTMENT")
                    {
                        e.CellStyle.ForeColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
                        e.CellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
                    }
                }
            }
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
                btnRecordPurchase.Enabled = false;
                btnRecordUsage.Enabled = false;
                btnAdjustStock.Enabled = false;
            }

            // Adjust Stock is reserved for Administrators
            if (!BaseController.IsAdmin)
            {
                btnAdjustStock.Visible = false;
            }
        }

        private int? GetSelectedProjectId()
        {
            ProjectItem item = cmbProjects.SelectedItem as ProjectItem;
            return item != null ? (int?)item.Id : null;
        }

        public void LoadProjectData()
        {
            int? projId = GetSelectedProjectId();
            if (!projId.HasValue) return;

            // 1. Load Stock Balances
            List<ProjectMaterialStock> stocks = _stockController.GetProjectStock(projId.Value);
            gridStock.Rows.Clear();

            int lowStockCount = 0;
            decimal totalValuation = 0m;

            foreach (ProjectMaterialStock s in stocks)
            {
                if (s.IsLowStock) lowStockCount++;
                totalValuation += s.TotalStockValue;

                string statusText = s.IsLowStock ? "⚠️ LOW STOCK" : "✓ In Stock";
                gridStock.Rows.Add(
                    s.MaterialName,
                    s.CategoryName,
                    s.Unit,
                    string.Format("{0:N2}", s.QuantityAvailable),
                    string.Format("{0:N2}", s.MinimumQuantity),
                    string.Format("{0:N2}", s.AverageUnitPrice),
                    string.Format("{0:N2}", s.TotalStockValue),
                    statusText
                );
            }

            lblMetricItems.Text = stocks.Count.ToString();
            lblMetricLowStock.Text = lowStockCount.ToString();
            lblMetricValuation.Text = string.Format("RWF {0:N2}", totalValuation);

            // 2. Load Purchases
            List<MaterialPurchase> purchases = _stockController.GetPurchases(projId.Value);
            gridPurchases.Rows.Clear();
            foreach (MaterialPurchase p in purchases)
            {
                gridPurchases.Rows.Add(
                    p.PurchaseDate.ToString("yyyy-MM-dd"),
                    string.Format("{0} ({1})", p.MaterialName, p.Unit),
                    p.SupplierName,
                    string.Format("{0:N2}", p.Quantity),
                    string.Format("{0:N2}", p.UnitPrice),
                    string.Format("{0:N2}", p.TotalPrice),
                    p.RecordedByName ?? "Admin"
                );
            }

            // 3. Load Usages
            List<MaterialUsage> usages = _stockController.GetUsages(projId.Value);
            gridUsages.Rows.Clear();
            foreach (MaterialUsage u in usages)
            {
                gridUsages.Rows.Add(
                    u.UsageDate.ToString("yyyy-MM-dd"),
                    string.Format("{0} ({1})", u.MaterialName, u.Unit),
                    string.Format("{0:N2} {1}", u.QuantityUsed, u.Unit),
                    string.Format("{0:N2}", u.UnitPrice),
                    string.Format("{0:N2}", u.TotalCost),
                    u.ActivityDescription,
                    u.RecordedByName ?? "Site Mgr"
                );
            }

            // 4. Load Movements
            List<MaterialStockMovement> movements = _stockController.GetMovements(projId.Value);
            gridMovements.Rows.Clear();
            foreach (MaterialStockMovement m in movements)
            {
                gridMovements.Rows.Add(
                    m.MovementType,
                    m.MovementDate.ToString("yyyy-MM-dd"),
                    m.MaterialName,
                    string.Format("{0:N2}", m.Quantity),
                    string.Format("{0:N2}", m.UnitPrice),
                    string.Format("{0:N2}", m.TotalPrice),
                    m.Description,
                    m.RecordedByName ?? "System"
                );
            }
        }

        private void BtnRecordPurchase_Click(object sender, EventArgs e)
        {
            int? projId = GetSelectedProjectId();
            using (FormPurchaseDialog dlg = new FormPurchaseDialog(projId, _stockController, _projectController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadProjectData();
                }
            }
        }

        private void BtnRecordUsage_Click(object sender, EventArgs e)
        {
            int? projId = GetSelectedProjectId();
            using (FormUsageDialog dlg = new FormUsageDialog(projId, _stockController, _projectController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadProjectData();
                }
            }
        }

        private void BtnAdjustStock_Click(object sender, EventArgs e)
        {
            int? projId = GetSelectedProjectId();
            using (FormAdjustmentDialog dlg = new FormAdjustmentDialog(projId, null, _stockController, _projectController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadProjectData();
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
