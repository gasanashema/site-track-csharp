using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.UI.Forms.Dialogs
{
    public class FormMaterialDialog : Form
    {
        private readonly MaterialController _materialController;
        private readonly Material _existingMaterial;

        private ComboBox cmbCategory;
        private TextBox txtName;
        private TextBox txtUnit;
        private NumericUpDown numPrice;
        private TextBox txtDescription;
        private ComboBox cmbStatus;
        private Button btnSave;
        private Button btnCancel;
        private Label lblError;

        public bool IsEditMode { get { return _existingMaterial != null; } }

        public FormMaterialDialog(MaterialController materialController, Material existingMaterial = null)
        {
            this._materialController = materialController ?? new MaterialController();
            this._existingMaterial = existingMaterial;
            InitializeComponent();
            LoadCategories();
            PopulateData();
        }

        private void InitializeComponent()
        {
            this.Text = IsEditMode ? "SiteTrack — Edit Material" : "SiteTrack — Add Master Material";
            this.Size = new Size(480, 520);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(248, 250, 252);
            this.Font = new Font("Segoe UI", 9.5f);

            int y = 20;

            Label lblTitle = new Label();
            lblTitle.Text = IsEditMode ? "Edit Master Material" : "Register Master Material";
            lblTitle.Font = new Font("Segoe UI", 12f, FontStyle.Bold);
            lblTitle.ForeColor = Color.FromArgb(15, 23, 42);
            lblTitle.Location = new Point(24, y);
            lblTitle.AutoSize = true;
            this.Controls.Add(lblTitle);
            y += 40;

            // Category
            AddLabel("Category *", 24, y);
            y += 20;
            cmbCategory = new ComboBox();
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategory.Location = new Point(24, y);
            cmbCategory.Size = new Size(416, 26);
            this.Controls.Add(cmbCategory);
            y += 35;

            // Material Name
            AddLabel("Material Name *", 24, y);
            y += 20;
            txtName = new TextBox();
            txtName.Location = new Point(24, y);
            txtName.Size = new Size(416, 26);
            this.Controls.Add(txtName);
            y += 35;

            // Unit of Measurement & Reference Price
            AddLabel("Unit (e.g. Bags, Tons, Pieces) *", 24, y);
            AddLabel("Reference Unit Price (RWF)", 240, y);
            y += 20;

            txtUnit = new TextBox();
            txtUnit.Location = new Point(24, y);
            txtUnit.Size = new Size(200, 26);
            txtUnit.Text = "Bags";
            this.Controls.Add(txtUnit);

            numPrice = new NumericUpDown();
            numPrice.Location = new Point(240, y);
            numPrice.Size = new Size(200, 26);
            numPrice.Maximum = 100000000m;
            numPrice.DecimalPlaces = 2;
            numPrice.ThousandsSeparator = true;
            this.Controls.Add(numPrice);
            y += 35;

            // Description
            AddLabel("Description & Specifications", 24, y);
            y += 20;
            txtDescription = new TextBox();
            txtDescription.Multiline = true;
            txtDescription.Location = new Point(24, y);
            txtDescription.Size = new Size(416, 50);
            this.Controls.Add(txtDescription);
            y += 60;

            // Status
            AddLabel("Catalog Status", 24, y);
            y += 20;
            cmbStatus = new ComboBox();
            cmbStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbStatus.Items.AddRange(new object[] { "ACTIVE", "INACTIVE" });
            cmbStatus.SelectedIndex = 0;
            cmbStatus.Location = new Point(24, y);
            cmbStatus.Size = new Size(200, 26);
            this.Controls.Add(cmbStatus);
            y += 40;

            // Error Label
            lblError = new Label();
            lblError.Location = new Point(24, y);
            lblError.Size = new Size(416, 30);
            lblError.ForeColor = Color.FromArgb(220, 38, 38);
            lblError.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            lblError.Visible = false;
            this.Controls.Add(lblError);
            y += 35;

            // Buttons
            btnSave = new Button();
            btnSave.Text = IsEditMode ? "Save Material" : "Create Material";
            btnSave.Location = new Point(230, y);
            btnSave.Size = new Size(125, 34);
            btnSave.BackColor = Color.FromArgb(37, 99, 235);
            btnSave.ForeColor = Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Font = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            btnCancel = new Button();
            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(365, y);
            btnCancel.Size = new Size(75, 34);
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

        private void LoadCategories()
        {
            cmbCategory.Items.Clear();
            List<MaterialCategory> cats = _materialController.GetAllCategories();
            foreach (MaterialCategory c in cats)
            {
                cmbCategory.Items.Add(new CategoryItem(c.Id, c.CategoryName));
            }
            if (cmbCategory.Items.Count > 0) cmbCategory.SelectedIndex = 0;
        }

        private void PopulateData()
        {
            if (_existingMaterial != null)
            {
                txtName.Text = _existingMaterial.MaterialName;
                txtUnit.Text = _existingMaterial.Unit;
                numPrice.Value = _existingMaterial.CurrentPrice;
                txtDescription.Text = _existingMaterial.Description;
                cmbStatus.SelectedItem = _existingMaterial.Status;

                for (int i = 0; i < cmbCategory.Items.Count; i++)
                {
                    CategoryItem item = cmbCategory.Items[i] as CategoryItem;
                    if (item != null && item.Id == _existingMaterial.CategoryId)
                    {
                        cmbCategory.SelectedIndex = i;
                        break;
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            lblError.Visible = false;
            CategoryItem cat = cmbCategory.SelectedItem as CategoryItem;
            if (cat == null)
            {
                lblError.Text = "Please select a category.";
                lblError.Visible = true;
                return;
            }

            string status = cmbStatus.SelectedItem != null ? cmbStatus.SelectedItem.ToString() : "ACTIVE";

            if (IsEditMode)
            {
                Result res = _materialController.UpdateMaterial(
                    _existingMaterial.Id,
                    cat.Id,
                    txtName.Text,
                    txtUnit.Text,
                    numPrice.Value,
                    txtDescription.Text,
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
                Result<int> res = _materialController.CreateMaterial(
                    cat.Id,
                    txtName.Text,
                    txtUnit.Text,
                    numPrice.Value,
                    txtDescription.Text
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

        private class CategoryItem
        {
            public int Id { get; private set; }
            public string Name { get; private set; }
            public CategoryItem(int id, string name) { this.Id = id; this.Name = name; }
            public override string ToString() { return Name; }
        }
    }
}
