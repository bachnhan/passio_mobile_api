using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HmsService.ViewModels;
using AutoMapper.QueryableExtensions;

namespace HmsService.Sdk
{
    public partial class ReportTrackingApi
    {
        #region SystemReport
        public IEnumerable<ReportTrackingViewModel> GetReportTrackings()
        {
            var reportTrackings = this.BaseService.GetReportTrackings()
                .ProjectTo<ReportTrackingViewModel>(this.AutoMapperConfig)
                .ToList();
            return reportTrackings;
        }
        #endregion
        #region StoreReport
        public async Task<ReportTrackingViewModel> GetReportTrackingByDateAndStoreId(DateTime startTime, int storeId)
        {
            var reportTracking = await this.BaseService.GetReportTrackingByDateAndStoreId(startTime, storeId);
            if(reportTracking == null)
            {
                return null;
            }
            else
            {
                return new ReportTrackingViewModel(reportTracking);
            }
        }
        #endregion
    }
}
