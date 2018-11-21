using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using COMExcel = Microsoft.Office.Interop.Excel;

namespace HmsService.Models
{
    public class ExportToExcelExtensions : Controller
    {
        public static bool ExportToExcel(List<string> headers, IEnumerable<object> _list, string fileName)
        {
            // Khởi động chtr Excel
            COMExcel.Application exApp = new COMExcel.Application();

            // Thêm file temp xls
            COMExcel.Workbook exBook = exApp.Workbooks.Add(
                      COMExcel.XlWBATemplate.xlWBATWorksheet);

            // Lấy sheet 1.
            COMExcel.Worksheet exSheet = (COMExcel.Worksheet)exBook.Worksheets[1];
            COMExcel.Range r = (COMExcel.Range)exSheet.Cells[1, 1];

            // header.Add("#;1;2;r");
            // i represents for column
            int maxRow = 2;
            var col = 1;

            #region Add header
            for (int i = 0; i < headers.Count; i++)
            {
                var header = headers[i];
                string[] items = header.Split(';');

                var value = items[0];
                var row = Int32.Parse(items[1]);
                var range = Int32.Parse(items[2]);

                if (maxRow < row + 1)
                {
                    maxRow = row + 1;
                }

                r = (COMExcel.Range)exSheet.Cells[row, col];

                if (range < 2)
                {
                    r.Value2 = items[0];

                }
                else
                {
                    var type = items[3];

                    //merge column
                    if (type.Equals("c"))
                    {
                        var mergedCell = (COMExcel.Range)exSheet.Range[r, exSheet.Cells[row, col + range - 1]].Merge();
                        r.Value2 = value;
                        col--;
                    }
                    // merge row
                    else
                    {
                        var mergedCell = (COMExcel.Range)exSheet.Range[r, exSheet.Cells[range, col]].Merge();
                        r.Value2 = value;
                    }
                }

                col++;
            }
            #endregion

            //#region Add value to table
            var list = _list.ToList();

            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                var type = item.GetType();
                PropertyInfo[] properties;

                properties = type.GetProperties();
                r = (COMExcel.Range)exSheet.Cells[maxRow + i, 1];
                r.Value2 = i + 1;
                for (int j = 0; j < properties.Length; j++)
                {
                    var property = properties[j];
                    r = (COMExcel.Range)exSheet.Cells[maxRow + i, j + 2];
                    r.Value2 = property.GetValue(item, null);
                }
            }
            //#endregion

            #region fit all column
            COMExcel.Range usedrange = exSheet.UsedRange; // detect all col were used (column whic has value)
            //usedrange.Column.autofit();
            usedrange.Columns.AutoFit();
            #endregion

            #region save file to local disk
            var issuccess = true;
            try
            {
                exBook.SaveAs(fileName, COMExcel.XlFileFormat.xlWorkbookNormal,
                             null, null, false, false,
                            COMExcel.XlSaveAsAccessMode.xlExclusive,
                            false, false, false, false, false);


                //folderBrowserDialog1.ShowDialog();
                //System.Windows
                //if (saveFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK
                //    && saveFileDialog1.FileName.Length > 0)
                //{
                //    System.Windows.Input.
                //    richTextBox1.SaveFile(saveFileDialog1.FileName,
                //        RichTextBoxStreamType.PlainText);
                //}
            }
            catch (Exception e)
            {
                //message = e.message.tostring();
                issuccess = false;
            }
            finally
            {
                exApp.Quit();
            }

            return issuccess;
           // return true;
            #endregion

        }
    }
}
