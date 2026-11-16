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
    public class UcMaterials : UserControl
    {
        private readonly MaterialController _materialController;

        private TabControl tabControl;
        private TabPage tabMaterials;
        private TabPage tabCategories;

        // Materials tab controls
        private DataGridView gridMaterials;
        private ComboBox cmbCategoryFilter;
        private TextBox txtSearchMaterials;
        private Button btnAddMaterial;
        private Button btnEditMaterial;
        private Button btnDeleteMaterial;
        private Button btnRefreshMaterials;
        private Label lblTotalMaterials;

        // Categories tab controls
        private DataGridView gridCategories;
        private Button btnAddCategory;
        private Button btnEditCategory;
        private Button btnDeleteCategory;
        private Button btnRefreshCategories;
        private Label lblTotalCategories;

        public UcMaterials() : this(new MaterialController())
        {
        }

        public UcMaterials(MaterialController materialController)
        {
            this._materialController = materialController ?? new MaterialController();
            InitializeComponent();
            ApplyRoleRestrictions();
            LoadCategories();
            LoadMaterials();
        }

        private void InitializeComponent()
        {
            this.BackColor = Color.White;
            this.Dock = DockStyle.Fill;
            this.Font = new Font("Segoe UI", 9.5f);
            this.Padding = new Padding(16);

            tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // --- Tab 1: Materials Catalog ---
            tabMaterials = new TabPage("Master Materials Catalog");
            tabMaterials.Padding = new Padding(16);
            tabMaterials.BackColor = Color.White;

            Panel pnlMatToolbar = new Panel();
            pnlMatToolbar.Dock = DockStyle.Top;
            pnlMatToolbar.Height = 55;

            btnAddMaterial = new Button();
            btnAddMaterial.Text = "+ Add Material";
            btnAddMaterial.Location = new Point(0, 8);
            btnAddMaterial.Size = new Size(115, 36);
            btnAddMaterial.BackColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
            btnAddMaterial.ForeColor = Color.White;
            btnAddMaterial.FlatStyle = FlatStyle.Flat;
            btnAddMaterial.FlatAppearance.BorderSize = 0;
            btnAddMaterial.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnAddMaterial.Cursor = Cursors.Hand;
            btnAddMaterial.Click += BtnAddMaterial_Click;
            pnlMatToolbar.Controls.Add(btnAddMaterial);

            btnEditMaterial = new Button();
            btnEditMaterial.Text = "Edit";
            btnEditMaterial.Location = new Point(125, 8);
            btnEditMaterial.Size = new Size(75, 36);
            btnEditMaterial.BackColor = Color.FromArgb(241, 245, 249);
            btnEditMaterial.FlatStyle = FlatStyle.Flat;
            btnEditMaterial.FlatAppearance.BorderSize = 1;
            btnEditMaterial.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnEditMaterial.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnEditMaterial.Click += BtnEditMaterial_Click;
            pnlMatToolbar.Controls.Add(btnEditMaterial);

            btnDeleteMaterial = new Button();
            btnDeleteMaterial.Text = "Delete";
            btnDeleteMaterial.Location = new Point(210, 8);
            btnDeleteMaterial.Size = new Size(75, 36);
            btnDeleteMaterial.BackColor = Color.FromArgb(241, 245, 249);
            btnDeleteMaterial.FlatStyle = FlatStyle.Flat;
            btnDeleteMaterial.FlatAppearance.BorderSize = 1;
            btnDeleteMaterial.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnDeleteMaterial.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnDeleteMaterial.Click += BtnDeleteMaterial_Click;
            pnlMatToolbar.Controls.Add(btnDeleteMaterial);

            btnRefreshMaterials = new Button();
            btnRefreshMaterials.Text = "Refresh";
            btnRefreshMaterials.Location = new Point(295, 8);
            btnRefreshMaterials.Size = new Size(75, 36);
            btnRefreshMaterials.BackColor = Color.FromArgb(241, 245, 249);
            btnRefreshMaterials.FlatStyle = FlatStyle.Flat;
            btnRefreshMaterials.FlatAppearance.BorderSize = 1;
            btnRefreshMaterials.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefreshMaterials.Click += delegate { LoadMaterials(); };
            pnlMatToolbar.Controls.Add(btnRefreshMaterials);

            // Filter Category
            Label lblCat = new Label();
            lblCat.Text = "Category:";
            lblCat.Location = new Point(390, 16);
            lblCat.AutoSize = true;
            lblCat.ForeColor = Color.FromArgb(100, 116, 139);
            pnlMatToolbar.Controls.Add(lblCat);

            cmbCategoryFilter = new ComboBox();
            cmbCategoryFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCategoryFilter.Location = new Point(455, 12);
            cmbCategoryFilter.Size = new Size(180, 26);
            cmbCategoryFilter.SelectedIndexChanged += delegate { LoadMaterials(); };
            pnlMatToolbar.Controls.Add(cmbCategoryFilter);

            // Search
            Label lblSearch = new Label();
            lblSearch.Text = "Search:";
            lblSearch.Location = new Point(650, 16);
            lblSearch.AutoSize = true;
            lblSearch.ForeColor = Color.FromArgb(100, 116, 139);
            pnlMatToolbar.Controls.Add(lblSearch);

            txtSearchMaterials = new TextBox();
            txtSearchMaterials.Location = new Point(700, 12);
            txtSearchMaterials.Size = new Size(160, 26);
            txtSearchMaterials.TextChanged += delegate { FilterMaterials(); };
            pnlMatToolbar.Controls.Add(txtSearchMaterials);

            lblTotalMaterials = new Label();
            lblTotalMaterials.Text = "Total: 0 materials";
            lblTotalMaterials.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotalMaterials.Location = new Point(pnlMatToolbar.Width - 140, 16);
            lblTotalMaterials.AutoSize = true;
            lblTotalMaterials.ForeColor = Color.FromArgb(100, 116, 139);
            pnlMatToolbar.Controls.Add(lblTotalMaterials);

            tabMaterials.Controls.Add(pnlMatToolbar);

            gridMaterials = CreateStyledGrid();
            gridMaterials.Columns.Add("Id", "ID");
            gridMaterials.Columns["Id"].Width = 45;
            gridMaterials.Columns.Add("MaterialName", "Material Name");
            gridMaterials.Columns.Add("CategoryName", "Category");
            gridMaterials.Columns.Add("Unit", "Unit");
            gridMaterials.Columns["Unit"].Width = 80;
            gridMaterials.Columns.Add("Price", "Ref. Price (RWF)");
            gridMaterials.Columns["Price"].Width = 120;
            gridMaterials.Columns.Add("Description", "Description & Specs");
            gridMaterials.Columns.Add("Status", "Status");
            gridMaterials.Columns["Status"].Width = 90;

            tabMaterials.Controls.Add(gridMaterials);
            gridMaterials.BringToFront();
            tabControl.TabPages.Add(tabMaterials);

            // --- Tab 2: Categories ---
            tabCategories = new TabPage("Material Categories");
            tabCategories.Padding = new Padding(16);
            tabCategories.BackColor = Color.White;

            Panel pnlCatToolbar = new Panel();
            pnlCatToolbar.Dock = DockStyle.Top;
            pnlCatToolbar.Height = 55;

            btnAddCategory = new Button();
            btnAddCategory.Text = "+ Add Category";
            btnAddCategory.Location = new Point(0, 8);
            btnAddCategory.Size = new Size(120, 36);
            btnAddCategory.BackColor = Color.FromArgb(235, 86, 18); // Main #eb5612 Brand Orange
            btnAddCategory.ForeColor = Color.White;
            btnAddCategory.FlatStyle = FlatStyle.Flat;
            btnAddCategory.FlatAppearance.BorderSize = 0;
            btnAddCategory.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnAddCategory.Cursor = Cursors.Hand;
            btnAddCategory.Click += BtnAddCategory_Click;
            pnlCatToolbar.Controls.Add(btnAddCategory);

            btnEditCategory = new Button();
            btnEditCategory.Text = "Edit";
            btnEditCategory.Location = new Point(130, 8);
            btnEditCategory.Size = new Size(75, 36);
            btnEditCategory.BackColor = Color.FromArgb(241, 245, 249);
            btnEditCategory.FlatStyle = FlatStyle.Flat;
            btnEditCategory.FlatAppearance.BorderSize = 1;
            btnEditCategory.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnEditCategory.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnEditCategory.Click += BtnEditCategory_Click;
            pnlCatToolbar.Controls.Add(btnEditCategory);

            btnDeleteCategory = new Button();
            btnDeleteCategory.Text = "Delete";
            btnDeleteCategory.Location = new Point(215, 8);
            btnDeleteCategory.Size = new Size(75, 36);
            btnDeleteCategory.BackColor = Color.FromArgb(241, 245, 249);
            btnDeleteCategory.FlatStyle = FlatStyle.Flat;
            btnDeleteCategory.FlatAppearance.BorderSize = 1;
            btnDeleteCategory.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnDeleteCategory.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
            btnDeleteCategory.Click += BtnDeleteCategory_Click;
            pnlCatToolbar.Controls.Add(btnDeleteCategory);

            btnRefreshCategories = new Button();
            btnRefreshCategories.Text = "Refresh";
            btnRefreshCategories.Location = new Point(300, 8);
            btnRefreshCategories.Size = new Size(75, 36);
            btnRefreshCategories.BackColor = Color.FromArgb(241, 245, 249);
            btnRefreshCategories.FlatStyle = FlatStyle.Flat;
            btnRefreshCategories.FlatAppearance.BorderSize = 1;
            btnRefreshCategories.FlatAppearance.BorderColor = Color.FromArgb(203, 213, 225);
            btnRefreshCategories.Click += delegate { LoadCategories(); };
            pnlCatToolbar.Controls.Add(btnRefreshCategories);

            lblTotalCategories = new Label();
            lblTotalCategories.Text = "Total: 0 categories";
            lblTotalCategories.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblTotalCategories.Location = new Point(pnlCatToolbar.Width - 140, 16);
            lblTotalCategories.AutoSize = true;
            lblTotalCategories.ForeColor = Color.FromArgb(100, 116, 139);
            pnlCatToolbar.Controls.Add(lblTotalCategories);

            tabCategories.Controls.Add(pnlCatToolbar);

            gridCategories = CreateStyledGrid();
            gridCategories.Columns.Add("Id", "ID");
            gridCategories.Columns["Id"].Width = 50;
            gridCategories.Columns.Add("CategoryName", "Category Name");
            gridCategories.Columns.Add("Description", "Description");

            tabCategories.Controls.Add(gridCategories);
            gridCategories.BringToFront();
            tabControl.TabPages.Add(tabCategories);

            this.Controls.Add(tabControl);
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

        private void ApplyRoleRestrictions()
        {
            if (!BaseController.IsAdmin)
            {
                btnAddMaterial.Visible = false;
                btnDeleteMaterial.Visible = false;
                btnEditMaterial.Text = "View";

                btnAddCategory.Visible = false;
                btnDeleteCategory.Visible = false;
                btnEditCategory.Text = "View";
            }
        }

        private List<MaterialCategory> _cachedCategories = new List<MaterialCategory>();
        private List<Material> _cachedMaterials = new List<Material>();

        private void LoadCategories()
        {
            _cachedCategories = _materialController.GetAllCategories();
            gridCategories.Rows.Clear();

            cmbCategoryFilter.Items.Clear();
            cmbCategoryFilter.Items.Add(new FilterItem(null, "All Categories"));

            foreach (MaterialCategory c in _cachedCategories)
            {
                gridCategories.Rows.Add(c.Id, c.CategoryName, c.Description);
                cmbCategoryFilter.Items.Add(new FilterItem(c.Id, c.CategoryName));
            }
            cmbCategoryFilter.SelectedIndex = 0;
            lblTotalCategories.Text = string.Format("Total: {0} categories", gridCategories.Rows.Count);
        }

        private void LoadMaterials()
        {
            int? catId = null;
            FilterItem selectedFilter = cmbCategoryFilter.SelectedItem as FilterItem;
            if (selectedFilter != null && selectedFilter.Id.HasValue)
            {
                catId = selectedFilter.Id.Value;
            }

            _cachedMaterials = _materialController.GetAllMaterials(catId);
            FilterMaterials();
        }

        private void FilterMaterials()
        {
            gridMaterials.Rows.Clear();
            string query = txtSearchMaterials.Text.Trim().ToLowerInvariant();

            foreach (Material m in _cachedMaterials)
            {
                if (string.IsNullOrEmpty(query) ||
                    m.MaterialName.ToLowerInvariant().Contains(query) ||
                    (m.CategoryName != null && m.CategoryName.ToLowerInvariant().Contains(query)) ||
                    (m.Unit != null && m.Unit.ToLowerInvariant().Contains(query)))
                {
                    gridMaterials.Rows.Add(
                        m.Id,
                        m.MaterialName,
                        m.CategoryName,
                        m.Unit,
                        string.Format("{0:N2}", m.CurrentPrice),
                        m.Description,
                        m.Status
                    );
                }
            }
            lblTotalMaterials.Text = string.Format("Total: {0} materials", gridMaterials.Rows.Count);
        }

        private Material GetSelectedMaterial()
        {
            if (gridMaterials.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(gridMaterials.SelectedRows[0].Cells["Id"].Value);
            return _materialController.GetMaterialById(id);
        }

        private MaterialCategory GetSelectedCategory()
        {
            if (gridCategories.SelectedRows.Count == 0) return null;
            int id = Convert.ToInt32(gridCategories.SelectedRows[0].Cells["Id"].Value);
            return _materialController.GetCategoryById(id);
        }

        private void BtnAddMaterial_Click(object sender, EventArgs e)
        {
            using (FormMaterialDialog dlg = new FormMaterialDialog(_materialController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadMaterials();
                }
            }
        }

        private void BtnEditMaterial_Click(object sender, EventArgs e)
        {
            Material selected = GetSelectedMaterial();
            if (selected == null)
            {
                MessageBox.Show("Please select a material first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FormMaterialDialog dlg = new FormMaterialDialog(_materialController, selected))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadMaterials();
                }
            }
        }

        private void BtnDeleteMaterial_Click(object sender, EventArgs e)
        {
            Material selected = GetSelectedMaterial();
            if (selected == null)
            {
                MessageBox.Show("Please select a material first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult dr = MessageBox.Show(
                string.Format("Are you sure you want to delete material '{0}'?", selected.MaterialName),
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dr == DialogResult.Yes)
            {
                Result res = _materialController.DeleteMaterial(selected.Id);
                if (!res.IsSuccess)
                {
                    MessageBox.Show(res.ErrorMessage, "Deletion Blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    LoadMaterials();
                }
            }
        }

        private void BtnAddCategory_Click(object sender, EventArgs e)
        {
            using (FormCategoryDialog dlg = new FormCategoryDialog(_materialController))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadCategories();
                }
            }
        }

        private void BtnEditCategory_Click(object sender, EventArgs e)
        {
            MaterialCategory selected = GetSelectedCategory();
            if (selected == null)
            {
                MessageBox.Show("Please select a category first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            using (FormCategoryDialog dlg = new FormCategoryDialog(_materialController, selected))
            {
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    LoadCategories();
                }
            }
        }

        private void BtnDeleteCategory_Click(object sender, EventArgs e)
        {
            MaterialCategory selected = GetSelectedCategory();
            if (selected == null)
            {
                MessageBox.Show("Please select a category first.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult dr = MessageBox.Show(
                string.Format("Are you sure you want to delete category '{0}'?", selected.CategoryName),
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (dr == DialogResult.Yes)
            {
                Result res = _materialController.DeleteCategory(selected.Id);
                if (!res.IsSuccess)
                {
                    MessageBox.Show(res.ErrorMessage, "Deletion Blocked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    LoadCategories();
                }
            }
        }

        private class FilterItem
        {
            public int? Id { get; private set; }
            public string Name { get; private set; }
            public FilterItem(int? id, string name) { this.Id = id; this.Name = name; }
            public override string ToString() { return Name; }
        }
    }
}
