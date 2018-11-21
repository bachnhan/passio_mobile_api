using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class VoucherEditViewModel : VoucherViewModel
    {
        int VoucherType { get; set; }
        /// <summary>
        /// voucherquantity là số lượng phát hành 
        /// </summary>
        int VoucherQuantity { get; set; }
    }
}
