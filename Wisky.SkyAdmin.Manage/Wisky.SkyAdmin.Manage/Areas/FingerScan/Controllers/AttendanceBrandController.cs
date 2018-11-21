using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers
{
    public class AttendanceBrandController : Controller
    {
        // GET: FingerScan/AttendanceBrand
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult CheckShift()
        {
            return View();
        }

        public ActionResult GetAllStore(int brandId)
        {
            var storeApi = new StoreApi();
            var list = storeApi.GetActive().Where(q => q.BrandId == brandId).ToList();
            return Json(list);
        }

        public ActionResult GetAllEmployeeToAdd(int storeId, int brandId, int storeIdAdd, TimeSpan shiftMin, TimeSpan shiftMax, string endTime, string startTime, int empGroupId)
        {
            int count = 1;
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var employeeApi = new EmployeeApi();
            var Listemployees = employeeApi.GetAllEmployeeFreeByTimeSpan(startDate.GetStartOfDate(), endDate.GetEndOfDate(), shiftMin, shiftMax, storeIdAdd, empGroupId, brandId);

            var listResultCanApprove = Listemployees.ListCanApprove.Select(q => new IConvertible[] {
                count++,
                q.Name,
                q.Phone,
                q.Id
            });
            return Json(new { datatable = listResultCanApprove }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListTimeFrame(int brandId)
        {
            int count = 1;
            var timeFrameApi = new TimeFrameApi();
            var datatable = timeFrameApi.GetActive().Where(p => p.BrandId == brandId).Select(q => new IConvertible[] {
                count++,
                q.Name,
                q.StartTime.ToString(),
                q.EndTime.ToString(),
                q.Duration.ToString(),
                q.Id
            }).ToList();
            return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult detailsTimeFrame(int Id)
        {
            var timeFrameApi = new TimeFrameApi();
            var result = timeFrameApi.Get(Id);
            var tmpModel = new
            {
                Name = result.Name,
                StartTime = result.StartTime.ToString(),
                EndTime = result.EndTime.ToString(),
                //Duration = result.ExpandTime.ToString()
            };
            return Json(tmpModel);
        }


        public ActionResult CreateAttendance(int employeeId, int storeId, DateTime shiftMin, DateTime shiftMax, TimeSpan duration)
        {
            bool result = true;
            var api = new AttendanceApi();

            var model = new AttendanceViewModel()
            {
                EmployeeId = employeeId,
                Status = (int)StatusAttendanceEnum.Processing,
                ShiftMin = shiftMin,
                ShiftMax = shiftMax,
                ProcessingStatus = (int)ProcessingStatusAttendenceEnum.Assign,
                StoreId = storeId,
                ExpandTime = duration,
                Active = true,
            };
            try
            {
                api.Create(model);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return Json(result);
        }

        public ActionResult CreateMoreAttendanceAction(List<int> empIdList, int storeIdAdd, string startTime, string endTime, TimeSpan shiftMin, TimeSpan shiftMax, TimeSpan duration)
        {
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            bool result = false;
            for (DateTime i = startDate; DateTime.Compare(i, endDate) < 0; i = i.AddDays(1))
            {
                var current = i.GetStartOfDate();
                var timeStart = current.Add(shiftMin);
                var timeEnd = current.Add(shiftMax);
                try
                {
                    foreach (var item in empIdList)
                    {
                        CreateAttendance(item, storeIdAdd, timeStart, timeEnd, duration);
                    }
                    result = true;
                }
                catch (Exception e)
                {
                    result = false;
                }
            }
            return Json(result);
        }

        public JsonResult GetAttendance(int status, int empType, int isLate, string strDateChooseStart, string strDateChooseEnd)
        {
            var dateChooseStart = Utils.ToDateTime(strDateChooseStart);
            var dateChooseEnd = Utils.ToDateTime(strDateChooseEnd);
            int count = 1;
            var api = new AttendanceApi();
            var employeeApi = new EmployeeApi();


            var datatable = api.GetActive().Where(q => q.ShiftMin.Date >= dateChooseStart && q.ShiftMin.Date <= dateChooseEnd).ToList();
            if (status == 0)
            {
                datatable = datatable.Where(q => q.Status == 0).ToList();
            }
            else if (status == 1)
            {
                datatable = datatable.Where(q => q.Status == 1).ToList();
            }
            else if (status == 2)
            {
                datatable = datatable.Where(q => q.Status == 2).ToList();
            }

            if (empType == 0)
            {
                datatable = datatable.Where(q => q.ProcessingStatus == 0 ||
                                                 q.ProcessingStatus == 1 ||
                                                 q.ProcessingStatus == 2 ||
                                                 q.ProcessingStatus == 3 ||
                                                 q.ProcessingStatus == 5).ToList();
            }
            else if (empType == 1)
            {
                datatable = datatable.Where(q => q.ProcessingStatus == 6 ||
                                                 q.ProcessingStatus == 7).ToList();
            }

            if (isLate == 0)
            {
                datatable = datatable.Where(q => q.CheckMin - q.ShiftMin >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
            }
            else if (isLate == 1)
            {
                datatable = datatable.Where(q => q.ShiftMax - q.CheckMax >= q.ExpandTime && q.TotalWorkTime > new TimeSpan(0, 0, 1, 0)).ToList();
            }
            else if (isLate == 2)
            {
                datatable = datatable.Where(q => q.ShiftMax - q.CheckMax < q.ExpandTime && q.CheckMin - q.ShiftMin < q.ExpandTime).ToList();
            }


            var result = datatable.OrderBy(q => q.ProcessingStatus).ThenByDescending(q => q.TotalWorkTime).Select(q => new IConvertible[]
            {
                count++,
                employeeApi.Get(q.EmployeeId).Name,
                q.CheckMin == null ? "Chưa cập nhật" : q.CheckMin.Value.ToString("HH:mm"),
                q.CheckMax == null ? "Chưa cập nhật" : q.CheckMax.Value.ToString("HH:mm"),
                q.TotalWorkTime.ToString(),
                q.Id,
                (q.CheckMax==null||q.CheckMin==null)?"chưa check đủ":"check đủ",
                (q.CheckMin-q.ShiftMin>= q.ExpandTime)? "Đi trễ":"Chuẩn vào",
                (q.ShiftMax-q.CheckMax>= q.ExpandTime)? "Về sớm":"Chuẩn ra",
                q.Status == null ? "N/a" : ((StatusAttendanceEnum)q.Status).DisplayName(),
                q.UpdatePerson == null ? "N/a" : q.UpdatePerson,
                q.ProcessingStatus == null ? "N/a" : ((ProcessingStatusAttendenceEnum)q.ProcessingStatus).DisplayName(),
                q.ShiftMin.ToString("HH:mm"),
                q.ShiftMax.ToString("HH:mm")
            }).ToList();
            TimeSpan totalWorkTimeOfShift = new TimeSpan(datatable.Sum(q => q.TotalWorkTime.GetValueOrDefault().Ticks));
            var timenull = TimeSpan.Zero;
            return Json(new
            {
                result = result,
            }, JsonRequestBehavior.AllowGet);
        }
    }
}