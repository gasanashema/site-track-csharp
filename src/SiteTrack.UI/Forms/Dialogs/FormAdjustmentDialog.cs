using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormAdjustmentDialog : Form
    {
        private readonly StockController _stockController;
        private readonly ProjectController _projectController;
        private readonly int? _preselectedProjectId;
        private readonly int? _preselectedMaterialId;

        private ComboBox cmbProjects;
        private ComboBox cmbMaterials;
        private Label lblCurrentStock;
        private TextBox txtNewQuantity;
        private Label lblDelta;
        private TextBox txtReason;
        private Button btnSave;
        private Button btnCancel;
        private ErrorProvider errorProvider;

        private decimal _currentQuantity = 0m;

        public FormAdjustmentDialog() : this(null, null, new StockController(), new ProjectController())
        {
        }

        public FormAdjustmentDialog(int? projectId, int? materialId) : this(projectId, materialId, new StockController(), new ProjectController())
        {
        }

        public FormAdjustmentDialog(int? projectId, int? materialId, StockController stockController, ProjectController projectController)
        {
            this._preselectedProjectId = projectId;
            this._preselectedMaterialId = materialId;
            this._stockController = stockController ?? new StockController();
            this._projectController = projectController ?? new ProjectController();

            InitializeComponent();
            LoadProjects();
        }

        private void InitializeComponent()
        {
            this.Text = "Stock Inventory Adjustment — SiteTrack";
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
            pnlHeader.BackColor = Color.FromArgb(30, 41, 59); // Slate 800
            pnlHeader.Padding = new Padding(20, 12, 20, 12);

            Label lblTitle = new Label();
            lblTitle.Text = "Physical Inventory Adjustment (Audit)";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.White;
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(18, 12);
            pnlHeader.Controls.Add(lblTitle);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = "Admin tool: reconcile physical inventory counts with immutable audit trail.";
            lblSubtitle.Font = new Font("Segoe UI", 8.5f);
            lblSubtitle.ForeColor = Color.FromArgb(148, 163, 184); // Slate 400
            lblSubtitle.AutoSize = true;
            lblSubtitle.Location = new Point(20, 38);
            pnlHeader.Controls.Add(lblSubtitle);
            this.Controls.Add(pnlHeader);

            int startY = 90;
            int labelWidth = 140;
            int fieldLeft = 160;
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
            cmbProjects.SelectedIndexChanged += delegate { LoadMaterialsForProject(); };
            this.Controls.Add(cmbProjects);

            // 2. Material
            startY += rowHeight;
            Label lblMat = new Label();
            lblMat.Text = "Material Item:*";
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

            // Current Stock label
            startY += 30;
            lblCurrentStock = new Label();
            lblCurrentStock.Text = "Current System Stock: 0.00";
            lblCurrentStock.Location = new Point(fieldLeft, startY);
            lblCurrentStock.Size = new Size(fieldWidth, 20);
            lblCurrentStock.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblCurrentStock.ForeColor = Color.FromArgb(71, 85, 105);
            this.Controls.Add(lblCurrentStock);

            // 3. New Physical Count
            startY += 30;
            Label lblNewQty = new Label();
            lblNewQty.Text = "Physical Count:*";
            lblNewQty.Location = new Point(20, startY);
            lblNewQty.Size = new Size(labelWidth, 24);
            lblNewQty.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblNewQty.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblNewQty);

            txtNewQuantity = new TextBox();
            txtNewQuantity.Location = new Point(fieldLeft, startY - 2);
            txtNewQuantity.Size = new Size(fieldWidth, 26);
            txtNewQuantity.TextChanged += delegate { CalculateDelta(); };
            this.Controls.Add(txtNewQuantity);

            // Calculated Delta label
            startY += 28;
            lblDelta = new Label();
            lblDelta.Text = "Adjustment Delta: 0.00";
            lblDelta.Location = new Point(fieldLeft, startY);
            lblDelta.Size = new Size(fieldWidth, 20);
            lblDelta.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblDelta.ForeColor = Color.FromArgb(100, 116, 139);
            this.Controls.Add(lblDelta);

            // 4. Reason / Audit Note
            startY += 28;
            Label lblReason = new Label();
            lblReason.Text = "Reason / Note:*";
            lblReason.Location = new Point(20, startY);
            lblReason.Size = new Size(labelWidth, 24);
            lblReason.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            lblReason.ForeColor = Color.FromArgb(51, 65, 85);
            this.Controls.Add(lblReason);

            txtReason = new TextBox();
            txtReason.Multiline = true;
            txtReason.Location = new Point(fieldLeft, startY - 2);
            txtReason.Size = new Size(fieldWidth, 60);
            txtReason.ScrollBars = ScrollBars.Vertical;
            this.Controls.Add(txtReason);

            // Action Buttons
            startY += 75;
            btnSave = new Button();
            btnSave.Text = "Apply Adjustment";
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

        private void LoadMaterialsForProject()
        {
            cmbMaterials.Items.Clear();
            lblCurrentStock.Text = "Current System Stock: 0.00";
            lblDelta.Text = "Adjustment Delta: 0.00";
            _currentQuantity = 0m;

            ProjectItem proj = cmbProjects.SelectedItem as ProjectItem;
            if (proj == null) return;

            List<ProjectMaterialStock> stocks = _stockController.GetProjectStock(proj.Id);
            int selectedIdx = 0;
            for (int i = 0; i < stocks.Count; i++)
            {
                cmbMaterials.Items.Add(new StockItem(stocks[i].MaterialId, stocks[i].MaterialName, stocks[i].Unit, stocks[i].QuantityAvailable));
                if (_preselectedMaterialId.HasValue && stocks[i].MaterialId == _preselectedMaterialId.Value)
                {
                    selectedIdx = i;
                }
            }

            if (cmbMaterials.Items.Count > 0)
            {
                cmbMaterials.SelectedIndex = selectedIdx;
            }
        }

        private void OnMaterialSelected()
        {
            StockItem item = cmbMaterials.SelectedItem as StockItem;
            if (item != null)
            {
                _currentQuantity = item.QuantityAvailable;
                lblCurrentStock.Text = string.Format("Current System Stock: {0:N2} {1}", item.QuantityAvailable, item.Unit);
                txtNewQuantity.Text = string.Format("{0:N2}", item.QuantityAvailable);
            }
            CalculateDelta();
        }

        private void CalculateDelta()
        {
            decimal newQty;
            if (decimal.TryParse(txtNewQuantity.Text.Trim(), out newQty))
            {
                decimal diff = newQty - _currentQuantity;
                if (diff > 0)
                {
                    lblDelta.Text = string.Format("Adjustment Delta: +{0:N2} (Surplus / Inflow)", diff);
                    lblDelta.ForeColor = Color.FromArgb(22, 101, 52); // Green
                }
                else if (diff < 0)
                {
                    lblDelta.Text = string.Format("Adjustment Delta: {0:N2} (Deficit / Shrinkage)", diff);
                    lblDelta.ForeColor = Color.FromArgb(220, 38, 38); // Red
                }
                else
                {
                    lblDelta.Text = "Adjustment Delta: 0.00 (No change)";
                    lblDelta.ForeColor = Color.FromArgb(100, 116, 139);
                }
            }
            else
            {
                lblDelta.Text = "Adjustment Delta: Invalid number";
                lblDelta.ForeColor = Color.FromArgb(220, 38, 38);
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

            StockItem stock = cmbMaterials.SelectedItem as StockItem;
            if (stock == null)
            {
                errorProvider.SetError(cmbMaterials, "Select a stocked material.");
                return;
            }

            decimal newQty;
            if (!decimal.TryParse(txtNewQuantity.Text.Trim(), out newQty) || newQty < 0)
            {
                errorProvider.SetError(txtNewQuantity, "Enter a valid non-negative physical stock count.");
                txtNewQuantity.Focus();
                return;
            }

            string reason = txtReason.Text.Trim();
            if (string.IsNullOrEmpty(reason))
            {
                errorProvider.SetError(txtReason, "Reason for stock adjustment is required (e.g. Physical inventory count).");
                txtReason.Focus();
                return;
            }

            Result res = _stockController.RecordStockAdjustment(proj.Id, stock.MaterialId, newQty, reason);
            if (!res.IsSuccess)
            {
                MessageBox.Show(res.ErrorMessage, "Adjustment Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(
                string.Format("Stock adjustment recorded successfully!\n\nMaterial: {0}\nPrevious Stock: {1:N2}\nAdjusted Stock: {2:N2}\nMovement: ADJUSTMENT logged in audit trail.",
                    stock.MaterialName, _currentQuantity, newQty),
                "Adjustment Applied",
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

            public StockItem(int materialId, string name, string unit, decimal qty)
            {
                this.MaterialId = materialId;
                this.MaterialName = name;
                this.Unit = unit;
                this.QuantityAvailable = qty;
            }

            public override string ToString()
            {
                return string.Format("{0} ({1}) — {2:N2} in stock", MaterialName, Unit, QuantityAvailable);
            }
        }
    }
}
