using System;
using System.Collections.Generic;
using SiteTrack.Controllers;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Core.Security;
using SiteTrack.Data.Connection;
using SiteTrack.Data.Repositories;

namespace SiteTrack.Tests
{
    class Program
    {
        private static int _passedCount = 0;
        private static int _failedCount = 0;

        static int Main(string[] args)
        {
            Console.WriteLine("=================================================");
            Console.WriteLine("SiteTrack Construction Manager — Test Suite Runner");
            Console.WriteLine("=================================================");

            // --- Phase 1 Model Tests ---
            Test("User Default Status is ACTIVE", delegate {
                User user = new User();
                Assert(user.IsActive, "User should default to ACTIVE");
                Assert(user.Role == "SiteManager", "Default role should be SiteManager");
            });

            Test("Project Default Status is Planning", delegate {
                Project proj = new Project();
                Assert(proj.Status == "Planning", "Project default status should be Planning");
            });

            Test("Stock Low Stock Indicator", delegate {
                ProjectMaterialStock stock = new ProjectMaterialStock();
                stock.MinimumQuantity = 10m;
                stock.QuantityAvailable = 5m;
                Assert(stock.IsLowStock, "Stock <= MinimumQuantity should trigger IsLowStock");

                stock.QuantityAvailable = 15m;
                Assert(!stock.IsLowStock, "Stock > MinimumQuantity should not trigger IsLowStock");
            });

            Test("Weighted Average Unit Price Calculation", delegate {
                decimal currentQty = 10m;
                decimal currentAvgPrice = 50m;
                decimal newQty = 20m;
                decimal newPrice = 80m;

                decimal totalCost = (currentQty * currentAvgPrice) + (newQty * newPrice);
                decimal totalQty = currentQty + newQty;
                decimal newAvg = totalCost / totalQty;

                Assert(newAvg == 70m, string.Format("Expected 70, got {0}", newAvg));
            });

            Test("Worker Attendance PRESENT status gate", delegate {
                WorkerAttendance attPresent = new WorkerAttendance();
                attPresent.AttendanceStatus = "PRESENT";
                Assert(attPresent.IsPresent, "Attendance PRESENT should evaluate IsPresent == true");

                WorkerAttendance attAbsent = new WorkerAttendance();
                attAbsent.AttendanceStatus = "ABSENT";
                Assert(!attAbsent.IsPresent, "Attendance ABSENT should evaluate IsPresent == false");
            });

            Test("Worker Payment Balance Due Calculation", delegate {
                WorkerPayment payment = new WorkerPayment();
                payment.AmountOwed = 5000m;
                payment.AmountPaid = 2000m;
                Assert(payment.BalanceDue == 3000m, "BalanceDue should equal AmountOwed - AmountPaid");
                Assert(!payment.IsFullyPaid, "Should not be fully paid when balance remains");

                payment.AmountPaid = 5000m;
                Assert(payment.IsFullyPaid, "Should be fully paid when AmountPaid >= AmountOwed");
            });

            Test("Result and Result<T> contract", delegate {
                Result ok = Result.Success();
                Assert(ok.IsSuccess, "Success result should have IsSuccess == true");

                Result fail = Result.Failure("Invalid input");
                Assert(!fail.IsSuccess, "Failure result should have IsSuccess == false");
                Assert(fail.ErrorMessage == "Invalid input", "ErrorMessage should match");

                Result<int> valResult = Result<int>.Success(42);
                Assert(valResult.IsSuccess && valResult.Value == 42, "Generic result holds value");
            });

            // --- Phase 2 Data Access & Repository Tests ---
            Test("Data Layer: Connection Factory TestConnection", delegate {
                IDbConnectionFactory factory = new DbConnectionFactory();
                bool connected = factory.TestConnection();
                Assert(connected, "DbConnectionFactory.TestConnection() should return true");
            });

            Test("Data Layer: Initial Seed Data is Loaded", delegate {
                IUserRepository userRepo = new UserRepository();
                User admin = userRepo.GetByUsername("admin");
                Assert(admin != null, "Admin user must exist in seed data");
                Assert(admin.IsAdmin, "Admin user must have Administrator role");

                User siteMgr = userRepo.GetByUsername("sitemgr");
                Assert(siteMgr != null, "Site manager must exist in seed data");

                IProjectRepository projRepo = new ProjectRepository();
                List<Project> projs = projRepo.GetAll();
                Assert(projs.Count >= 3, "Seed data must contain at least 3 sample projects");

                IMaterialRepository matRepo = new MaterialRepository();
                List<MaterialCategory> cats = matRepo.GetAllCategories();
                Assert(cats.Count >= 6, "Seed data must contain at least 6 material categories");

                ILaborRepository laborRepo = new LaborRepository();
                List<WorkerType> types = laborRepo.GetAllWorkerTypes();
                Assert(types.Count >= 6, "Seed data must contain at least 6 worker types");
            });

            Test("Data Layer: User Repository Duplicate Username/Email Guard", delegate {
                IUserRepository repo = new UserRepository();
                bool threw = false;
                try
                {
                    repo.Insert(new User { Username = "admin", Email = "unique@sitetrack.rw", FullName = "Dup Admin" });
                }
                catch (ValidationException)
                {
                    threw = true;
                }
                Assert(threw, "Inserting duplicate username must throw ValidationException");
            });

            Test("Data Layer: Project Scoping by Site Manager", delegate {
                IUserRepository userRepo = new UserRepository();
                User siteMgr = userRepo.GetByUsername("sitemgr");

                IProjectRepository projRepo = new ProjectRepository();
                List<Project> scopedProjs = projRepo.GetByManager(siteMgr.Id);
                Assert(scopedProjs.Count > 0, "Site manager must have assigned projects");
                Assert(scopedProjs.Count < projRepo.GetAll().Count, "Site manager must not see all projects across the company");
            });

            Test("Data Layer: Stock Purchase and Weighted Average Price Calculation", delegate {
                IStockRepository stockRepo = new StockRepository();
                int projId = 2;
                int matId = 1;

                stockRepo.RecordPurchase(new MaterialPurchase {
                    ProjectId = projId,
                    MaterialId = matId,
                    Quantity = 50m,
                    UnitPrice = 12000m,
                    SupplierName = "Cimerwa Direct",
                    PurchaseDate = DateTime.Today,
                    RecordedBy = 1
                });

                ProjectMaterialStock s1 = stockRepo.GetStock(projId, matId);
                Assert(s1.QuantityAvailable >= 50m, "Stock quantity should reflect purchase");
            });

            Test("Data Layer: Stock Usage Depletion Rejection Guard (Business Rule 1)", delegate {
                IStockRepository stockRepo = new StockRepository();
                int projId = 2;
                int matId = 1;

                ProjectMaterialStock current = stockRepo.GetStock(projId, matId);
                decimal excessQty = current.QuantityAvailable + 500m;

                bool rejected = false;
                try
                {
                    stockRepo.RecordUsage(new MaterialUsage {
                        ProjectId = projId,
                        MaterialId = matId,
                        QuantityUsed = excessQty,
                        UsageDate = DateTime.Today,
                        ActivityDescription = "Excessive concrete pouring",
                        RecordedBy = 2
                    });
                }
                catch (ValidationException ex)
                {
                    rejected = true;
                    Assert(ex.Message.Contains("Insufficient stock"), "Exception message should indicate insufficient stock");
                }
                Assert(rejected, "Material usage exceeding available stock must be rejected");
            });

            Test("Data Layer: Attendance-Gated Payment Creation (Business Rule 2)", delegate {
                ILaborRepository laborRepo = new LaborRepository();
                int projId = 1;
                int workerId = 1;
                DateTime testDate = new DateTime(2026, 11, 10);

                int attId = laborRepo.SaveAttendance(new WorkerAttendance {
                    ProjectId = projId,
                    WorkerId = workerId,
                    WorkDate = testDate,
                    AttendanceStatus = "ABSENT",
                    WorkDescription = "Sick leave",
                    RecordedBy = 2
                });

                bool paymentBlocked = false;
                try
                {
                    laborRepo.SavePayment(new WorkerPayment {
                        ProjectId = projId,
                        WorkerId = workerId,
                        AttendanceId = attId,
                        WorkDate = testDate,
                        DailyRate = 15000m,
                        AmountOwed = 15000m,
                        AmountPaid = 15000m,
                        PaymentStatus = "PAID",
                        PaidBy = 2
                    });
                }
                catch (ValidationException ex)
                {
                    paymentBlocked = true;
                    Assert(ex.Message.Contains("PRESENT"), "Error message should require PRESENT attendance status");
                }
                Assert(paymentBlocked, "Creating payment for ABSENT worker must be blocked");
            });

            // --- Phase 3 Authentication & Security Tests ---
            Test("Security: PasswordHasher Computes Consistent SHA-256 Hash", delegate {
                string hash = PasswordHasher.HashPassword("Admin@123");
                Assert(hash == "e86f78a8a3caf0b60d8e74e5942aa6d86dc150cd3c03338aef25b7d2d7e3acc7", "SHA-256 hash must match standard vector");
                Assert(PasswordHasher.VerifyPassword("Admin@123", hash), "Password verification must succeed for correct password");
                Assert(!PasswordHasher.VerifyPassword("WrongPassword", hash), "Password verification must fail for incorrect password");
            });

            Test("Auth: Controller Rejects Missing Username or Password", delegate {
                AuthController auth = new AuthController();
                Result<User> r1 = auth.Login("", "Admin@123");
                Assert(!r1.IsSuccess && r1.ErrorMessage.Contains("Username"), "Missing username must fail");

                Result<User> r2 = auth.Login("admin", "");
                Assert(!r2.IsSuccess && r2.ErrorMessage.Contains("Password"), "Missing password must fail");
            });

            Test("Auth: Controller Rejects Nonexistent User", delegate {
                AuthController auth = new AuthController();
                Result<User> r = auth.Login("ghost_user_999", "Admin@123");
                Assert(!r.IsSuccess && r.ErrorMessage.Contains("Invalid"), "Nonexistent user must fail with generic error");
            });

            Test("Auth: Controller Rejects Wrong Password", delegate {
                AuthController auth = new AuthController();
                Result<User> r = auth.Login("admin", "WrongPass123!");
                Assert(!r.IsSuccess && r.ErrorMessage.Contains("Invalid"), "Wrong password must fail");
            });

            Test("Auth: Controller Blocks Deactivated Accounts", delegate {
                IUserRepository userRepo = new UserRepository();
                int inactiveId = userRepo.Insert(new User {
                    FullName = "Disabled Worker",
                    Username = "disabled_user",
                    Email = "disabled@sitetrack.rw",
                    Password = PasswordHasher.HashPassword("Admin@123"),
                    Role = "SiteManager",
                    Status = "INACTIVE"
                });

                AuthController auth = new AuthController(userRepo);
                Result<User> r = auth.Login("disabled_user", "Admin@123");
                Assert(!r.IsSuccess && r.ErrorMessage.Contains("deactivated"), "Deactivated user must be blocked with explicit notice");
            });

            Test("Auth: Successful Admin Authentication Sets Session and Role", delegate {
                AuthController auth = new AuthController();
                Result<User> r = auth.Login("admin", "Admin@123");
                Assert(r.IsSuccess, "Admin login must succeed with correct credentials");
                Assert(r.Value != null && r.Value.IsAdmin, "Authenticated user must have Admin role");
                Assert(BaseController.IsAuthenticated, "BaseController.IsAuthenticated must be true");
                Assert(BaseController.IsAdmin, "BaseController.IsAdmin must be true");
            });

            Test("Auth: Successful Site Manager Authentication Sets Scoped Role", delegate {
                AuthController auth = new AuthController();
                Result<User> r = auth.Login("sitemgr", "Admin@123");
                Assert(r.IsSuccess, "Site Manager login must succeed with correct credentials");
                Assert(!r.Value.IsAdmin, "Site Manager must NOT have Admin role");
                Assert(BaseController.IsAuthenticated, "BaseController.IsAuthenticated must be true");
                Assert(!BaseController.IsAdmin, "BaseController.IsAdmin must be false for site manager");
            });

            Test("Auth: Logout Clears Application Session", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");
                Assert(BaseController.IsAuthenticated, "Must be authenticated before logout");

                auth.Logout();
                Assert(!BaseController.IsAuthenticated, "Session must be cleared after logout");
                Assert(BaseController.CurrentUser == null, "CurrentUser must be null after logout");
            });

