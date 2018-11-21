using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan
{
    public class ManagementRequestController : Controller
    {
        // GET: FingerScan/ManagementRequest
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadSessionAttendance(JQueryDataTableParamModel param, /*int employeeId,*/ string startDate, string endDate, int status, int storeId)
        {
            var attendanceApi = new AttendanceApi();
            var timeFrameApi = new TimeFrameApi();

            var startTime = startDate.ToDateTime().GetStartOfDate();

            var endTime = endDate.ToDateTime().GetEndOfDate();

            IEnumerable<Attendance> listResult = attendanceApi.GetAttendanceByRequestByTimeRange(storeId, startTime, endTime);
            
            var totalResult = listResult.Count();
            if (totalResult > 0)
            {
                switch (status)
                {
                    case (int)StatusAttendance.OnTime:
                        listResult = listResult.Where(q => q.CheckMin <= (q.ShiftMin.Add(q.ExpandTime)) && q.CheckMax >= (q.ShiftMax - q.ExpandTime));
                        break;
                    case (int)StatusAttendance.ComeLate:
                        listResult = listResult.Where(q => q.CheckMin > (q.ShiftMin.Add(q.ExpandTime)));
                        break;
                    case (int)StatusAttendance.ReturnEarly:
                        listResult = listResult.Where(q => q.CheckMax < (q.ShiftMax.Add(-q.ExpandTime)));
                        break;
                    case (int)StatusAttendance.Miss:
                        listResult = listResult.Where(q => q.CheckMin == null && q.CheckMax == null);
                        break;
                    case (int)StatusAttendance.Bothviolate:
                        listResult = listResult.Where(q => q.CheckMax < (q.ShiftMax.Add(-q.ExpandTime)) && q.CheckMin > (q.ShiftMin.Add(q.ExpandTime)));
                        break;
                }
            }
            var totalDisplay = listResult.Count();

            var count = param.iDisplayStart + 1;

            var currentDate = Utils.GetCurrentDateTime();
            var startInDate = currentDate.GetEndOfDate();
            var endInDate = currentDate.GetEndOfDate();

            var result = listResult.OrderBy(q => q.ShiftMax)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList()
                .Select(a => new IConvertible[]
                {
                    count++,
                    a.Employee.Name,
                    a.ShiftMin.ToString("dd/MM/yyyy",CultureInfo.InvariantCulture) + " - Từ " + a.ShiftMin.ToString(@"HH\:mm",CultureInfo.InvariantCulture) + " đến " + a.ShiftMax.ToString(@"HH\:mm",CultureInfo.InvariantCulture),
                    a.CheckMin != null ? a.CheckMin.Value.ToString(@"HH\:mm",CultureInfo.InvariantCulture) : "Chưa cập nhật",
                    a.CheckMax != null ? a.CheckMax.Value.ToString(@"HH\:mm", CultureInfo.InvariantCulture) : "Chưa cập nhật",
                    //a.TotalWorkTime == null  ? "N/A" : DateTime.Today.Add(a.TotalWorkTime.Value).ToString(@"HH\:mm", CultureInfo.InvariantCulture),
                    (a.CheckMin == null && a.CheckMax == null) ? (int)StatusAttendance.Miss :
                        (a.CheckMax < (a.ShiftMax.Add(-a.ExpandTime)) && a.CheckMin > (a.ShiftMin.Add(a.ExpandTime))) ? (int)StatusAttendance.Bothviolate :
                            (a.CheckMax < (a.ShiftMax.Add(-a.ExpandTime))) ? (int)StatusAttendance.ReturnEarly :
                                (a.CheckMin > (a.ShiftMin.Add(a.ExpandTime))) ? (int)StatusAttendance.ComeLate: (int)StatusAttendance.OnTime,
                    (a.ShiftMin >= startInDate && a.ShiftMin <= endInDate) ? (int)CurrentStatusEnum.Current : (a.ShiftMin < startInDate && a.ShiftMin <endInDate) ? (int)CurrentStatusEnum.Past : (int)CurrentStatusEnum.Future,
                    a.RequestedCheckIn != null ? a.RequestedCheckIn.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    a.RequestedCheckOut != null ? a.RequestedCheckOut.Value.ToString("dd/MM/yyyy HH:mm") : "",
                    a.Id,
                    a.IsRequested
                });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalResult,
                iTotalDisplayRecords = totalDisplay,
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckRequest(int attId, int type)
        {
            var user = HttpContext.User.Identity.Name;
            bool result = false;
            var attApi = new AttendanceApi();
            var att = attApi.Get(attId);
            try
            {
                if (type == (int) IsRequest.Approved)
                {
                    att.IsRequested = (int) IsRequest.Approved;
                    att.CheckMin = att.RequestedCheckIn;
                    att.CheckMax = att.RequestedCheckOut;
                    att.ApprovePerson = user;
                    attApi.Edit(attId, att);
                    result = true;
                }
                else if (type == (int)IsRequest.Reject)
                {
                    att.IsRequested = (int)IsRequest.Reject;
                    att.ApprovePerson = user;
                    attApi.Edit(attId, att);
                    result = true;
                }
                else if (type == (int)IsRequest.ApprovedNewAttendance)
                {
                    att.IsRequested = (int)IsRequest.ApprovedNewAttendance;
                    att.ApprovePerson = user;
                    attApi.Edit(attId, att);
                    result = true;
                }
                else if (type == (int)IsRequest.RejectNewAttendance)
                {
                    att.IsRequested = (int)IsRequest.RejectNewAttendance;
                    att.ApprovePerson = user;
                    attApi.Edit(attId, att);
                    result = true;
                }
            }
            catch (Exception e)
            {
                result = false;
            }

            return Json(new {rs = result}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateNoteRequest(int attId, string note)
        {
            bool result = false;
            var attApi = new AttendanceApi();
            var att = attApi.Get(attId);
            try
            {
                att.NoteRequest = note;
                attApi.Edit(attId, att);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }

            return Json(new { rs = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetNoteRequest(int attId)
        {
            var attApi = new AttendanceApi();
            var note = attApi.Get(attId).NoteRequest;
            return Json(new { rs = note }, JsonRequestBehavior.AllowGet);
        }
    }
}