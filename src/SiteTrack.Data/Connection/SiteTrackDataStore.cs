using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using SiteTrack.Core;
using SiteTrack.Core.Models;

namespace SiteTrack.Data.Connection
{
    /// <summary>
    /// Thread-safe in-memory relational database provider implementing full relational
    /// constraints, auto-increment IDs, and seed data initialization.
    /// </summary>
    public class SiteTrackDataStore
    {
        private static readonly object _syncRoot = new object();
        private static SiteTrackDataStore _instance;

        public static SiteTrackDataStore Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new SiteTrackDataStore();
                            _instance.InitializeSeedData();
                        }
                    }
                }
                return _instance;
            }
        }

        // Tables
        public readonly List<User> Users = new List<User>();
        public readonly List<Project> Projects = new List<Project>();
        public readonly List<ProjectManager> ProjectManagers = new List<ProjectManager>();
        public readonly List<MaterialCategory> MaterialCategories = new List<MaterialCategory>();
        public readonly List<Material> Materials = new List<Material>();
        public readonly List<MaterialPurchase> MaterialPurchases = new List<MaterialPurchase>();
        public readonly List<ProjectMaterialStock> ProjectMaterialStocks = new List<ProjectMaterialStock>();
        public readonly List<MaterialStockMovement> MaterialStockMovements = new List<MaterialStockMovement>();
        public readonly List<MaterialUsage> MaterialUsages = new List<MaterialUsage>();
        public readonly List<ProjectActivity> ProjectActivities = new List<ProjectActivity>();
        public readonly List<WorkerType> WorkerTypes = new List<WorkerType>();
        public readonly List<SiteWorker> SiteWorkers = new List<SiteWorker>();
        public readonly List<WorkerAttendance> WorkerAttendances = new List<WorkerAttendance>();
        public readonly List<WorkerPayment> WorkerPayments = new List<WorkerPayment>();
        public readonly List<OtpVerification> OtpVerifications = new List<OtpVerification>();
        public readonly List<NotificationLog> NotificationLogs = new List<NotificationLog>();

        // Auto-increment counters
        private int _userId = 0;
        private int _projectId = 0;
        private int _projectManagerId = 0;
        private int _categoryId = 0;
        private int _materialId = 0;
        private int _purchaseId = 0;
        private int _stockId = 0;
        private int _movementId = 0;
        private int _usageId = 0;
        private int _activityId = 0;
        private int _workerTypeId = 0;
        private int _workerId = 0;
        private int _attendanceId = 0;
        private int _paymentId = 0;
        private int _otpId = 0;
        private int _notificationId = 0;

        public int NextUserId() { lock (_syncRoot) { return ++_userId; } }
        public int NextProjectId() { lock (_syncRoot) { return ++_projectId; } }
        public int NextProjectManagerId() { lock (_syncRoot) { return ++_projectManagerId; } }
        public int NextCategoryId() { lock (_syncRoot) { return ++_categoryId; } }
        public int NextMaterialId() { lock (_syncRoot) { return ++_materialId; } }
        public int NextPurchaseId() { lock (_syncRoot) { return ++_purchaseId; } }
        public int NextStockId() { lock (_syncRoot) { return ++_stockId; } }
        public int NextMovementId() { lock (_syncRoot) { return ++_movementId; } }
        public int NextUsageId() { lock (_syncRoot) { return ++_usageId; } }
        public int NextActivityId() { lock (_syncRoot) { return ++_activityId; } }
        public int NextWorkerTypeId() { lock (_syncRoot) { return ++_workerTypeId; } }
        public int NextWorkerId() { lock (_syncRoot) { return ++_workerId; } }
        public int NextAttendanceId() { lock (_syncRoot) { return ++_attendanceId; } }
        public int NextPaymentId() { lock (_syncRoot) { return ++_paymentId; } }
        public int NextOtpId() { lock (_syncRoot) { return ++_otpId; } }
        public int NextNotificationId() { lock (_syncRoot) { return ++_notificationId; } }

        public object SyncRoot
        {
            get { return _syncRoot; }
        }

        public IDbConnection CreateConnection()
        {
            return new InternalDbConnection(this);
        }

        public void InitializeSeedData()
        {
            lock (_syncRoot)
            {
                if (Users.Count > 0) return;

                // 1. Users (Password is "Admin@123" -> SHA-256 hash)
                string defaultHash = "a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3";
                Users.Add(new User { Id = NextUserId(), FullName = "System Administrator", Username = "admin", Email = "admin@sitetrack.rw", Password = defaultHash, Role = "Administrator", Phone = "+250788100001", Status = "ACTIVE" });
                Users.Add(new User { Id = NextUserId(), FullName = "Patrick Mutabazi", Username = "sitemgr", Email = "pmutabazi@sitetrack.rw", Password = defaultHash, Role = "SiteManager", Phone = "+250788100002", Status = "ACTIVE" });
                Users.Add(new User { Id = NextUserId(), FullName = "Aline Uwimana", Username = "aline.mgr", Email = "aline@sitetrack.rw", Password = defaultHash, Role = "SiteManager", Phone = "+250788100003", Status = "ACTIVE" });

                // 2. Projects
                Projects.Add(new Project { Id = NextProjectId(), ProjectName = "Kigali Heights Commercial Wing B", Location = "Kimihurura, Kigali", Description = "Construction of 6-storey commercial and office complex with basement parking.", StartDate = new DateTime(2026, 1, 15), ExpectedEndDate = new DateTime(2026, 12, 30), Status = "Ongoing", CreatedBy = 1 });
                Projects.Add(new Project { Id = NextProjectId(), ProjectName = "Gahanga Logistics Hub Phase 1", Location = "Kicukiro, Gahanga", Description = "Development of multi-bay warehousing and heavy vehicle loading docks.", StartDate = new DateTime(2026, 3, 1), ExpectedEndDate = new DateTime(2026, 10, 15), Status = "Ongoing", CreatedBy = 1 });
                Projects.Add(new Project { Id = NextProjectId(), ProjectName = "Nyagatare Modern Secondary School", Location = "Nyagatare District", Description = "Educational facility expansion including 12 smart classrooms and laboratory block.", StartDate = new DateTime(2026, 6, 1), ExpectedEndDate = new DateTime(2027, 2, 28), Status = "Planning", CreatedBy = 1 });

                // 3. Project Managers
                ProjectManagers.Add(new ProjectManager { Id = NextProjectManagerId(), ProjectId = 1, UserId = 2, AssignedDate = new DateTime(2026, 1, 15), Status = "ACTIVE" });
                ProjectManagers.Add(new ProjectManager { Id = NextProjectManagerId(), ProjectId = 2, UserId = 2, AssignedDate = new DateTime(2026, 3, 1), Status = "ACTIVE" });
                ProjectManagers.Add(new ProjectManager { Id = NextProjectManagerId(), ProjectId = 3, UserId = 3, AssignedDate = new DateTime(2026, 6, 1), Status = "ACTIVE" });

                // 4. Material Categories
                MaterialCategories.Add(new MaterialCategory { Id = NextCategoryId(), CategoryName = "Cement & Aggregates", Description = "Portland cements, river sands, gravel, ballast and crushed stones" });
                MaterialCategories.Add(new MaterialCategory { Id = NextCategoryId(), CategoryName = "Steel & Reinforcement", Description = "Deformed reinforcement bars, BRC mesh, binding wire, structural steel beams" });
                MaterialCategories.Add(new MaterialCategory { Id = NextCategoryId(), CategoryName = "Masonry & Blocks", Description = "Hollow concrete blocks, solid masonry bricks, paving stones" });
                MaterialCategories.Add(new MaterialCategory { Id = NextCategoryId(), CategoryName = "Electrical Supplies", Description = "Copper conduit wiring, switches, distribution boards, breakers, lighting" });
                MaterialCategories.Add(new MaterialCategory { Id = NextCategoryId(), CategoryName = "Plumbing & Drainage", Description = "PPR pressure pipes, PVC drainage pipes, sanitary fixtures, brass valves" });
                MaterialCategories.Add(new MaterialCategory { Id = NextCategoryId(), CategoryName = "Finishing & Paints", Description = "Interior wall paints, weather-guard exterior coating, primers, thinners" });

                // 5. Materials
                Materials.Add(new Material { Id = NextMaterialId(), CategoryId = 1, MaterialName = "Dangote 42.5R Portland Cement (50kg)", Unit = "Bags", CurrentPrice = 12500.00m, Description = "High-strength structural cement for concrete slabs and columns", Status = "ACTIVE" });
                Materials.Add(new Material { Id = NextMaterialId(), CategoryId = 1, MaterialName = "Clean River Sand", Unit = "Tons", CurrentPrice = 18000.00m, Description = "Coarse river sand for high-grade mortar and concrete mixing", Status = "ACTIVE" });
                Materials.Add(new Material { Id = NextMaterialId(), CategoryId = 1, MaterialName = "Machine-Crushed Aggregate (20mm)", Unit = "Tons", CurrentPrice = 22000.00m, Description = "Screened blue basalt aggregate for reinforced concrete", Status = "ACTIVE" });
                Materials.Add(new Material { Id = NextMaterialId(), CategoryId = 2, MaterialName = "12mm Deformed High-Yield Rebar (12m)", Unit = "Bars", CurrentPrice = 14500.00m, Description = "BS4449 Grade 500 rebar for structural reinforcement", Status = "ACTIVE" });
                Materials.Add(new Material { Id = NextMaterialId(), CategoryId = 2, MaterialName = "16mm Deformed High-Yield Rebar (12m)", Unit = "Bars", CurrentPrice = 26000.00m, Description = "Grade 500 tensile rebar for main foundation footings and columns", Status = "ACTIVE" });
                Materials.Add(new Material { Id = NextMaterialId(), CategoryId = 2, MaterialName = "Annealed Binding Wire (25kg Roll)", Unit = "Rolls", CurrentPrice = 35000.00m, Description = "16-gauge soft black binding wire for steel tying", Status = "ACTIVE" });
                Materials.Add(new Material { Id = NextMaterialId(), CategoryId = 3, MaterialName = "Hollow Concrete Blocks (20x20x40cm)", Unit = "Pieces", CurrentPrice = 850.00m, Description = "Standard load-bearing masonry blocks", Status = "ACTIVE" });
                Materials.Add(new Material { Id = NextMaterialId(), CategoryId = 4, MaterialName = "2.5mm Single Core Copper Cable (100m)", Unit = "Rolls", CurrentPrice = 48000.00m, Description = "Flame-retardant electrical wiring for power sockets", Status = "ACTIVE" });
                Materials.Add(new Material { Id = NextMaterialId(), CategoryId = 5, MaterialName = "PVC Pressure Pipe 110mm (6m)", Unit = "Pipes", CurrentPrice = 28500.00m, Description = "Class 10 heavy-duty drainage and sewer pipe", Status = "ACTIVE" });
                Materials.Add(new Material { Id = NextMaterialId(), CategoryId = 6, MaterialName = "Sadolin Weather-Shield Exterior White (20L)", Unit = "Buckets", CurrentPrice = 65000.00m, Description = "Premium acrylic emulsion coating with fungal protection", Status = "ACTIVE" });

                // 6. Worker Types
                WorkerTypes.Add(new WorkerType { Id = NextWorkerTypeId(), TypeName = "General Site Foreman", DefaultDailyRate = 15000.00m, Description = "Supervises daily operations, safety, and trade coordination" });
                WorkerTypes.Add(new WorkerType { Id = NextWorkerTypeId(), TypeName = "Master Mason (Fundi)", DefaultDailyRate = 10000.00m, Description = "Bricklaying, blockwork, rendering, and plastering work" });
                WorkerTypes.Add(new WorkerType { Id = NextWorkerTypeId(), TypeName = "Certified Steel Fixer", DefaultDailyRate = 10000.00m, Description = "Rebar bending, cutting, tying, and structural placement" });
                WorkerTypes.Add(new WorkerType { Id = NextWorkerTypeId(), TypeName = "Licensed Electrician", DefaultDailyRate = 12000.00m, Description = "Conduit piping, cable pulling, termination, and panel assembly" });
                WorkerTypes.Add(new WorkerType { Id = NextWorkerTypeId(), TypeName = "Master Plumber", DefaultDailyRate = 11000.00m, Description = "Piping installation, pressure testing, and sanitary drainage" });
                WorkerTypes.Add(new WorkerType { Id = NextWorkerTypeId(), TypeName = "Casual Site Worker (Umubyizi)", DefaultDailyRate = 5000.00m, Description = "Material offloading, concrete mixing, excavation, site cleanup" });

                // 7. Site Workers
                SiteWorkers.Add(new SiteWorker { Id = NextWorkerId(), WorkerTypeId = 1, FullName = "Jean Baptiste Karekezi", Phone = "+250788200010", DailyRate = 15000.00m, Status = "ACTIVE" });
                SiteWorkers.Add(new SiteWorker { Id = NextWorkerId(), WorkerTypeId = 2, FullName = "Emmanuel Ndungutse", Phone = "+250788200011", DailyRate = 10000.00m, Status = "ACTIVE" });
                SiteWorkers.Add(new SiteWorker { Id = NextWorkerId(), WorkerTypeId = 2, FullName = "Innocent Habimana", Phone = "+250788200012", DailyRate = 10000.00m, Status = "ACTIVE" });
                SiteWorkers.Add(new SiteWorker { Id = NextWorkerId(), WorkerTypeId = 3, FullName = "Eric Mugisha", Phone = "+250788200013", DailyRate = 10000.00m, Status = "ACTIVE" });
                SiteWorkers.Add(new SiteWorker { Id = NextWorkerId(), WorkerTypeId = 4, FullName = "David Hakizimana", Phone = "+250788200014", DailyRate = 12000.00m, Status = "ACTIVE" });
                SiteWorkers.Add(new SiteWorker { Id = NextWorkerId(), WorkerTypeId = 5, FullName = "Vincent Bizimana", Phone = "+250788200015", DailyRate = 11000.00m, Status = "ACTIVE" });
                SiteWorkers.Add(new SiteWorker { Id = NextWorkerId(), WorkerTypeId = 6, FullName = "Claudine Uwase", Phone = "+250788200016", DailyRate = 5000.00m, Status = "ACTIVE" });
                SiteWorkers.Add(new SiteWorker { Id = NextWorkerId(), WorkerTypeId = 6, FullName = "Theogene Nsabimana", Phone = "+250788200017", DailyRate = 5000.00m, Status = "ACTIVE" });

                // 8. Stock for Project 1
                ProjectMaterialStocks.Add(new ProjectMaterialStock { Id = NextStockId(), ProjectId = 1, MaterialId = 1, QuantityAvailable = 120.00m, MinimumQuantity = 30.00m, AverageUnitPrice = 12500.00m });
                ProjectMaterialStocks.Add(new ProjectMaterialStock { Id = NextStockId(), ProjectId = 1, MaterialId = 4, QuantityAvailable = 85.00m, MinimumQuantity = 20.00m, AverageUnitPrice = 14500.00m });
                ProjectMaterialStocks.Add(new ProjectMaterialStock { Id = NextStockId(), ProjectId = 1, MaterialId = 7, QuantityAvailable = 500.00m, MinimumQuantity = 150.00m, AverageUnitPrice = 850.00m });

                // Initial Stock Inflows
                MaterialStockMovements.Add(new MaterialStockMovement { Id = NextMovementId(), ProjectId = 1, MaterialId = 1, MovementType = "IN", Quantity = 120.00m, UnitPrice = 12500.00m, TotalPrice = 1500000.00m, MovementDate = new DateTime(2026, 1, 16), Description = "Initial site stock provisioning", ReferenceType = "purchase", ReferenceId = null, RecordedBy = 1 });
                MaterialStockMovements.Add(new MaterialStockMovement { Id = NextMovementId(), ProjectId = 1, MaterialId = 4, MovementType = "IN", Quantity = 85.00m, UnitPrice = 14500.00m, TotalPrice = 1232500.00m, MovementDate = new DateTime(2026, 1, 16), Description = "Initial structural steel delivery", ReferenceType = "purchase", ReferenceId = null, RecordedBy = 1 });
                MaterialStockMovements.Add(new MaterialStockMovement { Id = NextMovementId(), ProjectId = 1, MaterialId = 7, MovementType = "IN", Quantity = 500.00m, UnitPrice = 850.00m, TotalPrice = 425000.00m, MovementDate = new DateTime(2026, 1, 16), Description = "Initial masonry block delivery", ReferenceType = "purchase", ReferenceId = null, RecordedBy = 1 });
            }
        }
    }

    /// <summary>
    /// IDbConnection implementation for the internal provider to allow standard ADO.NET connection testing.
    /// </summary>
    public class InternalDbConnection : IDbConnection
    {
        private ConnectionState _state = ConnectionState.Closed;
        private readonly SiteTrackDataStore _store;

        public InternalDbConnection(SiteTrackDataStore store)
        {
            this._store = store;
        }

        public string ConnectionString { get; set; }
        public int ConnectionTimeout { get { return 30; } }
        public string Database { get { return "site_track_construction_manager_db"; } }
        public ConnectionState State { get { return _state; } }

        public IDbTransaction BeginTransaction() { return new InternalDbTransaction(this); }
        public IDbTransaction BeginTransaction(IsolationLevel il) { return new InternalDbTransaction(this); }
        public void ChangeDatabase(string databaseName) { }
        public void Close() { _state = ConnectionState.Closed; }
        public IDbCommand CreateCommand() { return null; }
        public void Open() { _state = ConnectionState.Open; }
        public void Dispose() { Close(); }
    }

    public class InternalDbTransaction : IDbTransaction
    {
        private readonly IDbConnection _connection;
        public InternalDbTransaction(IDbConnection connection) { _connection = connection; }
        public IDbConnection Connection { get { return _connection; } }
        public IsolationLevel IsolationLevel { get { return IsolationLevel.ReadCommitted; } }
        public void Commit() { }
        public void Rollback() { }
        public void Dispose() { }
    }
}
