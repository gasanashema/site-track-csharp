using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormUsageDialog : Form
    {
        private readonly StockController _stockController;
        private readonly ProjectController _projectController;
        private readonly int? _preselectedProjectId;

        private ComboBox cmbProjects;
        private ComboBox cmbMaterials;
        private Label lblAvailableStock;
        private TextBox txtQuantity;
        private DateTimePicker dtpUsageDate;
        private TextBox txtActivityDescription;
        private Label lblStockWarning;
        private Label lblTotalCost;
        private Button btnSave;
        private Button btnCancel;
        private ErrorProvider errorProvider;

        private decimal _currentAvailableStock = 0m;
        private decimal _currentUnitPrice = 0m;

        public FormUsageDialog() : this(null, new StockController(), new ProjectController())
        {
        }

        public FormUsageDialog(int? projectId) : this(projectId, new StockController(), new ProjectController())
        {
        }

        public FormUsageDialog(int? projectId, StockController stockController, ProjectController projectController)
        {
            this._preselectedProjectId = projectId;
            this._stockController = stockController ?? new StockController();
            this._projectController = projectController ?? new ProjectController();

            InitializeComponent();
            LoadProjects();
        }

        private void InitializeComponent()
        {
            this.Text = "Record Material Usage — SiteTrack";
            this.Size = new Size(500, 560);
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
            lblTitle.Text = "Record Material Usage (Site Outflow)";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(18, 12);
            pnlHeader.Controls.Add(lblTitle);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Daily site consumption is strictly validated against available stock balance.";
            lblSubtitle.Font = new Font("Segoe UI", 8.5f);
            lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184); // Slate 400
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(20, 38);
            pnlHeader.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlHeader);

            int startY = 85;
            int labelWidth = 140;
            int fieldLeft = 160;
            int fieldWidth = 290;
            int rowHeight = 42;

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
            cmbProjects.SelectedIndexChanged += delegate { LoadMaterialsForProject(); };
            this.Controls.Add(cmbProjects);

            // 2. Material
            startY += rowHeight;
            Label lblMat = new Label();
            lblMat.Text = "Stocked Material:*";
            lblMat.Location = new Point(20, startY);
            lblMat.Size = new Size(labelWidth, 24);
            lblMat.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblMat.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblMat);

            cmbMaterials = new ComboBox();
            cmbMaterials.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMaterials.Location = new Point(fieldLeft, startY - 2);
            cmbMaterials.Size = new Size(fieldWidth, 26);
            cmbMaterials.SelectedIndexChanged += delegate { OnMaterialSelected(); };
            this.Controls.Add(cmbMaterials);

            // Available Stock info badge
            startY += 30;
            lblAvailableStock = new Label();
            lblAvailableStock.Text = "Select a material to see available stock.";
            lblAvailableStock.Location = new Point(fieldLeft, startY);
            lblAvailableStock.Size = new Size(fieldWidth, 20);
            lblAvailableStock.Font = new Font("Segoe UI", 8.5f, FontStyle.Italic);
            lblAvailableStock.ForeColor = Color.FromArgb(100, 116, 139);
            this.Controls.Add(lblAvailableStock);

            // 3. Quantity Used
            startY += 30;
            Label lblQty = new Label();
            lblQty.Text = "Quantity Used:*";
            lblQty.Location = new Point(20, startY);
            lblQty.Size = new Size(labelWidth, 24);
            lblQty.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblQty.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblQty);

            txtQuantity = new TextBox();
            txtQuantity.Location = new Point(fieldLeft, startY - 2);
            txtQuantity.Size = new Size(fieldWidth, 26);
            txtQuantity.TextChanged += delegate { ValidateStockQuantity(); };
            this.Controls.Add(txtQuantity);

            // Stock Depletion Warning / OK message
            startY += 28;
            lblStockWarning = new Label();
            lblStockWarning.Text = "";
            lblStockWarning.Location = new Point(fieldLeft, startY);
            lblStockWarning.Size = new Size(fieldWidth, 22);
            lblStockWarning.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            this.Controls.Add(lblStockWarning);

            // 4. Usage Date
            startY += 28;
            Label lblDate = new Label();
            lblDate.Text = "Usage Date:*";
            lblDate.Location = new Point(20, startY);
            lblDate.Size = new Size(labelWidth, 24);
            lblDate.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblDate.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblDate);

            dtpUsageDate = new DateTimePicker();
            dtpUsageDate.Format = DateTimePickerFormat.Short;
            dtpUsageDate.Value = DateTime.Today;
            dtpUsageDate.Location = new Point(fieldLeft, startY - 2);
            dtpUsageDate.Size = new Size(fieldWidth, 26);
            this.Controls.Add(dtpUsageDate);

            // 5. Activity Description
            startY += rowHeight;
            Label lblDesc = new Label();
            lblDesc.Text = "Site Activity:*";
            lblDesc.Location = new Point(20, startY);
            lblDesc.Size = new Size(labelWidth, 24);
            lblDesc.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblDesc.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblDesc);

            txtActivityDescription = new TextBox();
            txtActivityDescription.Multiline = true;
            txtActivityDescription.Location = new Point(fieldLeft, startY - 2);
            txtActivityDescription.Size = new Size(fieldWidth, 55);
            txtActivityDescription.ScrollBars = ScrollBars.Vertical;
            this.Controls.Add(txtActivityDescription);

            // Estimated Cost
            startY += 62;
            lblTotalCost = new Label();
            lblTotalCost.Text = "Estimated Cost: RWF 0.00";
            lblTotalCost.Location = new Point(fieldLeft, startY);
            lblTotalCost.Size = new Size(fieldWidth, 22);
            lblTotalCost.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblTotalCost.ForeColor = Color.FromArgb(30, 41, 59);
            this.Controls.Add(lblTotalCost);

            // Action Buttons
            startY += 36;
            btnSave = new Button();
            btnSave.Text = "Record Usage (OUT)";
            btnSave.Location = new Point(fieldLeft, startY);
            btnSave.Size = new Size(160, 36);
            btnSave.BackColor = Color.FromArgb(185, 28, 28); // Red 700
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnSave.Cursor = Cursors.Hand;
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(fieldLeft + 170, startY);
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

        private void LoadMaterialsForProject()
        {
            cmbMaterials.Items.Clear();
            lblAvailableStock.Text = "Select a material to see available stock.";
            lblStockWarning.Text = "";
            lblTotalCost.Text = "Estimated Cost: RWF 0.00";
            _currentAvailableStock = 0m;
            _currentUnitPrice = 0m;

            ProjectItem proj = cmbProjects.SelectedItem as ProjectItem;
            if (proj == null) return;

            List<ProjectMaterialStock> stocks = _stockController.GetProjectStock(proj.Id);
            foreach (ProjectMaterialStock s in stocks)
            {
                cmbMaterials.Items.Add(new StockItem(s.MaterialId, s.MaterialName, s.Unit, s.QuantityAvailable, s.AverageUnitPrice));
            }

            if (cmbMaterials.Items.Count > 0)
            {
                cmbMaterials.SelectedIndex = 0;
            }
            else
            {
                lblAvailableStock.Text = "No materials stocked in this project yet.";
                lblAvailableStock.ForeColor = Color.FromArgb(220, 38, 38);
            }
        }

        private void OnMaterialSelected()
        {
            StockItem item = cmbMaterials.SelectedItem as StockItem;
            if (item != null)
            {
                _currentAvailableStock = item.QuantityAvailable;
                _currentUnitPrice = item.AverageUnitPrice;
                lblAvailableStock.Text = string.Format("Available: {0:N2} {1} | Avg Unit Price: RWF {2:N2}",
                    item.QuantityAvailable, item.Unit, item.AverageUnitPrice);
                lblAvailableStock.ForeColor = item.QuantityAvailable > 0 ? Color.FromArgb(22, 101, 52) : Color.FromArgb(220, 38, 38);
            }
            ValidateStockQuantity();
        }

        private void ValidateStockQuantity()
        {
            errorProvider.SetError(txtQuantity, "");
            lblStockWarning.Text = "";

            decimal qty;
            if (!decimal.TryParse(txtQuantity.Text.Trim(), out qty))
            {
                lblTotalCost.Text = "Estimated Cost: RWF 0.00";
                return;
            }

            if (qty <= 0)
            {
                lblStockWarning.ForeColor = Color.FromArgb(220, 38, 38);
                lblStockWarning.Text = "⚠️ Quantity must be greater than zero.";
                return;
            }

            // Check Business Rule 1
            if (qty > _currentAvailableStock)
            {
                lblStockWarning.ForeColor = Color.FromArgb(220, 38, 38);
                lblStockWarning.Text = string.Format("⚠️ Rule 1 Rejection: Exceeds stock ({0:N2} available).", _currentAvailableStock);
            }
            else
            {
                lblStockWarning.ForeColor = Color.FromArgb(22, 101, 52);
                lblStockWarning.Text = string.Format("✓ OK ({0:N2} remaining after usage).", _currentAvailableStock - qty);
            }

            decimal totalCost = qty * _currentUnitPrice;
            lblTotalCost.Text = string.Format("Estimated Cost: RWF {0:N2}", totalCost);
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();

            ProjectItem proj = cmbProjects.SelectedItem as ProjectItem;
            if (proj == null)
            {
                errorProvider.SetError(cmbProjects, "Please select a project.");
                return;
            }

            StockItem stockItem = cmbMaterials.SelectedItem as StockItem;
            if (stockItem == null)
            {
                errorProvider.SetError(cmbMaterials, "Please select a material.");
                return;
            }

            decimal quantity;
            if (!decimal.TryParse(txtQuantity.Text.Trim(), out quantity) || quantity <= 0)
            {
                errorProvider.SetError(txtQuantity, "Enter a valid positive quantity used.");
                txtQuantity.Focus();
                return;
            }

            // Business Rule 1 enforcement check
            if (quantity > stockItem.QuantityAvailable)
            {
                MessageBox.Show(
                    string.Format("Business Rule 1 Violation:\n\nCannot record usage of {0:N2} {1}.\nOnly {2:N2} {1} available in project stock.",
                        quantity, stockItem.Unit, stockItem.QuantityAvailable),
                    "Insufficient Stock",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtQuantity.Focus();
                return;
            }

            string activity = txtActivityDescription.Text.Trim();
            if (string.IsNullOrEmpty(activity))
            {
                errorProvider.SetError(txtActivityDescription, "Site activity description is required (e.g. Concrete slab casting).");
                txtActivityDescription.Focus();
                return;
            }

            Result<int> result = _stockController.RecordUsage(proj.Id, stockItem.MaterialId, quantity, activity, dtpUsageDate.Value);
            if (!result.IsSuccess)
            {
                MessageBox.Show(result.ErrorMessage, "Validation Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(
                string.Format("Material usage successfully recorded!\n\nMaterial: {0}\nQuantity Deducted: {1:N2} {2}\nActivity: {3}\nMovement: OUT logged to audit trail.",
                    stockItem.MaterialName, quantity, stockItem.Unit, activity),
                "Usage Recorded",
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

        private class StockItem
        {
            public int MaterialId { get; private set; }
            public string MaterialName { get; private set; }
            public string Unit { get; private set; }
            public decimal QuantityAvailable { get; private set; }
            public decimal AverageUnitPrice { get; private set; }

            public StockItem(int materialId, string name, string unit, decimal qty, decimal avgPrice)
            {
                this.MaterialId = materialId;
                this.MaterialName = name;
                this.Unit = unit;
                this.QuantityAvailable = qty;
                this.AverageUnitPrice = avgPrice;
            }

            public override string ToString()
            {
                return string.Format("{0} ({1}) — {2:N2} in stock", MaterialName, Unit, QuantityAvailable);
            }
        }
    }
}
