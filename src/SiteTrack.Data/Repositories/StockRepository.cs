using System;
using System.Collections.Generic;
using System.Linq;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Connection;

namespace SiteTrack.Data.Repositories
{
    public class StockRepository : IStockRepository
    {
        private readonly SiteTrackDataStore _store;

        public StockRepository()
        {
            this._store = SiteTrackDataStore.Instance;
        }

        public List<ProjectMaterialStock> GetStockByProject(int projectId)
        {
            lock (_store.SyncRoot)
            {
                List<ProjectMaterialStock> list = _store.ProjectMaterialStocks
                    .Where(delegate(ProjectMaterialStock s) { return s.ProjectId == projectId; })
                    .ToList();

                foreach (ProjectMaterialStock s in list)
                {
                    EnrichStock(s);
                }
                return list;
            }
        }

        public ProjectMaterialStock GetStock(int projectId, int materialId)
        {
            lock (_store.SyncRoot)
            {
                ProjectMaterialStock s = _store.ProjectMaterialStocks
                    .FirstOrDefault(delegate(ProjectMaterialStock item) { return item.ProjectId == projectId && item.MaterialId == materialId; });
                if (s != null)
                {
                    EnrichStock(s);
                }
                return s;
            }
        }

        public int RecordPurchase(MaterialPurchase purchase)
        {
            if (purchase == null) throw new ArgumentNullException("purchase");
            if (purchase.Quantity <= 0) throw new ValidationException("Quantity", "Purchase quantity must be greater than zero.");
            if (purchase.UnitPrice < 0) throw new ValidationException("UnitPrice", "Unit price cannot be negative.");

            purchase.TotalPrice = purchase.Quantity * purchase.UnitPrice;

            lock (_store.SyncRoot)
            {
                purchase.Id = _store.NextPurchaseId();
                purchase.CreatedAt = DateTime.Now;
                purchase.UpdatedAt = DateTime.Now;
                _store.MaterialPurchases.Add(purchase);

                // Update or create Stock Record
                ProjectMaterialStock stock = _store.ProjectMaterialStocks
                    .FirstOrDefault(delegate(ProjectMaterialStock s) { return s.ProjectId == purchase.ProjectId && s.MaterialId == purchase.MaterialId; });

                if (stock == null)
                {
                    stock = new ProjectMaterialStock
                    {
                        Id = _store.NextStockId(),
                        ProjectId = purchase.ProjectId,
                        MaterialId = purchase.MaterialId,
                        QuantityAvailable = purchase.Quantity,
                        AverageUnitPrice = purchase.UnitPrice,
                        MinimumQuantity = 10.00m // Default minimum
                    };
                    _store.ProjectMaterialStocks.Add(stock);
                }
                else
                {
                    decimal currentQty = stock.QuantityAvailable;
                    decimal currentAvgPrice = stock.AverageUnitPrice;
                    decimal newQty = purchase.Quantity;
                    decimal newPrice = purchase.UnitPrice;

                    decimal totalCost = (currentQty * currentAvgPrice) + (newQty * newPrice);
                    decimal totalQty = currentQty + newQty;

                    stock.QuantityAvailable = totalQty;
                    stock.AverageUnitPrice = totalQty > 0 ? (totalCost / totalQty) : newPrice;
                    stock.UpdatedAt = DateTime.Now;
                }

                // Log immutable IN movement
                MaterialStockMovement mov = new MaterialStockMovement
                {
                    Id = _store.NextMovementId(),
                    ProjectId = purchase.ProjectId,
                    MaterialId = purchase.MaterialId,
                    MovementType = "IN",
                    Quantity = purchase.Quantity,
                    UnitPrice = purchase.UnitPrice,
                    TotalPrice = purchase.TotalPrice,
                    MovementDate = purchase.PurchaseDate,
                    Description = string.Format("Purchase from supplier: {0}", purchase.SupplierName),
                    ReferenceType = "purchase",
                    ReferenceId = purchase.Id,
                    RecordedBy = purchase.RecordedBy
                };
                _store.MaterialStockMovements.Add(mov);

                return purchase.Id;
            }
        }

