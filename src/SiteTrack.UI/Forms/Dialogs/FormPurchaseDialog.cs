using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormPurchaseDialog : Form
    {
        private readonly StockController _stockController;
        private readonly ProjectController _projectController;
        private readonly MaterialController _materialController;

        private readonly int? _preselectedProjectId;

        private ComboBox cmbProject;
        private ComboBox cmbMaterial;
        private NumericUpDown numQuantity;
        private NumericUpDown numUnitPrice;
        private Label lblUnitDisplay;
        private Label lblTotalCalculation;
        private TextBox txtSupplier;
        private DateTimePicker dtpPurchaseDate;
        private Button btnRecord;
        private Button btnCancel;
        private Label lblError;

        public FormPurchaseDialog(int? projectId = null, StockController stockController = null, ProjectController projectController = null, MaterialController materialController = null)
        {
            this._preselectedProjectId = projectId;
            this._stockController = stockController ?? new StockController();
            this._projectController = projectController ?? new ProjectController();
            this._materialController = materialController ?? new MaterialController();
            InitializeComponent();
            LoadProjects();
            LoadMaterials();
        }

        private void InitializeComponent()
        {
            this.Text = "SiteTrack — Record Material Purchase (IN Movement)";
            this.Size = new Size(560, 620);
            this.MinimumSize = new Size(560, 620);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            int y = 20;

            Label lblTitle = new Label();
            lblTitle.Text = "Record Material Purchase (Stock Inflow)";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, y);
            lblTitle.AutoSize = true;
            this.Controls.Add(lblTitle);
            y += 40;

            // Project
            AddLabel("Target Construction Project *", 24, y);
            y += 20;
            cmbProject = new ComboBox();
            cmbProject.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbProject.Location = new Point(24, y);
            cmbProject.Size = new Size(490, 26);
            this.Controls.Add(cmbProject);
            y += 35;

            // Material
            AddLabel("Material Item *", 24, y);
            y += 20;
            cmbMaterial = new ComboBox();
            cmbMaterial.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMaterial.Location = new Point(24, y);
            cmbMaterial.Size = new Size(490, 26);
            cmbMaterial.SelectedIndexChanged += CmbMaterial_SelectedIndexChanged;
            this.Controls.Add(cmbMaterial);
            y += 35;

            // Quantity & Unit
            AddLabel("Purchase Quantity *", 24, y);
            AddLabel("Unit Price (RWF) *", 260, y);
            y += 20;

            numQuantity = new NumericUpDown();
            numQuantity.Location = new Point(24, y);
            numQuantity.Size = new Size(130, 26);
            numQuantity.Maximum = 1000000m;
            numQuantity.DecimalPlaces = 2;
            numQuantity.Value = 10;
            numQuantity.ValueChanged += delegate { UpdateTotal(); };
            this.Controls.Add(numQuantity);

            lblUnitDisplay = new Label();
            lblUnitDisplay.Text = "Units";
            lblUnitDisplay.Location = new Point(160, y + 3);
            lblUnitDisplay.AutoSize = true;
            lblUnitDisplay.ForeColor = Color.FromArgb(100, 116, 139);
            this.Controls.Add(lblUnitDisplay);

            numUnitPrice = new NumericUpDown();
            numUnitPrice.Location = new Point(260, y);
            numUnitPrice.Size = new Size(254, 26);
            numUnitPrice.Maximum = 100000000m;
            numUnitPrice.DecimalPlaces = 2;
            numUnitPrice.ThousandsSeparator = true;
            numUnitPrice.ValueChanged += delegate { UpdateTotal(); };
            this.Controls.Add(numUnitPrice);
            y += 35;

            // Total Calculation Display Box
            Panel pnlTotal = new Panel();
            pnlTotal.Location = new Point(24, y);
            pnlTotal.Size = new Size(490, 38);
            pnlTotal.BackColor = Color.FromArgb(241, 245, 249);
            pnlTotal.BorderStyle = BorderStyle.FixedSingle;

            lblTotalCalculation = new Label();
            lblTotalCalculation.Text = "Total Expenditure: RWF 0.00";
            lblTotalCalculation.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            lblTotalCalculation.ForeColor = Color.FromArgb(30, 41, 59);
            lblTotalCalculation.Location = new Point(10, 8);
            lblTotalCalculation.AutoSize = true;
            pnlTotal.Controls.Add(lblTotalCalculation);
            this.Controls.Add(pnlTotal);
            y += 50;

            // Supplier Name
            AddLabel("Supplier / Vendor Name *", 24, y);
            y += 20;
            txtSupplier = new TextBox();
            txtSupplier.Location = new Point(24, y);
            txtSupplier.Size = new Size(490, 26);
            this.Controls.Add(txtSupplier);
            y += 35;

            // Purchase Date
            AddLabel("Purchase / Delivery Date *", 24, y);
            y += 20;
            dtpPurchaseDate = new DateTimePicker();
            dtpPurchaseDate.Format = DateTimePickerFormat.Short;
            dtpPurchaseDate.Location = new Point(24, y);
            dtpPurchaseDate.Size = new Size(220, 26);
            this.Controls.Add(dtpPurchaseDate);
            y += 35;

            // Error Label
            lblError = new Label();
            lblError.Location = new Point(24, y);
            lblError.Size = new Size(490, 30);
            lblError.ForeColor = Color.FromArgb(220, 38, 38);
            lblError.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblError.Visible = false;
            this.Controls.Add(lblError);
            y += 40;

            // Buttons
            btnRecord = new Button();
            btnRecord.Text = "Record Purchase";
            btnRecord.Location = new Point(270, y);
            btnRecord.Size = new Size(140, 38);
            btnRecord.BackColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
            btnRecord.ForeColor = Color.White;
            btnRecord.FlatStyle = FlatStyle.Flat;
            btnRecord.FlatAppearance.BorderSize = 0;
            btnRecord.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnRecord.Cursor = Cursors.Hand;
            btnRecord.Click += BtnRecord_Click;
            this.Controls.Add(btnRecord);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(420, y);
            btnCancel.Size = new Size(94, 38);
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

        private void LoadProjects()
        {
            cmbProject.Items.Clear();
            List<Project> projs = _projectController.GetProjectsForCurrentUser();
            int selectedIndex = 0;
            for (int i = 0; i < projs.Count; i++)
            {
                Project p = projs[i];
                cmbProject.Items.Add(new ProjectItem(p.Id, p.ProjectName));
                if (_preselectedProjectId.HasValue && p.Id == _preselectedProjectId.Value)
                {
                    selectedIndex = i;
                }
            }
            if (cmbProject.Items.Count > 0)
            {
                cmbProject.SelectedIndex = selectedIndex;
                if (_preselectedProjectId.HasValue)
                {
                    cmbProject.Enabled = false; // Lock to selected project
                }
            }
        }

        private void LoadMaterials()
        {
            cmbMaterial.Items.Clear();
            List<Material> materials = _materialController.GetAllMaterials();
            foreach (Material m in materials)
            {
                if (string.Equals(m.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
                {
                    cmbMaterial.Items.Add(new MaterialItem(m.Id, m.MaterialName, m.Unit, m.CurrentPrice));
                }
            }
            if (cmbMaterial.Items.Count > 0) cmbMaterial.SelectedIndex = 0;
        }

        private void CmbMaterial_SelectedIndexChanged(object sender, EventArgs e)
        {
            MaterialItem item = cmbMaterial.SelectedItem as MaterialItem;
            if (item != null)
            {
                lblUnitDisplay.Text = item.Unit;
                if (numUnitPrice.Value == 0)
                {
                    numUnitPrice.Value = item.Price;
                }
            }
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = numQuantity.Value * numUnitPrice.Value;
            lblTotalCalculation.Text = string.Format("Total Expenditure: RWF {0:N2}", total);
        }

        private void BtnRecord_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;

            ProjectItem proj = cmbProject.SelectedItem as ProjectItem;
            if (proj == null)
            {
                lblError.Text = "Please select a project.";
                lblError.Visible = true;
                return;
            }

            MaterialItem mat = cmbMaterial.SelectedItem as MaterialItem;
            if (mat == null)
            {
                lblError.Text = "Please select a material.";
                lblError.Visible = true;
                return;
            }

            Result<int> res = _stockController.RecordPurchase(
                proj.Id,
                mat.Id,
                numQuantity.Value,
                numUnitPrice.Value,
                txtSupplier.Text,
                dtpPurchaseDate.Value.Date
            );

            if (!res.IsSuccess)
            {
                lblError.Text = res.ErrorMessage;
                lblError.Visible = true;
            }
            else
            {
                MessageBox.Show(
                    string.Format("Successfully recorded purchase of {0} {1} of {2}.\nStock balance and weighted average price updated.",
                        numQuantity.Value, mat.Unit, mat.Name),
                    "Purchase Recorded",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private class ProjectItem
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public ProjectItem(int id, string name) { this.Id = id; this.Name = name; }
            public override string ToString() { return Name; }
        }

        private class MaterialItem
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public string Unit { get; private set; }
            public decimal Price { get; private set; }
            public MaterialItem(int id, string name, string unit, decimal price) { this.Id = id; this.Name = name; this.Unit = unit; this.Price = price; }
            public override string ToString() { return string.Format("{0} ({1})", Name, Unit); }
        }
    }
}
