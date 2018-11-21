using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class CostViewModel
    {
        public IEnumerable<Cost> costList;
        public IEnumerable<CostCategoryViewModel> Categories;

    }
    public class CostEditViewModel
    {
        public IEnumerable<CostCategoryViewModel> Categories;
        public int CatId { get; set; }
        public string CostDescription { get; set; }
        public DateTime CostDate { get; set; }
        public double Amount { get; set; }
        public string PaidPerson { get; set; }
        public int CostType { get; set; }
        public string LoggedPerson { get; set; }
    }
    public class CostOverViewModel
    {
        public double AmountReceipt { get; set; }

        public double AmountSpend { get; set; }

        public int Status { get; set; }

        public string StartTime { get; set; }
        public string EndTime { get; set; }
    }

    public class CostCreateViewModel
    {
        public int CatId { get; set; }
        public string CostDescription { get; set; }
        public string CostDate { get; set; }
        public double Amount { get; set; }
        public string PaidPerson { get; set; }
        public int CostType { get; set; }
        public string LoggedPerson { get; set; }
        public string CostCode { get; set; }
        public int StoreId { get; set; }
        public int CostCategoryType { get; set; }
        public List<CostPaymentViewModel> listPayment { get; set; }
    }
    public class CostPaymentViewModel
    {
        public string rentId { get; set; }
        public string invoiceId { get; set; }
        public double receivablesAmount { get; set; }
        public double amount { get; set; }
        public int paymentId { get; set; }
    }
    public class CostOverViewViewModel
    {
        public double TotalReceipt { get; set; }
        public double TotalSpend { get; set; }
        public double TotalDebt { get; set; }
    }
}
