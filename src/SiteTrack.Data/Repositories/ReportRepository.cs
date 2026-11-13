using System;
using System.Collections.Generic;
using System.Linq;
using SiteTrack.Core;
using SiteTrack.Core.Interfaces;
using SiteTrack.Core.Models;
using SiteTrack.Data.Connection;

namespace SiteTrack.Data.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly SiteTrackDataStore _store;

        public ReportRepository()
        {
            this._store = SiteTrackDataStore.Instance;
        }

        public decimal GetTotalMaterialExpenditure(int projectId)
        {
            lock (_store.SyncRoot)
            {
                return _store.MaterialPurchases
                    .Where(delegate(MaterialPurchase p) { return p.ProjectId == projectId; })
                    .Sum(delegate(MaterialPurchase p) { return p.TotalPrice; });
            }
        }

        public decimal GetTotalLaborCost(int projectId)
        {
            lock (_store.SyncRoot)
            {
                return _store.WorkerPayments
                    .Where(delegate(WorkerPayment p) { return p.ProjectId == projectId; })
                    .Sum(delegate(WorkerPayment p) { return p.AmountPaid; });
            }
        }

        public List<ProjectMaterialStock> GetLowStockAlerts(int? projectId)
        {
            lock (_store.SyncRoot)
            {
                IEnumerable<ProjectMaterialStock> query = _store.ProjectMaterialStocks
                    .Where(delegate(ProjectMaterialStock s) { return s.IsLowStock; });

                if (projectId.HasValue)
                {
                    query = query.Where(delegate(ProjectMaterialStock s) { return s.ProjectId == projectId.Value; });
                }

                List<ProjectMaterialStock> list = query.ToList();
                foreach (ProjectMaterialStock s in list)
                {
                    Material m = _store.Materials.FirstOrDefault(delegate(Material mat) { return mat.Id == s.MaterialId; });
                    if (m != null)
                    {
                        s.MaterialName = m.MaterialName;
                        s.Unit = m.Unit;
                    }
                    Project p = _store.Projects.FirstOrDefault(delegate(Project proj) { return proj.Id == s.ProjectId; });
                    if (p != null) s.ProjectName = p.ProjectName;
                }
                return list;
            }
        }
    }
}

