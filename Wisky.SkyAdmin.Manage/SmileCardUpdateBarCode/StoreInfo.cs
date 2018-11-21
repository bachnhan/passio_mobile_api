using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmileCardUpdateBarCode
{
    public class StoreInfo
    {
        public static SmileCardModel GetStoreInfo(int storeId)
        {
            var model = new SmileCardModel();
            switch(storeId)
            {
                #region 15F NTMK
                case 13:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000011";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 53C ND
                case 15:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000013";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 102 HVB
                case 28:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000010";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 11 VVT
                case 33:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000128";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 47 TCV
                case 34:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000139";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 97 DTH
                case 35:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000140";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 213 NVC
                case 36:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000554";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 91 ND
                case 37:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000556";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 227A XVNT
                case 38:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000012";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 250 DBP
                case 39:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000100";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 431 LHP
                case 40:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000555";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 43-45 HTM
                case 41:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000557";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 179 NCT
                case 42:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000559";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 54 NT
                case 44:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000553";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 124 KH
                case 45:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000558";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                #endregion
                #region 2A TDT
                case 46:
                    model.ProcessingCode = "000000";
                    model.MerchantId = "000000000000001";
                    model.TerminalId = "00000567";
                    model.StaffId = "0000";
                    model.DiscountPercent = "10.0";
                    model.Saving = "0.5";
                    break;
                    #endregion
            }
            return model;
        }
    }
}
