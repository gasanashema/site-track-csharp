using System;
using System.Collections.Generic;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Repositories;

namespace SiteTrack.Controllers
{
    public class MaterialController : BaseController
    {
        private readonly IMaterialRepository _materialRepository;

        public MaterialController() : this(new MaterialRepository())
        {
        }

        public MaterialController(IMaterialRepository materialRepository)
        {
            this._materialRepository = materialRepository ?? new MaterialRepository();
        }

        // --- Categories ---

        public List<MaterialCategory> GetAllCategories()
        {
            return _materialRepository.GetAllCategories();
        }

        public MaterialCategory GetCategoryById(int id)
        {
            return _materialRepository.GetCategoryById(id);
        }

        public Result<int> CreateCategory(string name, string description)
        {
            if (!IsAdmin)
            {
                return Result<int>.Failure("Access Denied: Only Administrators can create material categories.");
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                return Result<int>.Failure("Category name is required.");
            }

            MaterialCategory cat = new MaterialCategory
            {
                CategoryName = name.Trim(),
                Description = description != null ? description.Trim() : string.Empty
            };

            try
            {
                int id = _materialRepository.InsertCategory(cat);
                return Result<int>.Success(id);
            }
            catch (ValidationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Error creating category: " + ex.Message);
            }
        }

        public Result UpdateCategory(int id, string name, string description)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can modify material categories.");
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                return Result.Failure("Category name is required.");
            }

            MaterialCategory existing = _materialRepository.GetCategoryById(id);
            if (existing == null)
            {
                return Result.Failure("Category not found.");
            }

            existing.CategoryName = name.Trim();
            existing.Description = description != null ? description.Trim() : string.Empty;

            try
            {
                bool success = _materialRepository.UpdateCategory(existing);
                return success ? Result.Success() : Result.Failure("Failed to update category.");
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error updating category: " + ex.Message);
            }
        }

        public Result DeleteCategory(int id)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can delete material categories.");
            }

            try
            {
                bool deleted = _materialRepository.DeleteCategory(id);
                return deleted ? Result.Success() : Result.Failure("Category not found or could not be deleted.");
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error deleting category: " + ex.Message);
            }
        }

        // --- Materials ---

        public List<Material> GetAllMaterials(int? categoryId = null)
        {
            return _materialRepository.GetAllMaterials(categoryId);
        }

        public Material GetMaterialById(int id)
        {
            return _materialRepository.GetMaterialById(id);
        }

        public Result<int> CreateMaterial(int categoryId, string name, string unit, decimal currentPrice, string description)
        {
            if (!IsAdmin)
            {
                return Result<int>.Failure("Access Denied: Only Administrators can create master materials.");
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                return Result<int>.Failure("Material name is required.");
            }

            if (string.IsNullOrEmpty(unit) || string.IsNullOrEmpty(unit.Trim()))
            {
                return Result<int>.Failure("Unit of measurement is required (e.g. Bags, Tons, Pieces).");
            }

            if (currentPrice < 0)
            {
                return Result<int>.Failure("Reference price cannot be negative.");
            }

            Material mat = new Material
            {
                CategoryId = categoryId,
                MaterialName = name.Trim(),
                Unit = unit.Trim(),
                CurrentPrice = currentPrice,
                Description = description != null ? description.Trim() : string.Empty,
                Status = "ACTIVE"
            };

            try
            {
                int id = _materialRepository.InsertMaterial(mat);
                return Result<int>.Success(id);
            }
            catch (ValidationException ex)
            {
                return Result<int>.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result<int>.Failure("Error creating material: " + ex.Message);
            }
        }

        public Result UpdateMaterial(int id, int categoryId, string name, string unit, decimal currentPrice, string description, string status)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can modify master materials.");
            }

            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                return Result.Failure("Material name is required.");
            }

            if (string.IsNullOrEmpty(unit) || string.IsNullOrEmpty(unit.Trim()))
            {
                return Result.Failure("Unit of measurement is required.");
            }

            if (currentPrice < 0)
            {
                return Result.Failure("Reference price cannot be negative.");
            }

            Material existing = _materialRepository.GetMaterialById(id);
            if (existing == null)
            {
                return Result.Failure("Material not found.");
            }

            existing.CategoryId = categoryId;
            existing.MaterialName = name.Trim();
            existing.Unit = unit.Trim();
            existing.CurrentPrice = currentPrice;
            existing.Description = description != null ? description.Trim() : string.Empty;
            existing.Status = status ?? existing.Status;

            try
            {
                bool success = _materialRepository.UpdateMaterial(existing);
                return success ? Result.Success() : Result.Failure("Failed to update material.");
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error updating material: " + ex.Message);
            }
        }

        public Result DeleteMaterial(int id)
        {
            if (!IsAdmin)
            {
                return Result.Failure("Access Denied: Only Administrators can delete master materials.");
            }

            try
            {
                bool deleted = _materialRepository.DeleteMaterial(id);
                return deleted ? Result.Success() : Result.Failure("Material not found or could not be deleted.");
            }
            catch (ValidationException ex)
            {
                return Result.Failure(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure("Error deleting material: " + ex.Message);
            }
        }
    }
}
