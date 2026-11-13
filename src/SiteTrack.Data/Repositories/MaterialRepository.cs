using System;
using System.Collections.Generic;
using System.Linq;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Connection;

namespace SiteTrack.Data.Repositories
{
    public class MaterialRepository : IMaterialRepository
    {
        private readonly SiteTrackDataStore _store;

        public MaterialRepository()
        {
            this._store = SiteTrackDataStore.Instance;
        }

        public List<MaterialCategory> GetAllCategories()
        {
            lock (_store.SyncRoot)
            {
                return _store.MaterialCategories.OrderBy(delegate(MaterialCategory c) { return c.CategoryName; }).ToList();
            }
        }

        public MaterialCategory GetCategoryById(int id)
        {
            lock (_store.SyncRoot)
            {
                return _store.MaterialCategories.FirstOrDefault(delegate(MaterialCategory c) { return c.Id == id; });
            }
        }

        public int InsertCategory(MaterialCategory category)
        {
            if (category == null) throw new ArgumentNullException("category");

            lock (_store.SyncRoot)
            {
                if (_store.MaterialCategories.Any(delegate(MaterialCategory c) { return string.Equals(c.CategoryName, category.CategoryName, StringComparison.OrdinalIgnoreCase); }))
                {
                    throw new ValidationException("CategoryName", string.Format("Category '{0}' already exists.", category.CategoryName));
                }

                category.Id = _store.NextCategoryId();
                category.CreatedAt = DateTime.Now;
                category.UpdatedAt = DateTime.Now;
                _store.MaterialCategories.Add(category);
                return category.Id;
            }
        }

        public bool UpdateCategory(MaterialCategory category)
        {
            if (category == null) throw new ArgumentNullException("category");

            lock (_store.SyncRoot)
            {
                MaterialCategory existing = _store.MaterialCategories.FirstOrDefault(delegate(MaterialCategory c) { return c.Id == category.Id; });
                if (existing == null) return false;

                if (_store.MaterialCategories.Any(delegate(MaterialCategory c) { return c.Id != category.Id && string.Equals(c.CategoryName, category.CategoryName, StringComparison.OrdinalIgnoreCase); }))
                {
                    throw new ValidationException("CategoryName", string.Format("Category '{0}' already exists.", category.CategoryName));
                }

                existing.CategoryName = category.CategoryName;
                existing.Description = category.Description;
                existing.UpdatedAt = DateTime.Now;
                return true;
            }
        }

        public bool DeleteCategory(int id)
        {
            lock (_store.SyncRoot)
            {
                if (_store.Materials.Any(delegate(Material m) { return m.CategoryId == id; }))
                {
                    throw new ValidationException("CategoryId", "Cannot delete category: materials are linked to it.");
                }

                MaterialCategory existing = _store.MaterialCategories.FirstOrDefault(delegate(MaterialCategory c) { return c.Id == id; });
                if (existing == null) return false;

                _store.MaterialCategories.Remove(existing);
                return true;
            }
        }

        public List<Material> GetAllMaterials(int? categoryId)
        {
            lock (_store.SyncRoot)
            {
                IEnumerable<Material> query = _store.Materials;
                if (categoryId.HasValue)
                {
                    query = query.Where(delegate(Material m) { return m.CategoryId == categoryId.Value; });
                }

                List<Material> list = query.OrderBy(delegate(Material m) { return m.MaterialName; }).ToList();
                foreach (Material m in list)
                {
                    MaterialCategory cat = _store.MaterialCategories.FirstOrDefault(delegate(MaterialCategory c) { return c.Id == m.CategoryId; });
                    if (cat != null) m.CategoryName = cat.CategoryName;
                }
                return list;
            }
        }

        public Material GetMaterialById(int id)
        {
            lock (_store.SyncRoot)
            {
                Material m = _store.Materials.FirstOrDefault(delegate(Material item) { return item.Id == id; });
                if (m != null)
                {
                    MaterialCategory cat = _store.MaterialCategories.FirstOrDefault(delegate(MaterialCategory c) { return c.Id == m.CategoryId; });
                    if (cat != null) m.CategoryName = cat.CategoryName;
                }
                return m;
            }
        }

        public int InsertMaterial(Material material)
        {
            if (material == null) throw new ArgumentNullException("material");
            if (material.CurrentPrice < 0)
            {
                throw new ValidationException("CurrentPrice", "Material price cannot be negative.");
            }

            lock (_store.SyncRoot)
            {
                material.Id = _store.NextMaterialId();
                material.CreatedAt = DateTime.Now;
                material.UpdatedAt = DateTime.Now;
                _store.Materials.Add(material);
                return material.Id;
            }
        }

        public bool UpdateMaterial(Material material)
        {
            if (material == null) throw new ArgumentNullException("material");
            if (material.CurrentPrice < 0)
            {
                throw new ValidationException("CurrentPrice", "Material price cannot be negative.");
            }

            lock (_store.SyncRoot)
            {
                Material existing = _store.Materials.FirstOrDefault(delegate(Material m) { return m.Id == material.Id; });
                if (existing == null) return false;

                existing.CategoryId = material.CategoryId;
                existing.MaterialName = material.MaterialName;
                existing.Unit = material.Unit;
                existing.CurrentPrice = material.CurrentPrice;
                existing.Description = material.Description;
                existing.Status = material.Status;
                existing.UpdatedAt = DateTime.Now;
                return true;
            }
        }

        public bool DeleteMaterial(int id)
        {
            lock (_store.SyncRoot)
            {
                bool hasPurchases = _store.MaterialPurchases.Any(delegate(MaterialPurchase p) { return p.MaterialId == id; });
                bool hasUsage = _store.MaterialUsages.Any(delegate(MaterialUsage u) { return u.MaterialId == id; });

                if (hasPurchases || hasUsage)
                {
                    throw new ValidationException("MaterialId", "Cannot delete material: associated purchase or usage records exist.");
                }

                Material existing = _store.Materials.FirstOrDefault(delegate(Material m) { return m.Id == id; });
                if (existing == null) return false;

                _store.Materials.Remove(existing);
                return true;
            }
        }
    }
}

