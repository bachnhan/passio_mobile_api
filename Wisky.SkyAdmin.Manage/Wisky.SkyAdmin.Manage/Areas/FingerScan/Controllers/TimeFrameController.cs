using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Sdk;
using HmsService.Models;
using HmsService.ViewModels;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers
{
    public class TimeFrameController : Controller
    {
        // GET: FingerScan/TimeFrame
        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GetAllGroupEmp(int brandId)
        {
            var apiGroupEmp = new EmployeeGroupApi();
            var list = apiGroupEmp.GetActive().Where(q => q.BrandId == brandId).ToList();
            return Json(list);
        }
        #region Old code 30/03/2018
        //public ActionResult GetAllTimeFrame(int brandId, int groupID, bool? isOvertime)
        //{
        //    int count = 1;
        //    var apiTimeFrame = new TimeFrameApi();
        //    var apiGroupEmp = new EmployeeGroupApi();
        //    var list = apiTimeFrame.GetActive().Where(q => q.BrandId == brandId);
        //    if(groupID != -1)
        //    {
        //        list = apiTimeFrame.GetActive().Where(q => q.BrandId == brandId && q.EmployeeGroupId == groupID);
        //    }
        //    if (isOvertime == true)
        //    {
        //        list = list.Where(q => q.IsOverTime == true);
        //    }
        //    var datatable = list.ToList().Select(q => new IConvertible[] {
        //        count++,
        //        q.Name,
        //        q.StartTime.ToString(@"hh\:mm"),
        //        q.EndTime.ToString(@"hh\:mm"),
        //        q.BreakTime.ToString(@"hh\:mm"),
        //        q.Duration.ToString(@"hh\:mm"),              
        //        apiGroupEmp.Get(q.EmployeeGroupId).NameGroup,
        //        q.Id
        //    });
        //    return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        //}
        #endregion 
        //public ActionResult GetAllTimeFrame(int brandId, int groupID, bool? isOvertime)
        //{
        //    int count = 1;
        //    var apiTimeFrame = new TimeFrameApi();
        //    var apiGroupEmp = new EmployeeGroupApi();
        //    var list = apiTimeFrame.GetActive().Where(q => q.BrandId == brandId);
        //    if (groupID != -1)
        //    {
        //        list = apiTimeFrame.GetActive().Where(q => q.BrandId == brandId && q.EmployeeGroupId == groupID);
        //    }
        //    if (isOvertime == true)
        //    {
        //        list = list.Where(q => q.IsOverTime == true);
        //    }
        //    var datatable = list.ToList().Select(q => new IConvertible[] {
        //        count++,
        //        q.Name,
        //        q.StartTime.ToString(@"hh\:mm"),
        //        q.EndTime.ToString(@"hh\:mm"),
        //        q.BreakTime.ToString(@"hh\:mm"),
        //        q.Duration.ToString(@"hh\:mm"),
        //        apiGroupEmp.Get(q.EmployeeGroupId).NameGroup,
        //        q.Id,
        //        q.InMode,
        //        q.OutMode,
        //        q.BreakCount
        //    });
        //    return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult GetAllTimeFrame(int brandId, int storeId, bool? isOvertime)
        {
            int count = 1;
            var apiTimeFrame = new TimeFrameApi();
            var apiStore = new StoreApi();
            var list = apiTimeFrame.BaseService.Get(q => q.Active == true && q.BrandId == brandId);
            var store = apiStore.BaseService.Get(q=>q.isAvailable == true && q.ID == storeId).Select(s => s.AttendanceStoreFilter).ToArray();            
            if (store != null)
            {
                var temp = 7;
                if (store.Length != 0)
                {
                    temp = (int)Math.Pow(2, Convert.ToDouble(store[0]));
                }
                list = apiTimeFrame.BaseService.Get(q=>q.Active == true && q.BrandId == brandId && (temp & q.StoreFilter) != 0);
            }
            //if (isOvertime == true)
            //{
            //    list = list.Where(q => q.IsOverTime == true);
            //}
            var datatable = list.ToList().Select(q => new IConvertible[] {
                count++,
                q.Name,
                q.StartTime.ToString(@"hh\:mm"),
                q.EndTime.ToString(@"hh\:mm"),
                q.BreakTime.ToString(@"hh\:mm"),
                q.Duration.ToString(@"hh\:mm"),                
                q.Id
            }).ToList();
            return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetAllTimeFrameFree(int brandId, int storeId, bool? isOvertime, string date, int empId)
        {
            var startTime = date.ToDateTime().GetStartOfDate();
            var endTime = date.ToDateTime().GetEndOfDate();
            int count = 1;
            var apiTimeFrame = new TimeFrameApi();            
            var apiAttendance = new AttendanceApi();
            var list = apiTimeFrame.BaseService.Get(q => q.Active == true && q.BrandId == brandId);
            var att = apiAttendance.BaseService.Get(q => q.Active == true && q.ShiftMin >= startTime && q.ShiftMax <= endTime && q.EmployeeId == empId);
                        
            if (isOvertime == true)
            {
                list = list.Where(q => q.IsOverTime == true);
            }
            if(att.Any())
            {
                var shiftMin = att.ToList().FirstOrDefault().ShiftMin.TimeOfDay;
                var shiftMax = att.ToList().FirstOrDefault().ShiftMax.TimeOfDay;
                list = apiTimeFrame.BaseService.Get(q => q.Active == true && ((q.StartTime > shiftMin && q.StartTime > shiftMax) ||
                                                                                (q.EndTime < shiftMin && q.EndTime < shiftMax) ));
            }
            var datatable = list.ToList().Select(q => new IConvertible[] {
                count++,
                q.Name,
                q.StartTime.ToString(@"hh\:mm"),
                q.EndTime.ToString(@"hh\:mm"),
                q.BreakTime.ToString(@"hh\:mm"),
                q.Duration.ToString(@"hh\:mm"),
                q.Id,
                q.InMode,
                q.OutMode,
                q.BreakCount
            }).ToList();
            return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "BrandManager")]
        #region Old code 30/03/2018
        //public ActionResult EditTimeFrame(int brandId , int idtp,  String name, TimeSpan startTime, TimeSpan endTime, TimeSpan breakTime, int groupEmp, TimeSpan duration)
        //{
        //    //var duration = endTime - startTime;
        //    bool result = false;
        //    var apiTimeFrame = new TimeFrameApi();
        //    var model =new TimeFrameViewModel();
        //    model = apiTimeFrame.Get(idtp);
        //    model.EmployeeGroupId = groupEmp;
        //    model.Name = name;
        //    model.StartTime = startTime;
        //    model.EndTime = endTime;
        //    model.BreakTime = breakTime;
        //    model.Duration = duration;
        //    try
        //    {
        //        apiTimeFrame.Edit(idtp, model);
        //        result = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result = false;
        //    }
        //    return Json(new { rs = result }, JsonRequestBehavior.AllowGet);
        //}
        #endregion
        public ActionResult EditTimeFrame(int brandId, int idtp, String name, String startTime, String endTime, String breakTime, int groupEmp, String duration, int inmode, int outmode, int breakcount, String checkinExpandtime, String checkoutExpandtime, int storeFilter)
        {
            var startTimeT = Utils.ToHourTime(startTime);
            var endTimeT = Utils.ToHourTime(endTime);
            var breakTimeT = Utils.ToHourTime(breakTime);
            var durationT = Utils.ToHourTime(duration);
            var checkinexpandTime = Utils.ToHourTime(checkinExpandtime);
            var checkoutexpandTime = Utils.ToHourTime(checkoutExpandtime);

            bool result = false;
            var apiTimeFrame = new TimeFrameApi();
            var model = new TimeFrameViewModel();
            model = apiTimeFrame.Get(idtp);
            model.EmployeeGroupId = groupEmp;
            model.Name = name;
            model.StartTime = startTimeT.TimeOfDay;
            model.EndTime = endTimeT.TimeOfDay;
            model.BreakTime = breakTimeT.TimeOfDay;
            model.Duration = durationT.TimeOfDay;
            model.InMode = inmode;
            model.OutMode = outmode;
            model.BreakCount = breakcount;
            model.CheckInExpandTime = checkinexpandTime.TimeOfDay;
            model.CheckOutExpandTime = checkoutexpandTime.TimeOfDay;
            if (storeFilter < 1 || storeFilter > 7) throw new Exception();
            model.StoreFilter = storeFilter;
            try
            {
                apiTimeFrame.Edit(idtp, model);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return Json(new { rs = result }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "BrandManager")]
        #region Old code 30/03/2018
        //public ActionResult CreateTimeFrame(int brandId, String name, TimeSpan startTime, TimeSpan endTime , TimeSpan breakTime, int groupEmp, TimeSpan duration)
        //{
        //    bool result = false;
        //    var apiTimeFrame = new TimeFrameApi();
        //    var model = new TimeFrameViewModel()
        //    {
        //        EmployeeGroupId = groupEmp,
        //        BrandId = brandId,
        //        Name = name,
        //        StartTime = startTime,
        //        EndTime = endTime,
        //        Duration = duration,
        //        BreakTime = breakTime,
        //        Active = true,

        //    };
        //    try
        //    {
        //        apiTimeFrame.Create(model);
        //        result = true;
        //    }
        //    catch (Exception e)
        //    {
        //        result = false;
        //    }
        //    return Json(new { rs = result }, JsonRequestBehavior.AllowGet);
        //}
        #endregion
        public ActionResult CreateTimeFrame(int brandId, String name, String startTime, String endTime, String breakTime, int groupEmp, String duration, int inmode, int outmode, int breakcount, string checkinExpandtime, string checkoutExpandtime, int storeFilter)
        {
            bool result = false;
            var apiTimeFrame = new TimeFrameApi();

            var startTimeT = Utils.ToHourTime(startTime);
            var endTimeT = Utils.ToHourTime(endTime);
            var breakTimeT = Utils.ToHourTime(breakTime);
            var durationT = Utils.ToHourTime(duration);
            //var check1 = checkinExpandtime.Split(':');
            //var check2 = checkoutExpandtime.Split(':');

            var checkinexpandTime = Utils.ToHourTime(checkinExpandtime);
            var checkoutexpandTime = Utils.ToHourTime(checkoutExpandtime);
            //checkinexpandTime = checkinexpandTime.Add(int.Parse(check1[0]));


            //checkinexpandTime = (check1[1].Equals("h")) ? checkinexpandTime.Add(new TimeSpan(int.Parse(check1[0]), 0, 0)) : checkinexpandTime.Add(new TimeSpan(0, int.Parse(check1[0]), 0));
            //checkoutexpandTime = (check2[1].Equals("h")) ? checkoutexpandTime.Add(new TimeSpan(int.Parse(check2[0]), 0, 0)) : checkoutexpandTime.Add(new TimeSpan(0, int.Parse(check2[0]), 0));

            //if (check1[1].Equals("h") && check2[1].Equals("h"))
            //{
            //    checkinexpandTime = checkinexpandTime.Add(new TimeSpan(int.Parse(check1[0]), 0, 0));
            //    checkoutexpandTime = checkoutexpandTime.Add(new TimeSpan(int.Parse(check2[0]), 0, 0));
            //}
            //else if (check1[1].Equals("m") && check2[1].Equals("m"))
            //{
            //    checkinexpandTime = checkinexpandTime.Add(new TimeSpan(0, int.Parse(check1[0]), 0));
            //    checkoutexpandTime = checkoutexpandTime.Add(new TimeSpan(0, int.Parse(check2[0]), 0));
            //}

            // check bang enum k phai 1,7
            if (storeFilter < 1 || storeFilter > 7) throw new Exception();

            var model = new TimeFrameViewModel()
            {
                EmployeeGroupId = groupEmp,
                BrandId = brandId,
                Name = name,
                StartTime = startTimeT.TimeOfDay,
                EndTime = endTimeT.TimeOfDay,
                Duration = durationT.TimeOfDay,
                BreakTime = breakTimeT.TimeOfDay,
                InMode = inmode,
                OutMode = outmode,
                BreakCount = breakcount,
                CheckInExpandTime = checkinexpandTime.TimeOfDay,
                CheckOutExpandTime = checkoutexpandTime.TimeOfDay,
                Active = true,
                StoreFilter = storeFilter
            };
            try
            {
                apiTimeFrame.Create(model);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return Json(new { rs = result }, JsonRequestBehavior.AllowGet);
        }
        #region Old code 30/03/2018
        //public ActionResult prepareEdit(int TimeFrameId)
        //{
        //    var apiTimeFrame = new TimeFrameApi();
        //    var apiGroupEmp = new EmployeeGroupApi();
        //    var model = apiTimeFrame.Get(TimeFrameId);
        //    var tmpModel = new
        //    {
        //        id = model.Id,
        //        name = model.Name,
        //        startTime = model.StartTime.ToString(@"hh\:mm"),
        //        endTime = model.EndTime.ToString(@"hh\:mm"),
        //        duration = model.Duration.ToString(@"hh\:mm"),
        //        breakTime = model.BreakTime.ToString(@"hh\:mm"),
        //        group = apiGroupEmp.Get(model.EmployeeGroupId).Id
        //    };
        //    return Json(new { rs = tmpModel }, JsonRequestBehavior.AllowGet);
        //}
        #endregion
        public ActionResult prepareEdit(int TimeFrameId)
        {
            var apiTimeFrame = new TimeFrameApi();
            var apiGroupEmp = new EmployeeGroupApi();
            var model = apiTimeFrame.Get(TimeFrameId);
            //var checkinresult = (model.CheckInExpandTime > new TimeSpan(0, 59, 00)) ? model.CheckInExpandTime.Value.ToString(@"hh\:mm"):"";
            //var checkoutresult = (model.CheckOutExpandTime > new TimeSpan(0, 59, 00)) ? model.CheckOutExpandTime.Value.ToString(@"hh\:mm"):"";
            //var checkinresult = (model.CheckInExpandTime > new TimeSpan(0, 59, 0)) ? "0" + model.CheckInExpandTime.Value.Hours + "_h" : model.CheckInExpandTime.Value.Minutes + "_m";
            //var checkoutresult = (model.CheckOutExpandTi1me > new TimeSpan(0, 59, 0)) ? "0" + model.CheckOutExpandTime.Value.Hours + "_h" : model.CheckOutExpandTime.Value.Minutes + "_m";


            var tmpModel = new
            {
                id = model.Id,
                name = model.Name,
                startTime = model.StartTime.ToString(@"hh\:mm"),
                endTime = model.EndTime.ToString(@"hh\:mm"),
                duration = model.Duration.ToString(@"hh\:mm"),
                breakTime = model.BreakTime.ToString(@"hh\:mm"),
                inmode = model.InMode,
                outmode = model.OutMode,
                breakcount = model.BreakCount,
               // checkin = model.CheckInExpandTime.HasValue ? model.CheckInExpandTime.Value.ToString(@"hh\:mm") : "",
             //   checkout = model.CheckOutExpandTime.HasValue ? model.CheckOutExpandTime.Value.ToString(@"hh\:mm") : "",
                //checkin = checkinresult,
                //checkout = checkoutresult,
                group = apiGroupEmp.Get(model.EmployeeGroupId).Id,
                storeFilter = model.StoreFilter
            };
            return Json(new { rs = tmpModel }, JsonRequestBehavior.AllowGet);
        }
        [Authorize(Roles = "BrandManager")]
        public ActionResult DeleteTimeFrame(int TimeFrameId)
        {
            var result = true;
            var apiTimeFrame = new TimeFrameApi();
            var entity = apiTimeFrame.Get(TimeFrameId);

            if (entity == null)
            {
                result = false;

            }
            try
            {
                entity.Active = false;
                apiTimeFrame.Edit(TimeFrameId, entity);
            }
            catch (Exception ex)
            {
                result = false;
            }
            return Json(result);
        }
    }
}