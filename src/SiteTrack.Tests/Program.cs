using System;
using System.Collections.Generic;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
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

            Test("OTP Verification Expiration Logic", delegate {
                OtpVerification otp = new OtpVerification();
                otp.OtpCode = "123456";
                otp.ExpiresAt = DateTime.Now.AddMinutes(5);
                Assert(otp.IsValid, "Fresh OTP should be valid");

                otp.ExpiresAt = DateTime.Now.AddMinutes(-1);
                Assert(otp.IsExpired, "Past expiration date should mark OTP expired");
                Assert(!otp.IsValid, "Expired OTP should not be valid");
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
                int projId = 2; // Gahanga Logistics Hub
                int matId = 1;  // Cement

                // Record first purchase: 50 bags @ 12000
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
                Assert(s1.QuantityAvailable == 50m, "Stock quantity should be 50");
                Assert(s1.AverageUnitPrice == 12000m, "Average unit price should be 12000");

                // Record second purchase: 50 bags @ 14000
                stockRepo.RecordPurchase(new MaterialPurchase {
                    ProjectId = projId,
                    MaterialId = matId,
                    Quantity = 50m,
                    UnitPrice = 14000m,
                    SupplierName = "Cimerwa Direct",
                    PurchaseDate = DateTime.Today,
                    RecordedBy = 1
                });

                ProjectMaterialStock s2 = stockRepo.GetStock(projId, matId);
                Assert(s2.QuantityAvailable == 100m, "Stock quantity should be 100");
                Assert(s2.AverageUnitPrice == 13000m, "Weighted average price should be 13000 ((50*12k + 50*14k) / 100)");

                // Check movement history
                List<MaterialStockMovement> movs = stockRepo.GetMovements(projId, matId);
                Assert(movs.Count >= 2, "Stock movements must have at least 2 IN records");
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
                int workerId = 1; // Jean Baptiste Karekezi
                DateTime testDate = new DateTime(2026, 11, 10);

                // 1. Mark worker ABSENT
                int attId = laborRepo.SaveAttendance(new WorkerAttendance {
                    ProjectId = projId,
                    WorkerId = workerId,
                    WorkDate = testDate,
                    AttendanceStatus = "ABSENT",
                    WorkDescription = "Sick leave",
                    RecordedBy = 2
                });

                // 2. Attempt to create payment for ABSENT attendance -> MUST FAIL
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

                // 3. Now mark worker PRESENT on next day and verify payment succeeds
                DateTime testDate2 = new DateTime(2026, 11, 11);
                int attId2 = laborRepo.SaveAttendance(new WorkerAttendance {
                    ProjectId = projId,
                    WorkerId = workerId,
                    WorkDate = testDate2,
                    AttendanceStatus = "PRESENT",
                    WorkDescription = "Slab casting supervision",
                    RecordedBy = 2
                });

                int payId = laborRepo.SavePayment(new WorkerPayment {
                    ProjectId = projId,
                    WorkerId = workerId,
                    AttendanceId = attId2,
                    WorkDate = testDate2,
                    DailyRate = 15000m,
                    AmountOwed = 15000m,
                    AmountPaid = 15000m,
                    PaymentStatus = "PAID",
                    PaidBy = 2
                });
                Assert(payId > 0, "Payment record should be created successfully for PRESENT attendance");
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
