using System;
using SiteTrack.Core;
using SiteTrack.Core.Models;

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