            // --- Phase 4 User Management & Project Administration Tests ---
            Test("User Controller: Admin Can Create New User", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                UserController userCtrl = new UserController();
                Result<int> res = userCtrl.CreateUser("Jean Claude Murwanashyaka", "jcmurwa", "jcmurwa@sitetrack.rw", "Secret@2026", "SiteManager", "+250788555111");
                Assert(res.IsSuccess, "Admin should be able to create user");
                Assert(res.Value > 0, "Created user should have valid positive ID");
            });

            Test("User Controller: Non-Admin Blocked from Creating User", delegate {
                AuthController auth = new AuthController();
                auth.Login("sitemgr", "Admin@123");

                UserController userCtrl = new UserController();
                Result<int> res = userCtrl.CreateUser("Unauthorized Guy", "unauth", "unauth@sitetrack.rw", "Secret@2026", "SiteManager", "+250788999999");
                Assert(!res.IsSuccess, "Site Manager must be blocked from creating users");
                Assert(res.ErrorMessage.Contains("Access Denied"), "Error should state Access Denied");
            });

            Test("User Controller: Reject Short Password (< 6 chars)", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                UserController userCtrl = new UserController();
                Result<int> res = userCtrl.CreateUser("Short Pass", "shortpass", "short@sitetrack.rw", "123", "SiteManager", "+250788000000");
                Assert(!res.IsSuccess, "Password shorter than 6 characters must be rejected");
            });

            Test("User Controller: Prevent Self-Deactivation of Logged-in Admin", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                UserController userCtrl = new UserController();
                Result res = userCtrl.ToggleUserStatus(BaseController.CurrentUser.Id);
                Assert(!res.IsSuccess, "User cannot deactivate their own active account");
                Assert(res.ErrorMessage.Contains("Blocked"), "Error message should state operation blocked");
            });

            Test("Project Controller: Admin Can Create Project with Valid Dates", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ProjectController projCtrl = new ProjectController();
                Result<int> res = projCtrl.CreateProject(
                    "Bugesera International Airport Access Road",
                    "Bugesera District",
                    "Dual-carriageway expressway link construction.",
                    DateTime.Today,
                    DateTime.Today.AddYears(1),
                    "Planning"
                );
                Assert(res.IsSuccess, "Admin should be able to create project with valid dates");
                Assert(res.Value > 0, "New project should have valid ID");
            });

            Test("Project Controller: Technical Rule 3 Rejects EndDate Before StartDate", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ProjectController projCtrl = new ProjectController();
                Result<int> res = projCtrl.CreateProject(
                    "Invalid Timeline Road",
                    "Kigali",
                    "Faulty project.",
                    DateTime.Today,
                    DateTime.Today.AddDays(-10),
                    "Planning"
                );
                Assert(!res.IsSuccess, "Project with EndDate earlier than StartDate must be rejected");
                Assert(res.ErrorMessage.Contains("earlier"), "Error message should mention earlier end date");
            });

            Test("Project Controller: Admin Can Assign and Remove Site Manager", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ProjectController projCtrl = new ProjectController();
                int projId = 1;
                int userId = 3;

                Result assignRes = projCtrl.AssignManager(projId, userId);
                Assert(assignRes.IsSuccess, "Assigning manager should succeed");

                List<ProjectManager> pms = projCtrl.GetProjectManagers(projId);
                Assert(pms.Exists(delegate(ProjectManager pm) { return pm.UserId == userId; }), "User must appear in project managers");

                Result removeRes = projCtrl.RemoveManager(projId, userId);
                Assert(removeRes.IsSuccess, "Removing manager should succeed");
            });

            Test("Project Controller: Business Rule 5 Blocks Deleting Project with Active Child Records", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ProjectController projCtrl = new ProjectController();
                int activeProjId = 1;

                Result res = projCtrl.DeleteProject(activeProjId);
                Assert(!res.IsSuccess, "Deleting project with active child records must be blocked");
                Assert(res.ErrorMessage.Contains("Active material stock") || res.ErrorMessage.Contains("Cannot delete"), "Error should explain child record guard");
            });

            // --- Phase 5 Material Catalogs & Stock Tracking Tests ---
            Test("Material Controller: Admin Can Create Material Category", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                MaterialController matCtrl = new MaterialController();
                Result<int> res = matCtrl.CreateCategory("Roofing & Trusses", "Corrugated iron sheets, timber trusses, ridge caps");
                Assert(res.IsSuccess, "Admin should be able to create category");
                Assert(res.Value > 0, "Created category should have valid ID");
            });

            Test("Material Controller: Non-Admin Blocked from Creating Category", delegate {
                AuthController auth = new AuthController();
                auth.Login("sitemgr", "Admin@123");

                MaterialController matCtrl = new MaterialController();
                Result<int> res = matCtrl.CreateCategory("Unauthorized Category", "Description");
                Assert(!res.IsSuccess, "Site Manager must be blocked from creating categories");
                Assert(res.ErrorMessage.Contains("Access Denied"), "Error should state Access Denied");
            });

            Test("Material Controller: Block Deleting Category with Linked Materials", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                MaterialController matCtrl = new MaterialController();
                Result res = matCtrl.DeleteCategory(1); // Category 1 has Dangote Cement linked!
                Assert(!res.IsSuccess, "Deleting category with linked materials must be blocked");
                Assert(res.ErrorMessage.Contains("linked"), "Error message should mention linked materials");
            });

            Test("Material Controller: Admin Can Create Master Material with Positive Price", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                MaterialController matCtrl = new MaterialController();
                Result<int> res = matCtrl.CreateMaterial(1, "Rapid-Hardening Cement 52.5N", "Bags", 15500m, "Fast-setting high-strength cement");
                Assert(res.IsSuccess, "Admin should be able to create master material");
                Assert(res.Value > 0, "New material should have positive ID");
            });

            Test("Material Controller: Reject Material with Negative Price", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                MaterialController matCtrl = new MaterialController();
                Result<int> res = matCtrl.CreateMaterial(1, "Negative Price Sand", "Tons", -500m, "Invalid price");
                Assert(!res.IsSuccess, "Material with negative price must be rejected");
            });

            Test("Stock Controller: Purchase Atomically Updates Stock and Calculates Weighted Average", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                StockController stockCtrl = new StockController();
                int projId = 3; // Nyagatare Modern Secondary School
                int matId = 2;  // Clean River Sand (Starts at 0 stock)

                // 1. First delivery: 20 tons @ 15,000 RWF
                Result<int> p1 = stockCtrl.RecordPurchase(projId, matId, 20m, 15000m, "Nyabarongo Sand Quarries", DateTime.Today);
                Assert(p1.IsSuccess, "First purchase should succeed");

                ProjectMaterialStock s1 = stockCtrl.GetMaterialStock(projId, matId);
                Assert(s1.QuantityAvailable == 20m, "Available quantity should be 20");
                Assert(s1.AverageUnitPrice == 15000m, "Average unit price should be 15,000");

                // 2. Second delivery: 10 tons @ 18,000 RWF
                // Weighted average: ((20 * 15000) + (10 * 18000)) / (20 + 10) = (300,000 + 180,000) / 30 = 480,000 / 30 = 16,000 RWF
                Result<int> p2 = stockCtrl.RecordPurchase(projId, matId, 10m, 18000m, "Akagera Aggregates", DateTime.Today);
                Assert(p2.IsSuccess, "Second purchase should succeed");

                ProjectMaterialStock s2 = stockCtrl.GetMaterialStock(projId, matId);
                Assert(s2.QuantityAvailable == 30m, "Available quantity should be 30");
                Assert(s2.AverageUnitPrice == 16000m, string.Format("Weighted average should be 16,000 RWF, got {0}", s2.AverageUnitPrice));

                // 3. Verify IN movement audit trail
                List<MaterialStockMovement> movs = stockCtrl.GetMovements(projId, matId);
                Assert(movs.Count >= 2, "Stock movements must have at least 2 entries");
                Assert(movs[0].MovementType == "IN", "Movement type must be IN");
            });

            Test("Stock Controller: Site Manager Cannot Purchase For Unassigned Project", delegate {
                AuthController auth = new AuthController();
                auth.Login("sitemgr", "Admin@123"); // sitemgr is assigned to project 1 and 2, but NOT project 3

                StockController stockCtrl = new StockController();
                Result<int> res = stockCtrl.RecordPurchase(3, 1, 10m, 12000m, "Illegal Supplier", DateTime.Today);
                Assert(!res.IsSuccess, "Site manager should not be allowed to record purchases for unassigned project");
                Assert(res.ErrorMessage.Contains("Access Denied"), "Error message should indicate Access Denied");
            });

            Test("Stock Controller: Low Stock Detection on Project Stock", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                StockController stockCtrl = new StockController();
                int projId = 1; // Kigali Heights
                List<ProjectMaterialStock> stocks = stockCtrl.GetProjectStock(projId);
                Assert(stocks.Count > 0, "Project 1 must have stock items");

                // Check stock low stock calculation
                foreach (ProjectMaterialStock s in stocks)
                {
                    bool calculatedLow = s.QuantityAvailable <= s.MinimumQuantity;
                    Assert(s.IsLowStock == calculatedLow, "IsLowStock property must match QuantityAvailable <= MinimumQuantity");
                }
            });

            Test("Usage Controller: Valid Usage Decrements Stock and Records OUT Movement", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                StockController stockCtrl = new StockController();
                int projId = 1;
                int matId = 1; // Cement

                ProjectMaterialStock initialStock = stockCtrl.GetMaterialStock(projId, matId);
                Assert(initialStock != null && initialStock.QuantityAvailable > 10m, "Initial stock should exist and be > 10");
                decimal initialQty = initialStock.QuantityAvailable;

                Result<int> res = stockCtrl.RecordUsage(projId, matId, 5m, "Ground floor column casting", DateTime.Today);
                Assert(res.IsSuccess, "Recording valid usage should succeed: " + res.ErrorMessage);

                ProjectMaterialStock updatedStock = stockCtrl.GetMaterialStock(projId, matId);
                Assert(updatedStock.QuantityAvailable == initialQty - 5m,
                    string.Format("Quantity should decrement by 5. Expected {0}, got {1}", initialQty - 5m, updatedStock.QuantityAvailable));

                List<MaterialStockMovement> movs = stockCtrl.GetMovements(projId, matId);
                MaterialStockMovement lastOut = movs.Find(delegate(MaterialStockMovement m) {
                    return m.MovementType == "OUT" && m.Description.Contains("Ground floor column casting");
                });
                Assert(lastOut != null, "An OUT movement record must be created in movements ledger");
                Assert(lastOut.Quantity == 5m, "Movement quantity must match usage quantity (5)");
            });

            Test("Usage Controller: Business Rule 1 Rejects Usage Exceeding Stock", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                StockController stockCtrl = new StockController();
                int projId = 1;
                int matId = 1;

                ProjectMaterialStock currentStock = stockCtrl.GetMaterialStock(projId, matId);
                decimal excessiveQty = currentStock.QuantityAvailable + 1000m;

                Result<int> res = stockCtrl.RecordUsage(projId, matId, excessiveQty, "Excessive usage attempt", DateTime.Today);
                Assert(!res.IsSuccess, "Business Rule 1 must reject usage when quantity exceeds stock");
                Assert(res.ErrorMessage.Contains("Business Rule 1") || res.ErrorMessage.Contains("Insufficient stock"),
                    "Error message must specify Business Rule 1 or insufficient stock");
            });

            Test("Usage Controller: Reject Non-Positive Usage Quantity", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                StockController stockCtrl = new StockController();
                Result<int> resZero = stockCtrl.RecordUsage(1, 1, 0m, "Zero quantity test", DateTime.Today);
                Assert(!resZero.IsSuccess, "Usage with zero quantity must be rejected");

                Result<int> resNeg = stockCtrl.RecordUsage(1, 1, -15m, "Negative quantity test", DateTime.Today);
                Assert(!resNeg.IsSuccess, "Usage with negative quantity must be rejected");
            });

            Test("Usage Controller: Reject Empty Activity Description", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                StockController stockCtrl = new StockController();
                Result<int> resEmpty = stockCtrl.RecordUsage(1, 1, 2m, "   ", DateTime.Today);
                Assert(!resEmpty.IsSuccess, "Usage without activity description must be rejected");
                Assert(resEmpty.ErrorMessage.Contains("Activity description is required"), "Error should require activity description");
            });

            Test("Usage Controller: Site Manager Cannot Record Usage For Unassigned Project", delegate {
                AuthController auth = new AuthController();
                auth.Login("sitemgr", "Admin@123"); // sitemgr assigned to 1 & 2, not 3

                StockController stockCtrl = new StockController();
                Result<int> res = stockCtrl.RecordUsage(3, 1, 1m, "Unassigned project usage", DateTime.Today);
                Assert(!res.IsSuccess, "Site manager should not be allowed to record usage for unassigned project");
                Assert(res.ErrorMessage.Contains("Access Denied"), "Error should indicate Access Denied");
            });

            Test("Stock Adjustment: Admin Can Record Physical Inventory Reconciliation", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                StockController stockCtrl = new StockController();
                int projId = 2; // Kigali Convention Center
                int matId = 2; // Reinforcement Steel

                Result res = stockCtrl.RecordStockAdjustment(projId, matId, 85m, "Physical count audit adjustment");
                Assert(res.IsSuccess, "Admin stock adjustment must succeed: " + res.ErrorMessage);

                ProjectMaterialStock stock = stockCtrl.GetMaterialStock(projId, matId);
                Assert(stock != null && stock.QuantityAvailable == 85m, "Stock balance must reflect physical count of 85");

                List<MaterialStockMovement> movs = stockCtrl.GetMovements(projId, matId);
                MaterialStockMovement adjMov = movs.Find(delegate(MaterialStockMovement m) {
                    return m.MovementType == "ADJUSTMENT" && m.Description.Contains("Physical count audit adjustment");
                });
                Assert(adjMov != null, "Audit trail must record ADJUSTMENT movement");
            });

            Test("Stock Adjustment: Non-Admin Blocked From Recording Stock Adjustment", delegate {
                AuthController auth = new AuthController();
                auth.Login("sitemgr", "Admin@123");

                StockController stockCtrl = new StockController();
                Result res = stockCtrl.RecordStockAdjustment(1, 1, 50m, "Unauthorized adjustment attempt");
                Assert(!res.IsSuccess, "Non-admin must be blocked from stock adjustment");
                Assert(res.ErrorMessage.Contains("Access Denied"), "Error should indicate Access Denied");
            });

            Test("Audit Trail: Stock Movement Ledger Captures IN, OUT, and ADJUSTMENT", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                StockController stockCtrl = new StockController();
                List<MaterialStockMovement> movs = stockCtrl.GetMovements(1);
                Assert(movs.Count > 0, "Movements ledger must contain historical transactions");

                bool hasIn = movs.Exists(delegate(MaterialStockMovement m) { return m.MovementType == "IN"; });
                bool hasOut = movs.Exists(delegate(MaterialStockMovement m) { return m.MovementType == "OUT"; });
                Assert(hasIn, "Movements ledger should include IN movements");
                Assert(hasOut, "Movements ledger should include OUT movements");
            });

            Test("Labor Controller: Admin Can Create Worker Type With Benchmark Rate", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                LaborController laborCtrl = new LaborController();
                Result<int> res = laborCtrl.CreateWorkerType("Scaffolding Specialist", 7500m, "Certified scaffolding erection technician");
                Assert(res.IsSuccess, "Admin creating worker type must succeed: " + res.ErrorMessage);
                Assert(res.Value > 0, "Created worker type ID must be positive");
            });

            Test("Labor Controller: Non-Admin Blocked From Creating Worker Type", delegate {
                AuthController auth = new AuthController();
                auth.Login("sitemgr", "Admin@123");

                LaborController laborCtrl = new LaborController();
                Result<int> res = laborCtrl.CreateWorkerType("Unauthorized Trade", 6000m, "Should fail");
                Assert(!res.IsSuccess, "Non-admin must be blocked from creating worker type");
                Assert(res.ErrorMessage.Contains("Access Denied"), "Error should indicate Access Denied");
            });

            Test("Labor Controller: Reject Duplicate Worker Type Name", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                LaborController laborCtrl = new LaborController();
                Result<int> res = laborCtrl.CreateWorkerType("Scaffolding Specialist", 8000m, "Duplicate of Scaffolding Specialist");
                Assert(!res.IsSuccess, "Duplicate worker type name must be rejected");
                Assert(res.ErrorMessage.Contains("already exists"), "Error message should mention already exists");
            });

            Test("Labor Controller: Admin Can Register Site Worker With Custom Daily Rate", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                LaborController laborCtrl = new LaborController();
                Result<int> res = laborCtrl.CreateWorker(1, "Habimana Jean Pierre", "0788998877", 6500m, "ACTIVE");
                Assert(res.IsSuccess, "Registering site worker should succeed: " + res.ErrorMessage);
                Assert(res.Value > 0, "Created worker ID must be positive");

                SiteWorker w = laborCtrl.GetWorker(res.Value);
                Assert(w != null && w.DailyRate == 6500m, "Worker daily rate should match custom assigned rate (6500)");
            });

            Test("Labor Controller: Record Daily Attendance PRESENT and Upsert Logic", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                LaborController laborCtrl = new LaborController();
                int projId = 1;
                int workerId = 1;
                DateTime today = DateTime.Today;

                Result<int> resPresent = laborCtrl.RecordAttendance(projId, workerId, today, "PRESENT", "Concrete slab formwork preparation");
                Assert(resPresent.IsSuccess, "Logging attendance PRESENT should succeed: " + resPresent.ErrorMessage);

                List<WorkerAttendance> list = laborCtrl.GetAttendance(projId, today);
                WorkerAttendance att = list.Find(delegate(WorkerAttendance a) { return a.WorkerId == workerId; });
                Assert(att != null, "Attendance record must exist for worker 1 today");
                Assert(att.IsPresent, "Attendance status must evaluate to IsPresent == true");
            });

            Test("Labor Controller: Prevent Attendance For Inactive Worker", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                LaborController laborCtrl = new LaborController();
                // Create an inactive worker
                Result<int> resWorker = laborCtrl.CreateWorker(2, "Inactive Laborer", "0781112233", 4000m, "INACTIVE");
                Assert(resWorker.IsSuccess, "Inactive worker creation should succeed");

                Result<int> resAtt = laborCtrl.RecordAttendance(1, resWorker.Value, DateTime.Today, "PRESENT", "Attempting work while inactive");
                Assert(!resAtt.IsSuccess, "Attendance recording for inactive worker must be rejected");
                Assert(resAtt.ErrorMessage.Contains("inactive") || resAtt.ErrorMessage.Contains("Cannot record attendance"),
                    "Error should explain worker is inactive");
            });

            Test("Labor Controller: Business Rule 2 Rejects Wage Payment When Attendance is ABSENT", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                LaborController laborCtrl = new LaborController();
                int projId = 1;
                int workerId = 2; // Another worker
                DateTime targetDate = DateTime.Today.AddDays(-2);

                // Record attendance as ABSENT
                Result<int> resAtt = laborCtrl.RecordAttendance(projId, workerId, targetDate, "ABSENT", "Sick leave / absent");
                Assert(resAtt.IsSuccess, "Recording ABSENT attendance should succeed");

                // Try to create payment for ABSENT worker
                Result<int> resPay = laborCtrl.RecordPayment(projId, workerId, resAtt.Value, targetDate, 5000m, 5000m, "Payment attempt for absent worker");
                Assert(!resPay.IsSuccess, "Business Rule 2 must strictly block payment creation for absent worker");
                Assert(resPay.ErrorMessage.Contains("Business Rule 2") || resPay.ErrorMessage.Contains("PRESENT"),
                    "Error message must specify Business Rule 2 or PRESENT requirement");
            });

            Test("Labor Controller: Business Rule 2 Allows Wage Payment for PRESENT Worker and Tracks Balance", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                LaborController laborCtrl = new LaborController();
                int projId = 1;
                int workerId = 3;
                DateTime targetDate = DateTime.Today.AddDays(-1);

                // Record attendance as PRESENT
                Result<int> resAtt = laborCtrl.RecordAttendance(projId, workerId, targetDate, "PRESENT", "Wall bricklaying masonry");
                Assert(resAtt.IsSuccess, "Recording PRESENT attendance should succeed");

                // Partial payment: owed 6000, paid 4000 (pending balance 2000)
                decimal dailyRate = 6000m;
                decimal paidAmount = 4000m;
                Result<int> resPay = laborCtrl.RecordPayment(projId, workerId, resAtt.Value, targetDate, dailyRate, paidAmount, "Partial daily wage payout");
                Assert(resPay.IsSuccess, "Payment for PRESENT worker should succeed: " + resPay.ErrorMessage);

                List<WorkerPayment> payments = laborCtrl.GetPayments(projId, targetDate, targetDate);
                WorkerPayment pay = payments.Find(delegate(WorkerPayment p) { return p.AttendanceId == resAtt.Value; });
                Assert(pay != null, "Payment record must exist");
                Assert(pay.AmountOwed == 6000m, "AmountOwed should equal daily rate (6000)");
                Assert(pay.AmountPaid == 4000m, "AmountPaid should equal 4000");
                Assert(pay.BalanceDue == 2000m, "BalanceDue should be 2000");
                Assert(pay.PaymentStatus == "PENDING", "PaymentStatus should be PENDING");

                // Settle remainder
                Result resSettle = laborCtrl.SettlePayment(pay.Id, 6000m, 6000m);
                Assert(resSettle.IsSuccess, "Settling full payment balance should succeed");
            });

            Test("Activity Controller: Admin Can Record Valid Activity and Progress Percentage", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ActivityController actCtrl = new ActivityController();
                Result<int> res = actCtrl.RecordActivity(1, "Foundation Excavation & Compaction", "Substructure earthworks completed to bedrock", DateTime.Today, 25);
                Assert(res.IsSuccess, "Recording valid activity should succeed: " + res.ErrorMessage);
                Assert(res.Value > 0, "Created activity ID must be positive");

                List<ProjectActivity> activities = actCtrl.GetActivities(1);
                ProjectActivity act = activities.Find(delegate(ProjectActivity a) { return a.Id == res.Value; });
                Assert(act != null, "Recorded activity must be retrieved in list");
                Assert(act.ProgressPercentage == 25, "Progress percentage must match 25%");
            });

            Test("Activity Controller: Technical Rule 5 Rejects Negative Progress Percentage (< 0)", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ActivityController actCtrl = new ActivityController();
                Result<int> res = actCtrl.RecordActivity(1, "Invalid Negative Progress", "Testing boundary", DateTime.Today, -15);
                Assert(!res.IsSuccess, "Technical Rule 5 must reject progress percentage < 0");
                Assert(res.ErrorMessage.Contains("Technical Rule 5") || res.ErrorMessage.Contains("between 0 and 100"),
                    "Error message should mention Technical Rule 5 or range between 0 and 100");
            });

            Test("Activity Controller: Technical Rule 5 Rejects Excessive Progress Percentage (> 100)", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ActivityController actCtrl = new ActivityController();
                Result<int> res = actCtrl.RecordActivity(1, "Invalid Over-100 Progress", "Testing upper boundary", DateTime.Today, 110);
                Assert(!res.IsSuccess, "Technical Rule 5 must reject progress percentage > 100");
                Assert(res.ErrorMessage.Contains("Technical Rule 5") || res.ErrorMessage.Contains("between 0 and 100"),
                    "Error message should mention Technical Rule 5 or range between 0 and 100");
            });

            Test("Activity Controller: Technical Rule 5 Accepts Boundary Percentages (0% and 100%)", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ActivityController actCtrl = new ActivityController();
                Result<int> res0 = actCtrl.RecordActivity(1, "Project Kickoff", "Initial site setup", DateTime.Today, 0);
                Assert(res0.IsSuccess, "0% progress boundary should be accepted: " + res0.ErrorMessage);

                Result<int> res100 = actCtrl.RecordActivity(1, "Project Handover Complete", "All civil and finishing works certified", DateTime.Today, 100);
                Assert(res100.IsSuccess, "100% progress boundary should be accepted: " + res100.ErrorMessage);
            });

            Test("Activity Controller: Reject Empty Activity Title", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ActivityController actCtrl = new ActivityController();
                Result<int> res = actCtrl.RecordActivity(1, "   ", "Empty title test", DateTime.Today, 30);
                Assert(!res.IsSuccess, "Empty activity title must be rejected");
                Assert(res.ErrorMessage.Contains("title is required"), "Error message should require title");
            });

            Test("Activity Controller: Site Manager Can Record Activity For Assigned Project", delegate {
                AuthController auth = new AuthController();
                auth.Login("sitemgr", "Admin@123"); // Assigned to projects 1 and 2

                ActivityController actCtrl = new ActivityController();
                Result<int> res = actCtrl.RecordActivity(1, "Column Reinforcement Inspection", "Structural inspection passed", DateTime.Today, 40);
                Assert(res.IsSuccess, "Site manager should record activity for assigned project 1: " + res.ErrorMessage);
            });

            Test("Activity Controller: Site Manager Blocked From Recording Activity For Unassigned Project", delegate {
                AuthController auth = new AuthController();
                auth.Login("sitemgr", "Admin@123"); // NOT assigned to project 3

                ActivityController actCtrl = new ActivityController();
                Result<int> res = actCtrl.RecordActivity(3, "Illegal Site Access", "Should be blocked", DateTime.Today, 50);
                Assert(!res.IsSuccess, "Site manager should be blocked from unassigned project 3");
                Assert(res.ErrorMessage.Contains("Access Denied"), "Error should indicate Access Denied");
            });

            Test("Activity Controller: Timeline Returns Ordered Activities and Latest Progress", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ActivityController actCtrl = new ActivityController();
                List<ProjectActivity> activities = actCtrl.GetActivities(1);
                Assert(activities != null && activities.Count > 0, "Activities list must not be empty");

                int latestProgress = actCtrl.GetLatestProgress(1);
                Assert(latestProgress >= 0 && latestProgress <= 100, "Latest progress must be bounded between 0 and 100");
            });

            Test("Report Controller: Material Consumption Report Aggregates Quantities and Costs", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ReportController repCtrl = new ReportController();
                List<MaterialConsumptionReportItem> report = repCtrl.GetMaterialConsumptionReport(1, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(1));
                Assert(report != null, "Report should not be null");
                Assert(report.Count > 0, "Report should contain recorded usages for project 1");

                foreach (MaterialConsumptionReportItem item in report)
                {
                    Assert(!string.IsNullOrEmpty(item.MaterialName), "MaterialName should not be empty");
                    Assert(item.QuantityUsed > 0, "QuantityUsed should be positive");
                    Assert(item.TotalCost >= 0, "TotalCost should be non-negative");
                }
            });

            Test("Report Controller: Labor Wage Report Returns Present Workers and Correct Balances", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ReportController repCtrl = new ReportController();
                List<LaborWageReportItem> report = repCtrl.GetLaborWageSummaryReport(1, DateTime.Today.AddDays(-30), DateTime.Today.AddDays(1));
                Assert(report != null, "Report should not be null");
                Assert(report.Count > 0, "Report should contain recorded payments for project 1");

                foreach (LaborWageReportItem item in report)
                {
                    Assert(!string.IsNullOrEmpty(item.WorkerName), "WorkerName should not be empty");
                    Assert(item.AmountOwed >= item.AmountPaid, "AmountOwed should be >= AmountPaid");
                    Assert(item.BalanceDue == (item.AmountOwed - item.AmountPaid), "BalanceDue should equal AmountOwed - AmountPaid");
                }
            });

            Test("Report Controller: Stock Movement Audit Report Returns Complete Movement History", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                ReportController repCtrl = new ReportController();
                List<StockMovementReportItem> report = repCtrl.GetStockMovementAuditReport(1, new DateTime(2026, 1, 1), DateTime.Today.AddDays(1));
                Assert(report != null && report.Count > 0, "Movements audit report should have entries");

                bool hasIn = report.Exists(delegate(StockMovementReportItem i) { return i.MovementType == "IN"; });
                bool hasOut = report.Exists(delegate(StockMovementReportItem i) { return i.MovementType == "OUT"; });
                Assert(hasIn, "Audit report should contain IN records");
                Assert(hasOut, "Audit report should contain OUT records");
            });

            Test("Report Controller: Project Executive Summary Computes Totals and Milestones", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                // Record a purchase for project 1 to ensure purchase expenditure exists
                StockController stockCtrl = new StockController();
                stockCtrl.RecordPurchase(1, 1, 10m, 12500m, "Inyange Hardware Ltd", DateTime.Today);

                ReportController repCtrl = new ReportController();
                ProjectExecutiveSummary summary = repCtrl.GetProjectFinancialSummary(1);
                Assert(summary != null, "Project summary should not be null");
                Assert(summary.ProjectId == 1, "ProjectId must match 1");
                Assert(summary.TotalMaterialPurchases > 0, "Total material purchases should be > 0");
                Assert(summary.TotalProjectExpenditure == (summary.TotalMaterialPurchases + summary.TotalLaborWagesPaid),
                    "Total expenditure must equal material purchases + labor wages paid");
            });

            Test("Report Controller: Site Manager Cannot Generate Report For Unassigned Project", delegate {
                AuthController auth = new AuthController();
                auth.Login("sitemgr", "Admin@123"); // Not assigned to project 3

                ReportController repCtrl = new ReportController();
                List<MaterialConsumptionReportItem> report = repCtrl.GetMaterialConsumptionReport(3, DateTime.Today.AddDays(-30), DateTime.Today);
                Assert(report.Count == 0, "Site manager should receive empty report for unassigned project 3");
            });

            Test("CSV Exporter: Format Standard Headers and Rows", delegate {
                List<string> headers = new List<string> { "Project", "Material", "Cost" };
                List<List<string>> rows = new List<List<string>> {
                    new List<string> { "Kigali Heights", "Portland Cement", "12500" },
                    new List<string> { "Convention Center", "Steel Rebar", "26000" }
                };

                string csv = CsvExporter.FormatCsv(headers, rows);
                Assert(csv.Contains("Project,Material,Cost"), "CSV must contain header row");
                Assert(csv.Contains("Kigali Heights,Portland Cement,12500"), "CSV must contain first data row");
                Assert(csv.Contains("Convention Center,Steel Rebar,26000"), "CSV must contain second data row");
            });

            Test("CSV Exporter: Properly Escapes Commas, Quotes, and Newlines", delegate {
                string cellWithComma = CsvExporter.EscapeCell("Sand, River Coarse");
                Assert(cellWithComma == "\"Sand, River Coarse\"", "Cell with comma must be wrapped in quotes");

                string cellWithQuote = CsvExporter.EscapeCell("Steel 12mm \"High-Yield\"");
                Assert(cellWithQuote == "\"Steel 12mm \"\"High-Yield\"\"\"", "Quotes inside cell must be doubled and wrapped");

                string cellWithNewline = CsvExporter.EscapeCell("First line\nSecond line");
                Assert(cellWithNewline.StartsWith("\"") && cellWithNewline.EndsWith("\""), "Cell with newline must be wrapped in quotes");
            });

            Test("CSV Exporter: Save UTF-8 With BOM File to Disk", delegate {
                string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "sitetrack_test_export.csv");
                List<string> headers = new List<string> { "TestCol1", "TestCol2" };
                List<List<string>> rows = new List<List<string>> { new List<string> { "Val1", "Val2" } };

                string csv = CsvExporter.FormatCsv(headers, rows);
                CsvExporter.SaveToFile(tempPath, csv);

                Assert(System.IO.File.Exists(tempPath), "CSV export file must exist on disk");
                byte[] bytes = System.IO.File.ReadAllBytes(tempPath);
                Assert(bytes.Length >= 3, "File should have at least 3 bytes");
                // Check UTF-8 BOM
                Assert(bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF, "Exported file must have UTF-8 BOM");

                try { System.IO.File.Delete(tempPath); } catch { }
            });

            // --- Phase 10 End-to-End & Comprehensive Validation Tests ---
            Test("Technical Rule 1: Rejects Empty or Whitespace Required Fields", delegate {
                ProjectController projCtrl = new ProjectController();
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                Result<int> pRes = projCtrl.CreateProject("   ", "Kigali", "Desc", DateTime.Today, DateTime.Today.AddDays(30), "Planning");
                Assert(!pRes.IsSuccess, "Must reject empty/whitespace project name");

                MaterialController matCtrl = new MaterialController();
                Result<int> mRes = matCtrl.CreateMaterial(1, "   ", "Bags", 5000m, "Desc");
                Assert(!mRes.IsSuccess, "Must reject empty material name");

                LaborController labCtrl = new LaborController();
                Result<int> wRes = labCtrl.CreateWorker(1, "   ", "+250788000000", 5000m, "ACTIVE");
                Assert(!wRes.IsSuccess, "Must reject empty worker full name");
            });

            Test("Technical Rule 2: Rejects Zero and Negative Numeric Fields", delegate {
                StockController stockCtrl = new StockController();
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                Result pZeroQty = stockCtrl.RecordPurchase(1, 1, 0m, 5000m, "Supplier", DateTime.Today);
                Assert(!pZeroQty.IsSuccess, "Must reject 0 purchase quantity");

                Result pNegPrice = stockCtrl.RecordPurchase(1, 1, 10m, -500m, "Supplier", DateTime.Today);
                Assert(!pNegPrice.IsSuccess, "Must reject negative purchase unit price");

                LaborController labCtrl = new LaborController();
                Result<int> wNegRate = labCtrl.CreateWorker(1, "Negative Worker", "+250788000000", -2500m, "ACTIVE");
                Assert(!wNegRate.IsSuccess, "Must reject negative daily rate");
            });

            Test("Technical Rule 3: Accepts Same-Day Dates But Rejects EndDate Before StartDate", delegate {
                ProjectController projCtrl = new ProjectController();
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                DateTime today = DateTime.Today;
                Result<int> sameDay = projCtrl.CreateProject("Same Day Site Task", "Kigali", "One day task", today, today, "Planning");
                Assert(sameDay.IsSuccess, "Start date == End date should be accepted");

                Result<int> invalidDates = projCtrl.CreateProject("Invalid Dates Project", "Kigali", "Desc", today.AddDays(10), today, "Planning");
                Assert(!invalidDates.IsSuccess, "EndDate before StartDate must be rejected");
            });

            Test("Business Rule 4: Site Manager Scoping Strictly Enforced Across All Domains", delegate {
                AuthController auth = new AuthController();
                auth.Login("sitemgr", "Admin@123"); // Assigned only to Project 1 & 2, NOT Project 3

                StockController stockCtrl = new StockController();
                Result smPurchase = stockCtrl.RecordPurchase(3, 1, 10m, 1000m, "Supplier", DateTime.Today);
                Assert(!smPurchase.IsSuccess, "Site Manager cannot record purchase for unassigned project 3");

                Result smUsage = stockCtrl.RecordUsage(3, 1, 5m, "Excavation", DateTime.Today);
                Assert(!smUsage.IsSuccess, "Site Manager cannot record usage for unassigned project 3");

                ActivityController actCtrl = new ActivityController();
                Result<int> smAct = actCtrl.RecordActivity(3, "Tile Laying", "Roofing work", DateTime.Today, 50);
                Assert(!smAct.IsSuccess, "Site Manager cannot record activity for unassigned project 3");

                LaborController labCtrl = new LaborController();
                Result<int> smAtt = labCtrl.RecordAttendance(3, 1, DateTime.Today, "PRESENT", "Masonry work");
                Assert(!smAtt.IsSuccess, "Site Manager cannot record attendance for unassigned project 3");
            });

            Test("Business Rule 5: Cannot Cancel or Delete Project With Active Movements or Stock", delegate {
                ProjectController projCtrl = new ProjectController();
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                // Project 1 has active seed stock and purchase records
                Result delRes = projCtrl.DeleteProject(1);
                Assert(!delRes.IsSuccess, "Deleting Project 1 with active stock/usage/labor must be blocked");
            });

            Test("End-to-End Lifecycle Integration: Project, Purchase, Usage, Labor, Activity, and Reporting", delegate {
                AuthController auth = new AuthController();
                auth.Login("admin", "Admin@123");

                // 1. Create Project
                ProjectController projCtrl = new ProjectController();
                Result<int> newProjRes = projCtrl.CreateProject("Kigali Green Park Complex", "Gasabo, Kigali", "Eco-friendly commercial hub", DateTime.Today, DateTime.Today.AddMonths(12), "Ongoing");
                Assert(newProjRes.IsSuccess, "Admin must be able to create new project");
                int projectId = newProjRes.Value;

                // 2. Assign Site Manager
                Result assignRes = projCtrl.AssignManager(projectId, 2); // user 2 is sitemgr
                Assert(assignRes.IsSuccess, "Admin must be able to assign site manager to project");

                // 3. Purchase Material (50 Bags at 12000 RWF)
                StockController stockCtrl = new StockController();
                Result purchRes = stockCtrl.RecordPurchase(projectId, 1, 50m, 12000m, "Rwanda Ciments Ltd", DateTime.Today);
                Assert(purchRes.IsSuccess, "Must record initial purchase successfully");

                // Verify stock balance
                ProjectMaterialStock stock = stockCtrl.GetMaterialStock(projectId, 1);
                Assert(stock != null && stock.QuantityAvailable == 50m, "Stock quantity must be 50");
                Assert(stock.AverageUnitPrice == 12000m, "Weighted average unit price must be 12000");

                // 4. Material Usage (15 Bags)
                Result useRes = stockCtrl.RecordUsage(projectId, 1, 15m, "Pillar foundation casting", DateTime.Today);
                Assert(useRes.IsSuccess, "Usage of 15 bags must succeed");

                stock = stockCtrl.GetMaterialStock(projectId, 1);
                Assert(stock.QuantityAvailable == 35m, "Remaining stock must be exactly 35 bags");

                // Verify Business Rule 1 Depletion Rejection
                Result overUseRes = stockCtrl.RecordUsage(projectId, 1, 40m, "Overdraft attempt", DateTime.Today);
                Assert(!overUseRes.IsSuccess, "Business Rule 1 must reject usage exceeding 35 available bags");

                // 5. Worker Registration & Attendance-Gated Payment
                LaborController labCtrl = new LaborController();
                Result<int> workerRes = labCtrl.CreateWorker(1, "E2E Craftsman Test", "+250788112233", 6000m, "ACTIVE");
                Assert(workerRes.IsSuccess, "Must register site worker");
                int workerId = workerRes.Value;

                // Mark Attendance PRESENT
                Result<int> attRes = labCtrl.RecordAttendance(projectId, workerId, DateTime.Today, "PRESENT", "Pillar formwork");
                Assert(attRes.IsSuccess, "Must record attendance PRESENT");

                // Pay Worker Wage (Business Rule 2)
                var attRecord = labCtrl.GetAttendance(projectId, DateTime.Today).Find(delegate(WorkerAttendance a) { return a.WorkerId == workerId; });
                Assert(attRecord != null, "Attendance record must be retrieved");

                Result payRes = labCtrl.RecordPayment(projectId, workerId, attRecord.Id, DateTime.Today, 6000m, 6000m, "Cash voucher");
                Assert(payRes.IsSuccess, "Payment for PRESENT worker must succeed");

                // 6. Log Site Activity (Technical Rule 5: 35%)
                ActivityController actCtrl = new ActivityController();
                Result<int> actRes = actCtrl.RecordActivity(projectId, "Foundation Slab Completed", "Completed casting foundation slab", DateTime.Today, 35);
                Assert(actRes.IsSuccess, "Activity with 35% progress must succeed");

                // 7. Generate Executive Report & Export to CSV
                ReportController repCtrl = new ReportController();
                ProjectExecutiveSummary execSummary = repCtrl.GetProjectFinancialSummary(projectId);
                Assert(execSummary != null, "Executive summary must not be null");
                Assert(execSummary.TotalMaterialPurchases == 600000m, "Total purchases must be 50 * 12000 = 600000");
                Assert(execSummary.TotalLaborWagesPaid == 6000m, "Total wages paid must be 6000");
                Assert(execSummary.CurrentProgress == 35, "Latest reported progress must be 35%");

                List<MaterialConsumptionReportItem> consumptionReport = repCtrl.GetMaterialConsumptionReport(projectId, DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1));
                Assert(consumptionReport.Count == 1, "Must have 1 consumption line");
                Assert(consumptionReport[0].QuantityUsed == 15m, "Quantity used must be 15");

                // Export to CSV
                List<string> headers = new List<string> { "Material", "Qty Used", "Cost" };
                List<List<string>> rows = new List<List<string>> {
                    new List<string> { consumptionReport[0].MaterialName, consumptionReport[0].QuantityUsed.ToString(), consumptionReport[0].TotalCost.ToString() }
                };
                string csv = CsvExporter.FormatCsv(headers, rows);
                Assert(csv.Contains("15"), "CSV must contain 15");
                Assert(csv.Contains("180000"), "CSV must contain 180000 total cost");
            });

            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine(string.Format("Results: {0} PASSED, {1} FAILED", _passedCount, _failedCount));
            Console.WriteLine("=================================================");

            return _failedCount == 0 ? 0 : 1;
        }

        private static void Test(string testName, Action testAction)
        {
            try
            {
                testAction();
                Console.WriteLine(string.Format("[PASS] {0}", testName));
                _passedCount++;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("[FAIL] {0}: {1}", testName, ex.Message));
                _failedCount++;
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception("Assertion Failed: " + message);
            }
        }
    }
}