        public int RecordUsage(MaterialUsage usage)
        {
            if (usage == null) throw new ArgumentNullException("usage");
            if (usage.QuantityUsed <= 0) throw new ValidationException("QuantityUsed", "Quantity used must be greater than zero.");

            lock (_store.SyncRoot)
            {
                ProjectMaterialStock stock = _store.ProjectMaterialStocks
                    .FirstOrDefault(delegate(ProjectMaterialStock s) { return s.ProjectId == usage.ProjectId && s.MaterialId == usage.MaterialId; });

                if (stock == null || stock.QuantityAvailable < usage.QuantityUsed)
                {
                    decimal available = stock != null ? stock.QuantityAvailable : 0m;
                    throw new ValidationException("QuantityUsed",
                        string.Format("Material usage rejected: Insufficient stock. Requested: {0}, Available: {1}.", usage.QuantityUsed, available));
                }

                usage.UnitPrice = stock.AverageUnitPrice;
                usage.TotalCost = usage.QuantityUsed * usage.UnitPrice;
                usage.Id = _store.NextUsageId();
                usage.CreatedAt = DateTime.Now;
                usage.UpdatedAt = DateTime.Now;
                _store.MaterialUsages.Add(usage);

                // Deduct stock
                stock.QuantityAvailable -= usage.QuantityUsed;
                stock.UpdatedAt = DateTime.Now;

                // Log immutable OUT movement
                MaterialStockMovement mov = new MaterialStockMovement
                {
                    Id = _store.NextMovementId(),
                    ProjectId = usage.ProjectId,
                    MaterialId = usage.MaterialId,
                    MovementType = "OUT",
                    Quantity = usage.QuantityUsed,
                    UnitPrice = usage.UnitPrice,
                    TotalPrice = usage.TotalCost,
                    MovementDate = usage.UsageDate,
                    Description = string.Format("Daily usage on site: {0}", usage.ActivityDescription),
                    ReferenceType = "usage",
                    ReferenceId = usage.Id,
                    RecordedBy = usage.RecordedBy
                };
                _store.MaterialStockMovements.Add(mov);

                return usage.Id;
            }
        }

        public int RecordAdjustment(int projectId, int materialId, decimal newQuantity, string reason, int recordedBy)
        {
            if (newQuantity < 0) throw new ValidationException("NewQuantity", "Stock quantity cannot be negative.");

            lock (_store.SyncRoot)
            {
                ProjectMaterialStock stock = _store.ProjectMaterialStocks
                    .FirstOrDefault(delegate(ProjectMaterialStock s) { return s.ProjectId == projectId && s.MaterialId == materialId; });

                if (stock == null)
                {
                    stock = new ProjectMaterialStock
                    {
                        Id = _store.NextStockId(),
                        ProjectId = projectId,
                        MaterialId = materialId,
                        QuantityAvailable = newQuantity,
                        AverageUnitPrice = 0m,
                        MinimumQuantity = 10m
                    };
                    _store.ProjectMaterialStocks.Add(stock);
                }

                decimal diff = newQuantity - stock.QuantityAvailable;
                stock.QuantityAvailable = newQuantity;
                stock.UpdatedAt = DateTime.Now;

                // Log immutable ADJUSTMENT movement
                MaterialStockMovement mov = new MaterialStockMovement
                {
                    Id = _store.NextMovementId(),
                    ProjectId = projectId,
                    MaterialId = materialId,
                    MovementType = "ADJUSTMENT",
                    Quantity = Math.Abs(diff),
                    UnitPrice = stock.AverageUnitPrice,
                    TotalPrice = Math.Abs(diff) * stock.AverageUnitPrice,
                    MovementDate = DateTime.Today,
                    Description = string.Format("Physical stock audit adjustment: {0} (Delta: {1})", reason, diff),
                    ReferenceType = "adjustment",
                    ReferenceId = stock.Id,
                    RecordedBy = recordedBy
                };
                _store.MaterialStockMovements.Add(mov);

                return stock.Id;
            }
        }

