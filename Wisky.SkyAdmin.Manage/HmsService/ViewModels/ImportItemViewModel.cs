using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class ImportItemViewModel
    {
        
    }
    public class ImportExportModel
    {
        public double InInventory { get; set; }
        public double InChangeInventory { get; set; }
        public double OutChangeInventory { get; set; }
        public double OutInventory { get; set; }
        public double SoldProduct { get; set; }
        public double DraftInventory { get; set; }
        public double TotalExport { get; set; }
        public double TotalImport { get; set; }
    }
    public class InfoIndex
    {
        public long StockValue { get; set; }
        public double ImportValue { get; set; }
        public double COSValue { get; set; }
        public int NumberImport { get; set; }
        public int NumberExport { get; set; }
        public int Tranfer { get; set; }
        public int GetTranfer { get; set; }
    }
    public class ItemDetail
    {
        public string DateTime { get; set; }

        public string ProductName { get; set; }

        public string Unit { get; set; }

        public double ImportInventory { get; set; }

        public double ReceiveInventory { get; set; }

        public double TotalImport { get; set; }

        public double Sold { get; set; }

        public double GiveBack { get; set; }

        public double Destroy { get; set; }

        public double ExportInventory { get; set; }

        public double TotalExport { get; set; }
    }
}
