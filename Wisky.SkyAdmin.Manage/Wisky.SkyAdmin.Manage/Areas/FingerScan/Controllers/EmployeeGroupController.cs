using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers
{
    public class EmployeeGroupController : Controller
    {
        // GET: FingerScan/EmployeeGroup
        public ActionResult Index()
        {            
            return View();
        }

        public ActionResult GetAllStore(int brandId)
        {
            var storeApi = new StoreApi();            
            var list = storeApi.GetActive().Where(q => q.BrandId == brandId).ToList();
            return Json(new { rs = list }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllEmployeeToAdd(int storeIdAdd)
        {
            int count = 1;
            var employeeApi = new EmployeeApi();
            var datatable = employeeApi.GetActive().Where(q => q.MainStoreId == storeIdAdd && q.EmployeeGroupId == null).Select(q => new IConvertible[] {
                count++,
                q.Name,
                q.Phone,
                q.Id
            });
            return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllEmpGroup(int brandId)
        {
            int count = 1;
            var empGroupApi = new EmployeeGroupApi();
            var datatable = empGroupApi.GetActive().Where(p => p.BrandId == brandId).Select(q => new IConvertible[] {
                count++,
                q.CodeGroup,
                q.NameGroup,
                //q.ExpandTime.ToString(),
                q.Id
            }).ToList();
            return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        }
        #region Old code 30/03/2018
        //public ActionResult CreateEmpGroup(int brandId, /*List<int> empIdList,*/ string code, string name, string strExpandTime)
        //{
        //    bool result = false;
        //    string message = "";
        //    var empGroupApi = new EmployeeGroupApi();
        //    var checkDuplicate = empGroupApi.GetActive().Count(q => q.CodeGroup == code);
        //    if (checkDuplicate > 0)
        //    {
        //        message = "Mã này đã tồn tại. Vui lòng nhập mã khác";
        //        return Json(new { rs = result, mess = message }, JsonRequestBehavior.AllowGet);
        //    }
        //    var empApi = new EmployeeApi();
        //    var check = strExpandTime.Split('_');
        //    var expandTime = new TimeSpan(0, 0, 0);
        //    if (check[1].Equals("h"))
        //    {
        //        expandTime = expandTime.Add(new TimeSpan(int.Parse(check[0]), 0, 0));
        //    }
        //    else if (check[1].Equals("m"))
        //    {
        //        expandTime = expandTime.Add(new TimeSpan(0, int.Parse(check[0]), 0));
        //    }
        //    try
        //    {
        //        var empGroupModel = new EmployeeGroupViewModel()
        //        {
        //            CodeGroup = code,
        //            NameGroup = name,
        //            ExpandTime = expandTime,
        //            BrandId = brandId,
        //            Active = true,
        //           GroupPolicy = (int)GroupPolicy.RoundToPoint,
        //        };
        //        empGroupApi.Create(empGroupModel);

        //        //foreach (var item in empIdList)
        //        //{
        //        //    var emp = empApi.Get(item);
        //        //    emp.EmployeeGroupId = empGroupModel.Id;
        //        //    empApi.Edit(item, emp);
        //        //}
        //        message = "Thêm nhóm mới thành công";
        //        result = true;
        //    }
        //    catch (Exception e)
        //    {
        //        message = "Có lỗi trong quá trình xử lý, vui lòng liên hệ admin";
        //        result = false;
        //    }

        //    return Json(new {rs = result, mess = message}, JsonRequestBehavior.AllowGet);
        //}
        #endregion
        public ActionResult CreateEmpGroup(int brandId, /*List<int> empIdList,*/ string code, string name, string checkinExpandtime, string checkoutExpandtime)
        {
            bool result = false;
            string message = "";
            var empGroupApi = new EmployeeGroupApi();
            var checkDuplicate = empGroupApi.GetActive().Count(q => q.CodeGroup == code);
            if (checkDuplicate > 0)
            {
                message = "Mã này đã tồn tại. Vui lòng nhập mã khác";
                return Json(new { rs = result, mess = message }, JsonRequestBehavior.AllowGet);
            }
            var empApi = new EmployeeApi();
            try
            {
                var empGroupModel = new EmployeeGroupViewModel()
                {
                    CodeGroup = code,
                    NameGroup = name,
                    BrandId = brandId,
                    Active = true

                };
                empGroupApi.Create(empGroupModel);

                //foreach (var item in empIdList)
                //{
                //    var emp = empApi.Get(item);
                //    emp.EmployeeGroupId = empGroupModel.Id;
                //    empApi.Edit(item, emp);
                //}
                message = "Thêm nhóm mới thành công";
                result = true;
            }
            catch (Exception e)
            {
                message = "Có lỗi trong quá trình xử lý, vui lòng liên hệ admin";
                result = false;
            }
            return Json(new { rs = result, mess = message }, JsonRequestBehavior.AllowGet);
        }
        #region Old code 30/03/2018
        //public ActionResult PrepareEdit(int empGroupId)
        //{
        //    var employeeGroupApi = new EmployeeGroupApi();
        //    var model = employeeGroupApi.Get(empGroupId);
        //    var tmpModel = new
        //    {
        //        id = model.Id,
        //        code = model.CodeGroup,
        //        name = model.NameGroup,
        //        expTime = model.ExpandTime.ToString().Split(':')[0] == "00" ? model.ExpandTime.ToString().Split(':')[1] + "_m" : model.ExpandTime.ToString().Split(':')[0] + "_h"
        //    };
        //    return Json(new { rs = tmpModel }, JsonRequestBehavior.AllowGet);
        //}
        #endregion
        public ActionResult PrepareEdit(int empGroupId)
        {
            var employeeGroupApi = new EmployeeGroupApi();
            var model = employeeGroupApi.Get(empGroupId);
            var tmpModel = new
            {
                id = model.Id,
                code = model.CodeGroup,
                name = model.NameGroup,                
            };
            return Json(new { rs = tmpModel }, JsonRequestBehavior.AllowGet);
        }
        #region Old code 30/03/2018         
        //public ActionResult EditEmpGroup(int id, string code, string name, string strExpandTime, bool isChange)
        //{
        //    bool result = false;
        //    string message = "";
        //    var employeeGroupApi = new EmployeeGroupApi();
        //    if (isChange)
        //    {
        //        var checkDuplicate = employeeGroupApi.GetActive().Count(q => q.CodeGroup == code);
        //        if (checkDuplicate > 0)
        //        {
        //            message = "Mã này đã tồn tại. Vui lòng nhập mã khác";
        //            return Json(new { rs = result, mess = message }, JsonRequestBehavior.AllowGet);
        //        }
        //    }

        //    var model = employeeGroupApi.Get(id);
        //    var check = strExpandTime.Split('_');
        //    var expandTime = new TimeSpan(0, 0, 0);
        //    if (check[1].Equals("h"))
        //    {
        //        expandTime = expandTime.Add(new TimeSpan(int.Parse(check[0]), 0, 0));
        //    }
        //    else if (check[1].Equals("m"))
        //    {
        //        expandTime = expandTime.Add(new TimeSpan(0, int.Parse(check[0]), 0));
        //    }


        //    try
        //    {
        //        model.NameGroup = name;
        //        model.CodeGroup = code;
        //        model.ExpandTime = expandTime;
        //        model.GroupPolicy = (int)GroupPolicy.RoundToPoint;
        //        employeeGroupApi.Edit(model.Id, model);
        //        message = "Sửa thông tin thành công";
        //        result = true;
        //    }
        //    catch (Exception e)
        //    {
        //        message = "Có lỗi trong quá trình xử lý, vui lòng liên hệ admin";
        //        result = false;
        //    }
        //    return Json(new { rs = result, mess = message }, JsonRequestBehavior.AllowGet);
        //}
        #endregion
        public ActionResult EditEmpGroup(string id, string code, string name, bool isChange)
        {
            bool result = false;
            string message = "";
            var employeeGroupApi = new EmployeeGroupApi();
            if (isChange)
            {
                var checkDuplicate = employeeGroupApi.GetActive().Count(q => q.CodeGroup == code);
                if (checkDuplicate > 0)
                {
                    message = "Mã này đã tồn tại. Vui lòng nhập mã khác";
                    return Json(new { rs = result, mess = message }, JsonRequestBehavior.AllowGet);
                }
            }
            var model = employeeGroupApi.Get(int.Parse(id));
            try
            {
                model.NameGroup = name;
                model.CodeGroup = code;                
                employeeGroupApi.Edit(model.Id, model);
                message = "Sửa thông tin thành công";
                result = true;
            }
            catch (Exception e)
            {
                message = "Có lỗi trong quá trình xử lý, vui lòng liên hệ admin";
                result = false;
            }
            return Json(new { rs = result, mess = message }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DeleteEmpGroup(int empGroupId)
        {
            bool result = false;
            string message = "";
            var empGroupApi = new EmployeeGroupApi();
            var empApi = new EmployeeApi();
            var timeFrameApi = new TimeFrameApi();
            try
            {
                var empList = empApi.GetActive().Where(q => q.EmployeeGroupId == empGroupId).ToList();
                var timeFrameList = timeFrameApi.GetActive().Where(q => q.EmployeeGroupId == empGroupId).ToList();
                if (empList.Count() > 0 && timeFrameList.Count() <= 0)
                {
                    message = "Nhóm này hiện đang có nhân viên, vui lòng xóa nhân viên trước";
                    return Json(new {rs = result, mess = message}, JsonRequestBehavior.AllowGet);

                }

                if (timeFrameList.Count() > 0 && empList.Count() <= 0)
                {
                    message = "Nhóm này hiện đang có khung giờ, vui lòng xóa khung giờ trước";
                    return Json(new { rs = result, mess = message }, JsonRequestBehavior.AllowGet);

                }

                if (timeFrameList.Count() > 0 && empList.Count() > 0)
                {
                    message = "Nhóm này hiện đang có khung giờ và nhân viên, vui lòng xóa khung giờ, nhân viên trước";
                    return Json(new { rs = result, mess = message }, JsonRequestBehavior.AllowGet);

                }

                var empGroup = empGroupApi.Get(empGroupId);
                empGroup.Active = false;
                empGroupApi.Edit(empGroup.Id, empGroup);
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return Json(new { rs = result }, JsonRequestBehavior.AllowGet);
        }               
    }
}