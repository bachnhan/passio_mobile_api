using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScanAtBrand.Controllers
{
    public class UsingEmployeeReportAtBrandController : Controller
    {
        // GET: FingerScanAtBrand/UsingEmployeeReportAtBrand
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetDataStore(string startTime, string endTime, int storeId, int BrandId)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();

            var empInstore = new EmployeeInStoreApi();
            var store = new StoreApi();
            var attendanceApi = new AttendanceApi();

            var listStore = store.BaseService.Get(q => q.isAvailable==true);
            var result = new List<Object>();
            int stt = 0;
            foreach (var st in listStore)
            {
                stt++;
                var listEmp = empInstore.BaseService.Get(q => q.StoreId == st.ID).GroupBy(q => q.EmployeeId);
                var numOfEmp = listEmp.Select(x => x.Key).Count();
                var getAllEmpWorkTime = attendanceApi.BaseService.Get(q => q.StoreId == st.ID && (q.CheckMin != null || q.CheckMax != null)).Where(q => startDate <= q.ShiftMin && endDate >= q.ShiftMax).GroupBy(p => p.EmployeeId);
                long totalTime = 0;
                int count = 0;
                var finalTotalWorkTime = new TimeSpan();
                string stringtime = "";
                foreach (var time in getAllEmpWorkTime)
                {
                    foreach (var tmpTime in time)
                    {
                        if (tmpTime.TotalWorkTime != null)
                        {
                            totalTime += tmpTime.TotalWorkTime.Value.Ticks;
                        }

                    }
                    finalTotalWorkTime = new TimeSpan(totalTime);

                    count++;                   
                }
                var tmp = Int32.Parse(finalTotalWorkTime.TotalHours.ToString("N").Split('.')[0]);
                stringtime += finalTotalWorkTime.TotalHours.ToString("N").Split('.')[0];
                stringtime += ":";
                if (finalTotalWorkTime.Minutes < 10)
                {
                    stringtime += "0" + finalTotalWorkTime.Minutes;
                }
                else
                {
                    stringtime += finalTotalWorkTime.Minutes;
                }
                var tmpMinute = int.Parse(stringtime.Split(':')[1]);
                if (tmpMinute > 60)
                {
                    stringtime = (int.Parse(stringtime.Split(':')[0]) + (int)(tmpMinute / 60)).ToString("N");
                    stringtime += ":" + (tmpMinute % 60).ToString("N");
                }

                result.Add(new Object[] {stt,st.ShortName, count, stringtime});
            }
            
            
            return Json(new { aaData = result }, JsonRequestBehavior.AllowGet);
        }
    }
}