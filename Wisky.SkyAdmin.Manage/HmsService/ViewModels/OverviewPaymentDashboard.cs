using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class OverviewPaymentDashboard
    {
        public double TotalPayment { get; set; }
        public int TotalTransactionPayment { get; set; }
        public double TotalPaymentForSales { get; set; }
        public double TotalTransactionExchangeCash { get; set; }
        public int TotalTransactionPaymentForSales { get; set; }

        public double TotalPaymentCard { get; set; }
        public int TotalTransactionPaymentCard { get; set; }

        public double TotalPaymentE_Wallet { get; set; }
        public int TotalTransactionPaymentE_Wallet { get; set; }

        public double TotalPaymentE_Wallet_Momo { get; set; }
        public int TotalTransactionPaymentE_Wallet_Momo { get; set; }
        public double TotalPaymentE_Wallet_GiftTalk { get; set; }
        public int TotalTransactionPaymentE_Wallet_GiftTalk { get; set; }

        public double TotalPaymentBank { get; set; }
        public int TotalTransactionPaymentBank { get; set; }

        public double TotalPaymentOther { get; set; }
        public int TotalTransactionPaymentOther { get; set; }

        public double TotalTransactionPaymentBuyCard { get; set; }
        public double TotalPaymentBuyCard { get; set; }

    }
}