        public List<MaterialStockMovement> GetMovements(int projectId, int? materialId)
        {
            lock (_store.SyncRoot)
            {
                IEnumerable<MaterialStockMovement> query = _store.MaterialStockMovements
                    .Where(delegate(MaterialStockMovement m) { return m.ProjectId == projectId; });

                if (materialId.HasValue)
                {
                    query = query.Where(delegate(MaterialStockMovement m) { return m.MaterialId == materialId.Value; });
                }

                List<MaterialStockMovement> list = query.OrderByDescending(delegate(MaterialStockMovement m) { return m.MovementDate; }).ToList();
                foreach (MaterialStockMovement mov in list)
                {
                    Material mat = _store.Materials.FirstOrDefault(delegate(Material m) { return m.Id == mov.MaterialId; });
                    if (mat != null) mov.MaterialName = mat.MaterialName;

                    Project p = _store.Projects.FirstOrDefault(delegate(Project item) { return item.Id == mov.ProjectId; });
                    if (p != null) mov.ProjectName = p.ProjectName;

                    if (mov.RecordedBy.HasValue)
                    {
                        User u = _store.Users.FirstOrDefault(delegate(User usr) { return usr.Id == mov.RecordedBy.Value; });
                        if (u != null) mov.RecordedByName = u.FullName;
                    }
                }
                return list;
            }
        }

        public List<MaterialUsage> GetUsages(int projectId, DateTime? startDate, DateTime? endDate)
        {
            lock (_store.SyncRoot)
            {
                IEnumerable<MaterialUsage> query = _store.MaterialUsages
                    .Where(delegate(MaterialUsage u) { return u.ProjectId == projectId; });

                if (startDate.HasValue) query = query.Where(delegate(MaterialUsage u) { return u.UsageDate >= startDate.Value; });
                if (endDate.HasValue) query = query.Where(delegate(MaterialUsage u) { return u.UsageDate <= endDate.Value; });

                List<MaterialUsage> list = query.OrderByDescending(delegate(MaterialUsage u) { return u.UsageDate; }).ToList();
                foreach (MaterialUsage u in list)
                {
                    Material mat = _store.Materials.FirstOrDefault(delegate(Material m) { return m.Id == u.MaterialId; });
                    if (mat != null)
                    {
                        u.MaterialName = mat.MaterialName;
                        u.Unit = mat.Unit;
                    }
                }
                return list;
            }
        }

        public List<MaterialPurchase> GetPurchases(int projectId, DateTime? startDate, DateTime? endDate)
        {
            lock (_store.SyncRoot)
            {
                IEnumerable<MaterialPurchase> query = _store.MaterialPurchases
                    .Where(delegate(MaterialPurchase p) { return p.ProjectId == projectId; });

                if (startDate.HasValue) query = query.Where(delegate(MaterialPurchase p) { return p.PurchaseDate >= startDate.Value; });
                if (endDate.HasValue) query = query.Where(delegate(MaterialPurchase p) { return p.PurchaseDate <= endDate.Value; });

                List<MaterialPurchase> list = query.OrderByDescending(delegate(MaterialPurchase p) { return p.PurchaseDate; }).ToList();
                foreach (MaterialPurchase p in list)
                {
                    Material mat = _store.Materials.FirstOrDefault(delegate(Material m) { return m.Id == p.MaterialId; });
                    if (mat != null)
                    {
                        p.MaterialName = mat.MaterialName;
                        p.Unit = mat.Unit;
                    }
                }
                return list;
            }
        }

        private void EnrichStock(ProjectMaterialStock s)
        {
            Material mat = _store.Materials.FirstOrDefault(delegate(Material m) { return m.Id == s.MaterialId; });
            if (mat != null)
            {
                s.MaterialName = mat.MaterialName;
                s.Unit = mat.Unit;
                MaterialCategory cat = _store.MaterialCategories.FirstOrDefault(delegate(MaterialCategory c) { return c.Id == mat.CategoryId; });
                if (cat != null) s.CategoryName = cat.CategoryName;
            }
            Project proj = _store.Projects.FirstOrDefault(delegate(Project p) { return p.Id == s.ProjectId; });
            if (proj != null) s.ProjectName = proj.ProjectName;
        }
    }
}

