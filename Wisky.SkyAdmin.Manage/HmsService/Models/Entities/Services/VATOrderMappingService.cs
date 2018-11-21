using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IVATOrderMappingService
    {
        VATOrderMapping GetVATOrderMappingMasterAsync(int VATOrderMappingId);

        #region Store Report
        IQueryable<VATOrderMapping> GetVATOrderMappingByInvoiceId(int InvoiceId);
        #endregion
        IEnumerable<VATOrderMapping> GetVATOrderMapping();
    }
    public partial class VATOrderMappingService
    {
        public VATOrderMapping GetVATOrderMappingMasterAsync(int VATOrderMappingId)
        {
            var orderMapping = Get(VATOrderMappingId);
            return orderMapping;
        }
        #region StoreReport
        public IQueryable<VATOrderMapping> GetVATOrderMappingByInvoiceId(int InvoiceId)
        {
            return this.Get(c => c.InvoiceID == InvoiceId);
        }
        #endregion

        public IEnumerable<VATOrderMapping> GetVATOrderMapping()
        {
            return this.GetActive();
        }
    }
}
