using SkyWeb.DatVM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IReportTrackingService
    {
        #region SystemReport
        IQueryable<ReportTracking> GetReportTrackings();
        #endregion
        #region StoreReport
        Task<ReportTracking> GetReportTrackingByDateAndStoreId(DateTime startTime, int storeId);
        #endregion
    }
    public partial class ReportTrackingService
    {
        #region SystemReport
        public IQueryable<ReportTracking> GetReportTrackings()
        {
            return this.Get();
        }
        #endregion
        #region StoreReport
        public async Task<ReportTracking> GetReportTrackingByDateAndStoreId(DateTime startTime, int storeId)
        {
            return await this.FirstOrDefaultAsync(d =>
                        d.Date.Value.Year == startTime.Year && d.Date.Value.Month == startTime.Month && d.Date.Value.Day == startTime.Day &&
                        d.StoreId == storeId);
        }
        #endregion
    }
}
