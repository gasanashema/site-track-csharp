using System;
using System.Collections.Generic;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Repositories;

namespace SiteTrack.Controllers
{
    public class StockController : BaseController
    {
        private readonly IStockRepository _stockRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IMaterialRepository _materialRepository;

        public StockController() : this(new StockRepository(), new ProjectRepository(), new MaterialRepository())
        {
        }

        public StockController(IStockRepository stockRepository, IProjectRepository projectRepository, IMaterialRepository materialRepository)
        {
            this._stockRepository = stockRepository ?? new StockRepository();
            this._projectRepository = projectRepository ?? new ProjectRepository();
            this._materialRepository = materialRepository ?? new MaterialRepository();
        }

        public bool CanAccessProject(int projectId)
        {
            if (IsAdmin) return true;
            if (!CurrentUserId.HasValue) return false;

            List<Project> assigned = _projectRepository.GetByManager(CurrentUserId.Value);
            return assigned.Exists(delegate(Project p) { return p.Id == projectId; });
        }

        public List<ProjectMaterialStock> GetProjectStock(int projectId)
        {
            if (!CanAccessProject(projectId))
            {
                return new List<ProjectMaterialStock>();
            }
            return _stockRepository.GetStockByProject(projectId);
        }

        public ProjectMaterialStock GetMaterialStock(int projectId, int materialId)
        {
            if (!CanAccessProject(projectId)) return null;
            return _stockRepository.GetStock(projectId, materialId);
        }

        public Result<int> RecordPurchase(int projectId, int materialId, decimal quantity, decimal unitPrice, string supplierName, DateTime purchaseDate)
        {
            if (!CanAccessProject(projectId))
            {
                return Result<int>.Failure("Access Denied: You are not assigned to record transactions for this project.");
            }

            if (quantity <= 0)
            {
                return Result<int>.Failure("Purchase quantity must be greater than zero.");
            }

            if (unitPrice < 0)
            {
                return Result<int>.Failure("Unit price cannot be negative.");
            }

            if (string.IsNullOrEmpty(supplierName) || string.IsNullOrEmpty(supplierName.Trim()))
            {
                return Result<int>.Failure("Supplier name is required.");
            }

            Material material = _materialRepository.GetMaterialById(materialId);
            if (material == null || !string.Equals(material.Status, "ACTIVE", StringComparison.OrdinalIgnoreCase))
            {
                return Result<int>.Failure("Selected material is invalid or inactive.");
            }

            MaterialPurchase purchase = new MaterialPurchase
            {
                ProjectId = projectId,
                MaterialId = materialId,
                Quantity = quantity,
                UnitPrice = unitPrice,
                TotalPrice = quantity * unitPrice,
                SupplierName = supplierName.Trim(),
                PurchaseDate = purchaseDate.Date,
                RecordedBy = CurrentUserId
            };

            try
            {
                int id = _stockRepository.RecordPurchase(purchase);
                return Result<int>.Success(id);
            }
            catch (ValidationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Error recording purchase: " + ex.Message);
            }
        }

        public List<MaterialPurchase> GetPurchases(int projectId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (!CanAccessProject(projectId)) return new List<MaterialPurchase>();
            return _stockRepository.GetPurchases(projectId, startDate, endDate);
        }

        public List<MaterialStockMovement> GetMovements(int projectId, int? materialId = null)
        {
            if (!CanAccessProject(projectId)) return new List<MaterialStockMovement>();
            return _stockRepository.GetMovements(projectId, materialId);
        }

        public Result RecordStockAdjustment(int projectId, int materialId, decimal newQuantity, string reason)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can record physical inventory stock adjustments.");
            }

            if (newQuantity < 0)
            {
                return Result.Failure("Adjusted stock quantity cannot be negative.");
            }

            if (string.IsNullOrEmpty(reason) || string.IsNullOrEmpty(reason.Trim()))
            {
                return Result.Failure("Reason for stock adjustment is required (e.g. Physical inventory count).");
            }

            try
            {
                _stockRepository.RecordAdjustment(projectId, materialId, newQuantity, reason.Trim(), CurrentUserId.HasValue ? CurrentUserId.Value : 1);
                return Result.Success();
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error recording stock adjustment: " + ex.Message);
            }
        }
    }
}
