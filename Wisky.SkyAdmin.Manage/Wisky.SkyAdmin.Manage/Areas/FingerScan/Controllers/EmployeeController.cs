using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Sdk;
using HmsService.Models;
using HmsService.ViewModels;
using System.Globalization;
using System.Windows.Forms;
using System.IO;
using Newtonsoft.Json;
using Remotion.FunctionalProgramming;
using Wisky.SkyAdmin.Manage.Areas.FingerScan.Models.SkyfiInfoModel;
using HmsService.Models.Entities;
using Wisky.SkyAdmin.Manage.Controllers;
using System.Data.Entity;
using System.Drawing;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Wisky.SkyAdmin.Manage.Areas.FingerScan.Controllers
{
    public class CalendarAttendance
    {
        public string date;
        public List<TimeSlot> timeslot;
        public TimeSpan totalWork;
    }
    public class TimeSlot
    {
        public double duration;
        public double startHour;
        public string status;
        public string color;
        public int idAttendance;
        public string LeaveEarly;
        public string Late;
    }
    public class EmployeeController : DomainBasedController
    {
        // GET: FingerScan/Employee
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ManageEmployeeAtBrand()
        {
            return View();
        }
        public ActionResult GetAllEmployeeModal()
        {
            var api = new EmployeeApi();
            var datatable = api.GetActive().ToList();
            return Json(datatable);
        }

        public ActionResult GetListStore(int storeId, int brandId)
        {
            var storeApi = new StoreApi();
            var listStore = storeApi.GetAllActiveStore(brandId).Where(q => q.ID != storeId).ToList();
            var result = listStore.Select(q => new
            {
                storeId = q.ID,
                storeName = q.Name
            });
            return Json(new { listresult = result }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult GetEmployeeByStoreId(JQueryDataTableParamModel param, int storeIdCode, int storeId)
        {
            var employeeApi = new EmployeeApi();
            var mappingEmployeeApi = new EmployeeInStoreApi();
            var count = param.iDisplayStart + 1;

            IEnumerable<Employee> listEmployee = employeeApi.GetEmployeeStoredIdIQ(storeIdCode);
            var totalRecord = listEmployee.Count();
            if (!String.IsNullOrEmpty(param.sSearch))
            {
                listEmployee = listEmployee.Where(q => q.Name.Contains(param.sSearch));
            }
            var totalDisplay = listEmployee.Count();
            var result = listEmployee.Select(q => new IConvertible[]{
                count++,
                q.Name,
                q.Phone,
                q.Address,
                q.EmpEnrollNumber,
                q.Id,
                mappingEmployeeApi.CheckEmployee(storeId, q.Id)
            }).ToList();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecord,
                iTotalDisplayRecords = totalDisplay,
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult AssignEmployee(List<int> listId, int storeId, int storeSelected)
        {
            if (listId == null)
            {
                listId = new List<int>();
            }
            var mappingApi = new EmployeeInStoreApi();
            IQueryable<EmployeeInStore> currentMappings = mappingApi.GetBaseMapping(storeId, storeSelected);
            try
            {
                var listNotcheck = currentMappings.Where(q => q.Active && !listId.Any(a => a == q.EmployeeId)).ToList();
                foreach (var item in listNotcheck)
                {
                    item.Active = false;
                    mappingApi.BaseService.Update(item);
                }
                foreach (var item in listId)
                {
                    var mapping = currentMappings.FirstOrDefault(q => q.EmployeeId == item);
                    if (mapping != null)
                    {
                        if (!mapping.Active)
                        {
                            mapping.Active = true;
                            mapping.AssignedDate = Utils.GetCurrentDateTime();
                            mappingApi.BaseService.Update(mapping);
                        }
                    }
                    else
                    {
                        mapping = new EmployeeInStore();
                        mapping.Active = true;
                        mapping.StoreId = storeId;
                        mapping.EmployeeId = item;
                        mapping.AssignedDate = Utils.GetCurrentDateTime();
                        mapping.Status = 1;
                        mappingApi.BaseService.Create(mapping);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetAllEmployeeFinger()
        {
            int count = 1;
            var api = new EmployeeApi();
            var apiEmployeeFinger = new EmployeeFingerApi();
            var datatable = apiEmployeeFinger.GetActive().Select(q => new IConvertible[] {
                count++,
                q.NameEmployeeInMachine,
                q.EmpEnrollNumber,
                api.Get(q.EmpId).Name,
                api.Get(q.EmpId).EmpEnrollNumber,
                api.Get(q.EmpId).Role == 3 ? "Admin" : "User",
                q.Id
            }).ToList();
            return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult GetAllEmployeeFingerSeverSide(JQueryDataTableParamModel param)
        {
            int count = 1;
            var api = new EmployeeApi();
            var apiEmployeeFinger = new EmployeeFingerApi();
            var datatable = apiEmployeeFinger.GetActive().ToList();
            var result = datatable.Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength).Select(q => new IConvertible[] {
                count++,
                q.NameEmployeeInMachine,
                q.EmpEnrollNumber,
                api.Get(q.EmpId).Name,
                api.Get(q.EmpId).EmpEnrollNumber,
                api.Get(q.EmpId).Role == 3 ? "Admin" : "User",
                q.Id
            }).ToList();
            var totalRecords = datatable.Count;
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ImportEmployees()
        {
            return null;

            //var attApi = new AttendanceApi();
            //var shiftApi = new ShiftApi();
            //var tfApi = new TimeFrameApi();
            //List<ShiftViewModel> listShift = new List<ShiftViewModel>();
            //var atts = attApi.GetActive().Where(a => a.EmployeeId == employeeId);
            //foreach (var att in atts)
            //{
            //    listShift.Add(shiftApi.Get(att.ShiftId));
            //}
            //var result = listShift.Where(a => a.StartTime >= DateTime.Now.Date && a.Active)
            //    .OrderBy(q => q.StartTime)
            //    .Select(a => new IConvertible[]
            //    {
            //        a.StartTime.Value.ToShortDateString(),
            //        tfApi.Get(a.TimeFrameId).Name,
            //        a.StartTime.ToString(),
            //        a.EndTime.ToString(),
            //        a.StartTime.Value.ToString("MM/dd/yyyy")
            //    }).ToList();
            //var empApi = new EmployeeApi();
            //var empName = empApi.Get(employeeId).Name;
            //return Json(new
            //{
            //    data = result,
            //    name = empName
            //});
        }


        public ActionResult prepareEditEmployFinger(int EmployeeFingerId)
        {
            var api = new EmployeeFingerApi();
            var employeeApi = new EmployeeApi();
            var model = api.Get(EmployeeFingerId);
            var employee = employeeApi.Get(model.EmpId);
            var Name = employee.Name;
            var EmpEnrollNumber = employee.EmpEnrollNumber;
            var Role = employee.Role;
            return Json(new { model = model, Name = Name, Role = Role, EmpEnrollNumber = EmpEnrollNumber }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult EditEmployeeFinger(int id, int EmpId)
        {
            int result = 0;
            var api = new EmployeeFingerApi();
            var model = new EmployeeFingerViewModel();
            var employeeApi = new EmployeeApi();
            var fingerPrint = employeeApi.Get(EmpId).EmpEnrollNumber;
            model = api.Get(id);
            model.EmpEnrollNumber = fingerPrint;
            model.EmpId = EmpId;
            try
            {
                api.Edit(id, model);
                result = 1;
            }
            catch (Exception e)
            {
                result = 0;
            }
            return Json(result);
        }

        public ActionResult GetAllEmployee(JQueryDataTableParamModel param, int storeId, int brandId)
        {
            var employeeApi = new EmployeeApi();
            var employeeInStoreApi = new EmployeeInStoreApi();
            var storeApi = new StoreApi();
            IEnumerable<EmployeeInStore> currentEmployeeInStore = employeeInStoreApi.GetCurrentEmployeeInStore(storeId);
            IEnumerable<Employee> employeesBrands = employeeApi.GetEmployeesByBrand(brandId);
            var listEmployeeInfo = from currentEmp in currentEmployeeInStore
                                   join empBrand in employeesBrands
                                   on currentEmp.EmployeeId equals empBrand.Id
                                   select new
                                   {
                                       empId = empBrand.Id,
                                       empCode = empBrand.EmployeeCode,
                                       empEnroll = empBrand.EmpEnrollNumber,
                                       empName = empBrand.Name,
                                       empStartDate = empBrand.DateStartWork,
                                       empPhone = empBrand.Phone,
                                       empSalary = empBrand.Salary,
                                       empBrandMainStore = empBrand.MainStoreId,
                                       empMappingStore = currentEmp.StoreId,
                                       empMappingId = currentEmp.Id
                                   };

            var totalResult = listEmployeeInfo.Count();

            // After Search
            if (!String.IsNullOrEmpty(param.sSearch))
            {
                listEmployeeInfo = listEmployeeInfo.Where(q => q.empName.ToUpper().Contains(param.sSearch.ToUpper()));
            }
            var totalDisplay = listEmployeeInfo.Count();

            var count = param.iDisplayStart + 1;

            // Return Data table
            var result = listEmployeeInfo.OrderByDescending(q => q.empId)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength).ToList().Select(q => new IConvertible[]
                {
                    count++,
                    q.empEnroll,
                    q.empName,
                    q.empPhone,
                    q.empStartDate == null ? "N/A": q.empStartDate.Value.ToString("dd/MM/yyyy"),
                    q.empSalary,
                    q.empId,
                    q.empBrandMainStore,
                    q.empMappingStore,
                    (q.empMappingStore != q.empBrandMainStore) ?storeApi.Get(q.empBrandMainStore).Name: ""
                });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalResult,
                iTotalDisplayRecords = totalDisplay,
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Get Employee by StoreId for datatable
        /// </summary>
        /// <param name="storeSelected"></param>
        /// <returns></returns>
        public ActionResult GetEmployeeByStore(string storeSelected, int brandId)
        {
            int count = 1;
            var employeeApi = new EmployeeApi();
            var storeId = 0;
            try
            {
                storeId = int.Parse(storeSelected);
            }
            catch (Exception) // Skip if Parse Fail
            {
            }
            var datatable = employeeApi.GetEmployeeByStoreId(storeId, brandId).Select(q => new IConvertible[]
            {
                    count++,
                    q.Name,
                    q.EmpEnrollNumber,
                    q.Phone == null ? "N/A" : q.Phone,
                    q.DateStartWork == null ? "N/A": q.DateStartWork.Value.ToString("dd/MM/yyyy"),
                    q.Salary,
                    q.Id,
            });
            return Json(new { data = datatable }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetEmployeeNonWorkingByStore(string storeSelected, int brandId)
        {
            int count = 1;
            var employeeApi = new EmployeeApi();
            var storeId = 0;
            try
            {
                storeId = int.Parse(storeSelected);
            }
            catch (Exception) // Skip if Parse Fail
            {
            }
            var datatable = employeeApi.GetEmployeeNonWorkingByStoreId(storeId, brandId).Select(q => new IConvertible[]
            {
                count++,
                q.Name,
                q.EmpEnrollNumber,
                q.Phone == null ? "N/A" : q.Phone,
                q.DateStartWork == null ? "N/A": q.DateStartWork.Value.ToString("dd/MM/yyyy"),
                q.Salary,
                q.Id,
            });
            return Json(new { data = datatable }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ChangeEmployee(int storeId, string EmpEnrollNumber)
        {
            var checkFingerApi = new CheckFingerApi();
            var checkFinger = checkFingerApi.GetCheckFingerByEmp(EmpEnrollNumber);
            try
            {
                foreach (var item in checkFinger)
                {
                    item.IsUpdated = false;
                    checkFingerApi.BaseService.Update(item);
                }
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }



        public ActionResult GetAllEmployeeToAdd(int storeId, int brandId, TimeSpan shiftMin, TimeSpan shiftMax, string endTime, string startTime/*, int empGroupId*/)
        {
            int count = 1;
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();
            var Listemployees = employeeApi.GetAllEmployeeFreeByTimeSpan1(startDate.GetStartOfDate(), endDate.GetEndOfDate(), shiftMin, shiftMax, storeId/*, empGroupId*/, brandId);
            var listResultCanApprove = Listemployees.ListCanApprove
            .Select(q => new IConvertible[] {
                count++,
                q.Name,
                q.Phone,
                q.Id,
                (q.MainStoreId != storeId)?storeApi.Get(q.MainStoreId).Name:"",
            });
            var listResultCanNotApprove = Listemployees.ListCannotApprove
            .Select(q => new IConvertible[] {
                count++,
                q.Name,
                q.Phone,
                q.Id,
                (q.MainStoreId != storeId)?storeApi.Get(q.MainStoreId).Name:"",
            });
            return Json(new { datatable = listResultCanApprove, datatableNotApprove = listResultCanNotApprove }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult CheckDuplicateFingerPrint(string fingerPrint)
        {
            bool result = false;
            var api = new EmployeeApi();
            var checkDuplicate = api.GetActive().Where(q => q.EmpEnrollNumber == fingerPrint).ToList();
            if (checkDuplicate.Count > 0)
            {
                result = false;
            }
            else
            {
                result = true;
            }
            return Json(result);
        }

        public ActionResult CreateEmployee(int storeId, int brandId, String name, String fingerPrint, String addr, String phone, string salary, String datestart, int mainStoreId, int empGroupId, string empCode, string empRecgency, String dob, int sex, string personalCardId, String datePersonalCard, string placeOfPersonalCard, string email, string mainAddr, string hometown, string placeBorn)
        {
            var employeeApi = new EmployeeApi();
            var employeeStoreApi = new EmployeeInStoreApi();

            var employee = employeeApi.GetEmployeeByEmp(fingerPrint);

            if (employee == null)
            {
                DateTime? datestartwork = null;
                DateTime? dateOfBirth = null;
                DateTime? parse_datePersonalCard = null;
                try
                {
                    datestartwork = Utils.ToDateTime(datestart);
                    dateOfBirth = Utils.ToDateTime(dob);
                    parse_datePersonalCard = Utils.ToDateTime(datePersonalCard);
                }
                catch (Exception)
                {
                }

                var checkDuplicate = employeeApi.GetActive().Count(q => q.EmployeeCode == empCode);
                if (checkDuplicate > 0)
                {
                    return Json(new { success = false, message = "Mã nhân viên đã tồn tại. Vui lòng nhập mã khác" }, JsonRequestBehavior.AllowGet);
                }

                employee = new Employee()
                {

                    MainStoreId = mainStoreId,
                    Name = name,
                    Address = addr != "" ? addr : null,
                    Phone = phone != "" ? phone : null,
                    Salary = decimal.Parse(salary != "" ? salary : "0"),
                    DateStartWork = datestartwork,
                    EmpEnrollNumber = fingerPrint,
                    Active = true,
                    BrandId = brandId,
                    EmployeeCode = empCode,
                    EmployeeRegency = empRecgency,
                    DateOfBirth = dateOfBirth,
                    PersonalCardId = personalCardId,
                    DatePersonalCard = parse_datePersonalCard,
                    PlaceOfPersonalCard = placeOfPersonalCard,
                    MainAddress = mainAddr,
                    EmployeePlaceBorn = placeBorn,
                    EmployeeHometown = hometown,
                    Email = email,
                    EmployeeSex = sex,
                    EmployeeGroupId = empGroupId,

                };


                try
                {
                    employeeApi.BaseService.Create(employee);
                    var mapping = new EmployeeInStore();
                    mapping.Active = true;
                    mapping.StoreId = mainStoreId;
                    mapping.Status = 1;
                    mapping.AssignedDate = Utils.GetCurrentDateTime();
                    mapping.EmployeeId = employee.Id;
                    employeeStoreApi.BaseService.Create(mapping);
                }
                catch (Exception e)
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra trong quá trình thêm nhân viên! Vui lòng liên hệ Admin" }, JsonRequestBehavior.AllowGet);
                }
                return Json(new { success = true, message = "Thêm nhân viên thành công" }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Mã chấm công đã tồn tại" }, JsonRequestBehavior.AllowGet);
            }

        }

        public ActionResult prepareEdit(int EmployeeId)
        {
            var api = new EmployeeApi();
            var model = api.Get(EmployeeId);
            var tmpModel = new
            {
                Id = model.Id,
                Name = model.Name,
                Address = model.Address,
                Phone = model.Phone,
                Salary = model.Salary,
                DateStartWork = model.DateStartWork == null ? "N/A" : model.DateStartWork.Value.ToString("dd/MM/yyyy"),
                EmpEnrollNumber = model.EmpEnrollNumber,
                StoreId = model.MainStoreId,
                EmpGroupId = model.EmployeeGroupId,
                EmployeeCode = model.EmployeeCode,
                EmployeeRegency = model.EmployeeRegency,
                DateOfBirth = model.DateOfBirth == null ? "N/A" : model.DateOfBirth.Value.ToString("dd/MM/yyyy"),
                PersonalCardId = model.PersonalCardId == null ? "N/A" : model.PersonalCardId,
                DatePersonalCard = model.DatePersonalCard == null ? "N/A" : model.DateOfBirth.GetValueOrDefault().ToString("dd/MM/yyyy"),
                PlaceOfPersonalCard = model.PlaceOfPersonalCard,
                MainAddress = model.MainAddress,
                EmployeePlaceBorn = model.EmployeePlaceBorn,
                EmployeeHometown = model.EmployeeHometown,
                Email = model.Email,
                EmployeeSex = model.EmployeeSex,

            };
            return Json(tmpModel);
        }

        public ActionResult EditEmployee(int brandId, int id, int mainStoreId, String name, String fingerPrint, String addr, String phone, string salary, String datestart, int empGroupId, string empCode, bool isChangEmpCode, string empRecgency, String dob, int sex, string personalCardId, String datePersonalCard, string placeOfPersonalCard, string email, string mainAddr, string hometown, string placeBorn)
        {
            int result = 0;
            var employeeApi = new EmployeeApi();
            var empInStoreApi = new EmployeeInStoreApi();

            DateTime? dateworkstart = null;
            DateTime? dateOfBirth = null;
            DateTime? parse_datePersonalCard = null;
            try
            {
                dateworkstart = Utils.ToDateTime(datestart);
                dateOfBirth = Utils.ToDateTime(dob);
                parse_datePersonalCard = Utils.ToDateTime(datePersonalCard);
            }
            catch (Exception)
            {
            }
            if (isChangEmpCode)
            {
                var checkDuplicate = employeeApi.GetActive().Count(q => q.EmployeeCode == empCode);
                if (checkDuplicate > 0)
                {
                    return Json(new { success = false, message = "Mã nhân viên đã tồn tại. Vui lòng nhập mã khác" }, JsonRequestBehavior.AllowGet);
                }
            }
            var currentEmployee = employeeApi.BaseService.Get(id);
            try
            {
                if (currentEmployee != null)
                {
                    if (mainStoreId != currentEmployee.MainStoreId) // Nếu có sự thay đổi về mainstoreID
                    {
                        var listMapping = empInStoreApi.BaseService.GetActive(q => q.EmployeeId == currentEmployee.Id);
                        var numberStoreMapping = listMapping.Count();
                        // Check newmapping
                        var newMapping = empInStoreApi.BaseService.GetActive(q => q.EmployeeId == currentEmployee.Id && q.StoreId == mainStoreId).FirstOrDefault();
                        if (newMapping != null)
                        {
                            if (!newMapping.Active)
                            {
                                newMapping.Active = true;
                                empInStoreApi.BaseService.Update(newMapping);
                            }
                        }
                        else
                        {
                            newMapping = new EmployeeInStore();
                            newMapping.AssignedDate = Utils.GetCurrentDateTime();
                            newMapping.EmployeeId = currentEmployee.Id;
                            newMapping.Status = 1;
                            newMapping.StoreId = mainStoreId;
                            newMapping.Active = true;
                            empInStoreApi.BaseService.Create(newMapping);
                        }

                        if (numberStoreMapping <= 1) // Nhân viên được tạo lần đầu chưa apply multi store
                        {
                            var currentMapping = empInStoreApi.BaseService.GetActive(q => q.EmployeeId == currentEmployee.Id && q.StoreId == currentEmployee.MainStoreId).FirstOrDefault();
                            if (currentMapping != null)
                            {
                                currentMapping.Active = false;
                                empInStoreApi.BaseService.Update(newMapping);
                            }
                            else
                            {
                                var mappings = empInStoreApi.BaseService.GetActive(q => q.EmployeeId == currentEmployee.Id).ToList();
                                if (mappings.Count() > 0)
                                {
                                    foreach (var item in mappings)
                                    {
                                        item.Active = false;
                                        empInStoreApi.BaseService.Update(item);
                                    }
                                    var tmpMapping = new EmployeeInStore();
                                    tmpMapping.AssignedDate = Utils.GetCurrentDateTime();
                                    tmpMapping.EmployeeId = currentEmployee.Id;
                                    tmpMapping.Status = 1;
                                    tmpMapping.StoreId = mainStoreId;
                                    tmpMapping.Active = true;
                                    empInStoreApi.BaseService.Create(newMapping);
                                }
                            }
                        }

                    }
                    // Update Employee
                    currentEmployee.Name = name;
                    currentEmployee.Address = addr != "" ? addr : null;
                    currentEmployee.Phone = phone != "" ? phone : null;
                    currentEmployee.Salary = decimal.Parse(salary != "" ? salary : "0");
                    currentEmployee.DateStartWork = dateworkstart;
                    currentEmployee.EmpEnrollNumber = fingerPrint;
                    currentEmployee.MainStoreId = mainStoreId;
                    currentEmployee.EmployeeGroupId = empGroupId;
                    currentEmployee.EmployeeCode = empCode;
                    currentEmployee.EmployeeRegency = empRecgency;
                    currentEmployee.DateOfBirth = dateOfBirth;
                    currentEmployee.PersonalCardId = personalCardId;
                    currentEmployee.DatePersonalCard = parse_datePersonalCard;
                    currentEmployee.PlaceOfPersonalCard = placeOfPersonalCard;
                    currentEmployee.MainAddress = mainAddr;
                    currentEmployee.EmployeePlaceBorn = placeBorn;
                    currentEmployee.EmployeeHometown = hometown;
                    currentEmployee.Email = email;
                    currentEmployee.EmployeeSex = sex;
                    currentEmployee.BrandId = brandId;

                    employeeApi.BaseService.Update(currentEmployee);
                    return Json(new { success = true, message = "Thay đổi thông tin thành công" }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Truy cập không hợp lệ" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng liên hệ Admin để được giải quyết" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteEmployee(int employeeId)
        {
            var result = true;
            var empApi = new EmployeeApi();
            var attApi = new AttendanceApi();
            var empInStoreApi = new EmployeeInStoreApi();
            var emp = empApi.Get(employeeId);

            if (emp == null)
            {
                result = false;

            }
            try
            {

                var empInStore = empInStoreApi.GetActive().Where(q => q.EmployeeId == emp.Id);
                foreach (var item in empInStore)
                {
                    item.Active = false;
                    empInStoreApi.Edit(item.Id, item);
                }
                //var attList = attApi.GetActive().Where(q => q.EmployeeId == emp.Id);
                //foreach (var attItem in attList)
                //{
                //    attItem.Active = false;
                //    attApi.Edit(attItem.Id, attItem);
                //}

                emp.Active = false;
                empApi.Edit(employeeId, emp);
            }
            catch (Exception ex)
            {
                result = false;
            }
            return Json(new { rs = result }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ReturnEmployee(string enrollId, int storeId)
        {
            var result = true;
            var empApi = new EmployeeApi();
            var attApi = new AttendanceApi();
            var empInStoreApi = new EmployeeInStoreApi();
            var emp = empApi.GetEmployeesByEnroll(enrollId);

            if (emp == null)
            {
                result = false;

            }
            try
            {

                var empInStore = empInStoreApi.GetActive().Where(q => q.EmployeeId == emp.Id && q.StoreId == storeId);
                foreach (var item in empInStore)
                {
                    item.Active = false;
                    empInStoreApi.Edit(item.Id, item);
                }
                //var attList = attApi.GetActive().Where(q => q.EmployeeId == emp.Id);
                //foreach (var attItem in attList)
                //{
                //    attItem.Active = false;
                //    attApi.Edit(attItem.Id, attItem);
                //}
            }
            catch (Exception ex)
            {
                result = false;
            }
            return Json(new { rs = result }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult UnDeleteEmployee(int employeeId)
        {
            var result = true;
            var empApi = new EmployeeApi();
            var attApi = new AttendanceApi();
            var empInStoreApi = new EmployeeInStoreApi();
            var emp = empApi.Get(employeeId);

            if (emp == null)
            {
                result = false;

            }
            try
            {

                var empInStore = empInStoreApi.GetActive().Where(q => q.EmployeeId == emp.Id);
                foreach (var item in empInStore)
                {
                    item.Active = true;
                    empInStoreApi.Edit(item.Id, item);
                }
                //var attList = attApi.GetActive().Where(q => q.EmployeeId == emp.Id);
                //foreach (var attItem in attList)
                //{
                //    attItem.Active = false;
                //    attApi.Edit(attItem.Id, attItem);
                //}

                emp.Active = true;
                empApi.Edit(employeeId, emp);
            }
            catch (Exception ex)
            {
                result = false;
            }
            return Json(new { rs = result }, JsonRequestBehavior.AllowGet);
        }
        /// <summary>
        /// Lấy danh sách điểm danh trong quá khứ
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public ActionResult LoadSessionAttendance(JQueryDataTableParamModel param, int employeeId, string startDate, string endDate, int status, int storeId)
        {
            var attendanceApi = new AttendanceApi();
            var timeFrameApi = new TimeFrameApi();

            var startTime = startDate.ToDateTime().GetStartOfDate();

            var endTime = endDate.ToDateTime().GetEndOfDate();

            IEnumerable<Attendance> listResult = attendanceApi.GetAttendanceByEmpIdAndStoreByTimeRange(employeeId, storeId, startTime, endTime);
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
                    a.ShiftMin.ToString("dd/MM/yyyy",CultureInfo.InvariantCulture),
                    a.ShiftMin.ToString(@"HH\:mm",CultureInfo.InvariantCulture),
                    a.ShiftMax.ToString(@"HH\:mm", CultureInfo.InvariantCulture),
                    a.TotalWorkTime == null  ? "N/A" : DateTime.Today.Add(a.TotalWorkTime.Value).ToString(@"HH\:mm", CultureInfo.InvariantCulture),
                    (a.CheckMin == null && a.CheckMax == null) ? (int)StatusAttendance.Miss :
                    (a.CheckMax < (a.ShiftMax.Add(-a.ExpandTime)) && a.CheckMin > (a.ShiftMin.Add(a.ExpandTime))) ? (int)StatusAttendance.Bothviolate :
                    (a.CheckMax < (a.ShiftMax.Add(-a.ExpandTime))) ? (int)StatusAttendance.ReturnEarly :
                    (a.CheckMin > (a.ShiftMin.Add(a.ExpandTime))) ? (int)StatusAttendance.ComeLate: (int)StatusAttendance.OnTime,
                    (a.ShiftMin >= startInDate && a.ShiftMin <= endInDate) ? (int)CurrentStatusEnum.Current : (a.ShiftMin < startInDate && a.ShiftMin <endInDate) ? (int)CurrentStatusEnum.Past : (int)CurrentStatusEnum.Future,
                });
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalResult,
                iTotalDisplayRecords = totalDisplay,
                aaData = result
            }, JsonRequestBehavior.AllowGet);
        }

        private SkyFiInfo ReadSkyFiInfo(string path)
        {
            using (StreamReader r = new StreamReader(path))
            {
                try
                {
                    string json = r.ReadToEnd();
                    var skyfi = JsonConvert.DeserializeObject<SkyFiInfo>(json);
                    return skyfi;
                }
                catch (Exception)
                {
                    return new SkyFiInfo();
                }
            }
        }


        /// <summary>
        /// DanhSachNhanVien.skyfi
        /// </summary>
        public static bool WriteSkyFiInfo(SkyFiInfo skyfi, string path)
        {
            try
            {
                var json = JsonConvert.SerializeObject(skyfi, Newtonsoft.Json.Formatting.Indented);
                //File.WriteAllText(path, json);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public ActionResult ShowMapToAcount(int employeeId, int brandId)
        {
            var aspNetUserApi = new AspNetUserApi();
            int count = 1;

            var listAccountEmp =
                aspNetUserApi.BaseService.Get(q => q.BrandId == brandId)
                    .Where(q => q.AspNetRoles.FirstOrDefault(a => a.Id == "13") != null).ToList();
            var rsList = listAccountEmp.Select(q => new IConvertible[]
            {
                count++,
                q.UserName,
                q.Email,
                q.Id
            });
            var checkList = listAccountEmp.Where(q => q.EmployeeId == employeeId).Select(q => new IConvertible[]
            {
                q.Id
            }); ;

            return Json(new { rs = rsList, rsCheckList = checkList }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult MapAcount(int empId, string userId, int brandId)
        {
            bool rs = false;
            var aspNetUserApi = new AspNetUserApi();
            var empApi = new EmployeeApi();
            
            try
            {
                var listAccountEmp = aspNetUserApi.BaseService.Get(q => q.BrandId == brandId && q.EmployeeId == empId).ToList();
                foreach (var item in listAccountEmp)
                {
                    var itemEdit = aspNetUserApi.Get(item.Id);
                    itemEdit.EmployeeId = null;
                    itemEdit.FullName = null;
                    aspNetUserApi.Edit(item.Id, itemEdit);
                }
                var user = aspNetUserApi.Get(userId);
                user.EmployeeId = empId;
                user.FullName = empApi.Get(empId).Name;
                aspNetUserApi.Edit(user.Id, user);

                rs = true;

            }
            catch (Exception)
            {
                rs = false;
            }
            return Json(new { rs = rs }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult prepareUpdateStoreOfEmp(int employeeId)
        {
            var empApi = new EmployeeApi();
            var empInStoreApi = new EmployeeInStoreApi();
            var model = empApi.Get(employeeId);
            var tmpModel = new
            {
                Id = model.Id,
                Name = model.Name,
                MainStoreId = model.MainStoreId
            };

            var listEmpInStore = empInStoreApi.GetActive().Where(q => q.EmployeeId == employeeId);

            return Json(new { rs = tmpModel, rsList = listEmpInStore }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult UpdateStoreOfEmp(int employeeId, List<int> listStoreId)
        {
            bool result = false;
            var empApi = new EmployeeApi();
            var emp = empApi.Get(employeeId);
            var empInStoreApi = new EmployeeInStoreApi();
            var listEmpInStore = empInStoreApi.GetActive().Where(q => q.EmployeeId == employeeId).ToList();
            List<int> storeIdTmp = new List<int>();
            List<int> storeIDOutDatabase = new List<int>();// list storeID delete
            List<int> storeIDOut = new List<int>();//list storeID insert

            try
            {
                foreach (var item in listStoreId)
                {
                    foreach (var item2 in listEmpInStore)
                    {
                        if (item2.StoreId.Equals(item))
                        {
                            storeIdTmp.Add(item);
                        }
                    }
                }
                foreach (var item in listStoreId)
                {
                    bool flag = false;
                    foreach (var item2 in storeIdTmp)
                    {
                        if (item2.Equals(item))
                        {
                            flag = true;
                        }
                    }
                    if (flag == false)
                    {
                        storeIDOut.Add(item);
                    }
                }
                foreach (var item in listEmpInStore)
                {
                    bool flag = false;
                    foreach (var item2 in storeIdTmp)
                    {
                        if (item2.Equals(item.StoreId))
                        {
                            flag = true;
                        }
                    }
                    if (flag == false)
                    {
                        storeIDOutDatabase.Add(item.StoreId);
                    }
                }

                foreach (var itemDelete in storeIDOutDatabase)
                {
                    var delete = listEmpInStore.Where(q => q.StoreId == itemDelete).First();
                    delete.Active = false;
                    empInStoreApi.Edit(delete.Id, delete);
                }

                foreach (var insert in storeIDOut)
                {
                    var model = new EmployeeInStoreViewModel()
                    {
                        EmployeeId = employeeId,
                        StoreId = insert,
                        Status = 1,
                        Active = true,
                        AssignedDate = DateTime.Now
                    };
                    empInStoreApi.Create(model);

                }
                result = true;
            }
            catch (Exception e)
            {
                result = false;
            }
            return Json(new { success = result }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult AttendanceInfo(int employeeId)
        {
            ViewBag.empId = employeeId;
            var employeeApi = new EmployeeApi();
            string empName = employeeApi.GetEmployee(employeeId).Name;
            ViewBag.empName = empName;
            return View("AttendanceInfo");
        }
        public ActionResult GetAllCheckFinger(int storeId, String startTime, String endTime, int empId)
        {
            var startDate = startTime.ToDateTime();
            var endDate = endTime.ToDateTime();
            startDate = startDate.GetStartOfDate();
            endDate = endDate.GetEndOfDate();
            var checkFingerApi = new CheckFingerApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var count = 1;


            var checkFinger2 = checkFingerApi.GetActive().ToList().OrderByDescending(q => q.DateTime)
                .Where(a => a.StoreId == storeId && a.DateTime >= startDate && a.DateTime <= endDate && a.EmployeeId == empId)
                .Select(a => new IConvertible[]{
                        count++,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                        fingerMachineApi.Get(a.FingerScanMachineId).Name,
                }).ToList();

            return Json(checkFinger2);

        }

        #region Export Excel Orginal
        public ActionResult ExportToExcelReportFinger(int employeeId, string startDate, string endDate, int storeId)
        {
            
            var empApi = new EmployeeApi();
            var employeeName = empApi.Get(employeeId).Name;
            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();
            var checkFingerApi = new CheckFingerApi();
            var fingerMachineApi = new FingerScanMachineApi();
            var count = 1;

            var result = checkFingerApi.GetActive().ToList().OrderByDescending(q => q.DateTime)
                .Where(a => a.StoreId == storeId && a.DateTime >= startTime && a.DateTime <= endTime && a.EmployeeId == employeeId)
                .Select(a => new IConvertible[]{
                        count++,
                        a.DateTime.ToString("dd/MM/yyyy HH:mm"),
                        fingerMachineApi.Get(a.FingerScanMachineId).Name,
                }).ToList();

            return ExportReportToExcelFinger(result, employeeName);

        }

        public ActionResult ExportReportToExcelFinger(List<IConvertible[]> data, string empName)
        {
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo");
                #region header
                ws.Cells["A1:A2"].Merge = true;
                ws.Cells["A1:A2"].Value = "STT";
                ws.Cells["A1:A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1:A2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["A1:A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["A1:A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["B1:B2"].Merge = true;
                ws.Cells["B1:B2"].Value = "Ngày giờ check";
                ws.Cells["B1:B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["B1:B2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["B1:B2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["C1:C2"].Merge = true;
                ws.Cells["C1:C2"].Value = "Máy scan";
                ws.Cells["C1:C2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["C1:C2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["C1:C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["C1:C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                
                #endregion
                //Set style for excel

                #region set value for cells

                int indexRow = 3;
                foreach (var item in data)
                {
                    int indexCol = 1;
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[0];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[1];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[2];    
                    indexRow = indexRow + 1;
                }
                #endregion
                #region style
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.WrapText = true;
                string fileName = "LichSuDiemDanh_" + empName + ".xlsx";
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }
        #endregion

       
        public ActionResult LoadCalendarAttendance(int empId, int storeId, String startTime, String endTime)
        {
            var listAttendance = new List<CalendarAttendance>();
            DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).GetStartOfDate();
            DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).GetEndOfDate();
            if (startTime.Length > 0)
            {
                startDate = startTime.ToDateTime().GetStartOfDate();
                endDate = endTime.ToDateTime().GetEndOfDate();
            }

            var apiAttendance = new AttendanceApi();
            var list = apiAttendance.GetActive().Where(p => p.EmployeeId == empId && p.StoreId == storeId && p.ShiftMin >= startDate && p.ShiftMin <= endDate).ToList();
            var datetimeNow = Utils.GetCurrentDateTime();
            foreach (var item in list)
            {
                var date = item.ShiftMin.Date.DayOfWeek.ToString();
                bool flag = true;
                if (listAttendance.Count() > 0)
                {
                    foreach (var item2 in listAttendance)
                    {

                        if (item2.date.Equals(date))
                        {
                            flag = false;
                        }
                    }
                }
                if (flag == true)
                {
                    var detail = new CalendarAttendance();
                    detail.date = item.ShiftMin.Date.DayOfWeek.ToString();
                    var listTimeslot = new List<TimeSlot>();
                    var timeSlot = new TimeSlot();

                    var duration = item.ShiftMax - item.ShiftMin;
                    double durationtofloat = Double.Parse(duration.TotalHours.ToString("F1"));
                    timeSlot.duration = durationtofloat;
                    double starHour = Double.Parse(item.ShiftMin.Hour.ToString("F1"));
                    timeSlot.startHour = starHour;
                    timeSlot.idAttendance = item.Id;
                    var totalWork = duration;
                    if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin < datetimeNow)
                    {
                        //absent
                        timeSlot.color = "#D9534F";
                    }
                    else if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin >= datetimeNow)
                    {
                        timeSlot.color = "#5BC0DE";
                        //N/A 
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax < (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre vs ve mau hong
                        //timeSlot.color = "#ec407a";
                        timeSlot.Late = "#26a69a";
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre ve dung gio mau xanh duong
                        timeSlot.Late = "#26a69a";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //ve som di dung gio mau tim
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else
                    {
                        //chuan mau xanh la cay
                        timeSlot.color = "#5CB85C";
                    }
                    timeSlot.status = date;
                    listTimeslot.Add(timeSlot);
                    var timeSlotAllDay = new TimeSlot();
                    timeSlotAllDay.color = "#7cb342";
                    timeSlotAllDay.startHour = 24;
                    timeSlotAllDay.duration = 1;
                    timeSlotAllDay.status = "Tổng giờ :" + duration.ToString(@"hh\:mm");
                    listTimeslot.Add(timeSlotAllDay);
                    detail.timeslot = listTimeslot;
                    detail.totalWork = duration;
                    listAttendance.Add(detail);
                }
                else
                {
                    var detail = listAttendance.Where(p => p.date == date).FirstOrDefault();
                    int index = listAttendance.FindLastIndex(p => p.date == date);
                    var timeSlot = new TimeSlot();
                    var duration = item.ShiftMax - item.ShiftMin;
                    double durationtofloat = Double.Parse(duration.TotalHours.ToString("F1"));
                    timeSlot.duration = durationtofloat;
                    var timesub = item.ShiftMin.GetStartOfDate();
                    double starHour = Math.Round(item.ShiftMin.Subtract(timesub).TotalHours, 1);
                    timeSlot.startHour = starHour;
                    timeSlot.idAttendance = item.Id;
                    if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin < datetimeNow)
                    {
                        //absent
                        timeSlot.color = "#D9534F";
                    }
                    else if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin >= datetimeNow)
                    {
                        timeSlot.color = "#5BC0DE";
                        //N/A
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax < (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre vs ve mau hong
                        //timeSlot.color = "#ec407a";
                        timeSlot.Late = "#26a69a";
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre ve dung gio mau xanh duong
                        timeSlot.Late = "#26a69a";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //ve som di dung gio mau tim
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else
                    {
                        //chuan mau xanh la cay
                        timeSlot.color = "#5CB85C";
                    }
                    timeSlot.status = date;
                    var totalwork = duration + detail.totalWork;
                    int a = detail.timeslot.Count() - 1;
                    detail.timeslot.RemoveAt(1);//xóa total work cũ
                    detail.timeslot.Add(timeSlot);
                    //thêm total work mới
                    var timeSlotAllDay = new TimeSlot();
                    timeSlotAllDay.color = "#7cb342";
                    timeSlotAllDay.startHour = 24;
                    timeSlotAllDay.duration = 1;
                    timeSlotAllDay.status = "Tổng giờ :" + totalwork.ToString(@"hh\:mm");
                    detail.timeslot.Add(timeSlotAllDay);
                    listAttendance.Add(detail);
                    listAttendance.RemoveAt(index);
                }
            }

            return Json(new { rs = listAttendance }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult detailsAttendance(int Id)
        {
            var attendanceApi = new AttendanceApi();
            var result = attendanceApi.Get(Id);
            var empApi = new EmployeeApi();
            var datetimeNow = Utils.GetCurrentDateTime();
            string status = "";
            if (result.CheckMin == null && result.CheckMax == null && result.ShiftMin < datetimeNow)
            {
                //absent mau den
                status = "Vắng";
            }
            else if (result.CheckMin == null && result.CheckMax == null && result.ShiftMin >= datetimeNow)
            {
                status = "Tương lai";
                //N/A mau do
            }
            else if ((result.CheckMin > (result.ShiftMin.Add(result.ExpandTime))) &&
                (result.CheckMax < (result.ShiftMax.Add(-result.ExpandTime))))
            {
                //di tre vs ve mau hong
                status = "Đi trễ về sớm";
            }
            else if ((result.CheckMin > (result.ShiftMin.Add(result.ExpandTime))) &&
                (result.CheckMax >= (result.ShiftMax.Add(-result.ExpandTime))))
            {
                //di tre ve dung gio mau xanh duong
                status = "Đi trễ";
            }
            else if ((result.CheckMin > (result.ShiftMin.Add(result.ExpandTime))) &&
                (result.CheckMax >= (result.ShiftMax.Add(-result.ExpandTime))))
            {
                //ve som di dung gio mau tim
                status = "Về sớm";
            }
            else
            {
                //chuan mau xanh la cay
                status = "Đúng giờ";
            }


            var tmpModel = new
            {
                Name = empApi.Get(result.EmployeeId).Name,
                StartHour = result.ShiftMin.ToString(),
                EndHour = result.ShiftMax.ToString(),
                Status = status,
                StartTime = result.CheckMin == null ? "N/A" : result.CheckMin.ToString(),
                EndTime = result.CheckMax == null ? "N/A" : result.CheckMax.ToString(),
            };
            return Json(tmpModel);
        }

        public ActionResult ExportToExcelReport(int employeeId, string startDate, string endDate, int status, int storeId)
        {
            var attendanceApi = new AttendanceApi();
            var empApi = new EmployeeApi();
            var employeeName = empApi.Get(employeeId).Name;
            var startTime = startDate.ToDateTime().GetStartOfDate();
            var endTime = endDate.ToDateTime().GetEndOfDate();

            IEnumerable<Attendance> listResult = attendanceApi.GetAttendanceByEmpIdAndStoreByTimeRange(employeeId, storeId, startTime, endTime);
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
            var count = 1;
            var currentDate = Utils.GetCurrentDateTime();
            var startInDate = currentDate.GetEndOfDate();
            var endInDate = currentDate.GetEndOfDate();
            var result = listResult.OrderBy(q => q.ShiftMax).Select(a => new IConvertible[]
                {
                    count++,
                    a.ShiftMin.ToString("dd/MM/yyyy",CultureInfo.InvariantCulture),
                    a.ShiftMin.ToString(@"HH\:mm",CultureInfo.InvariantCulture),
                    a.ShiftMax.ToString(@"HH\:mm", CultureInfo.InvariantCulture),
                    a.TotalWorkTime == null  ? "N/A" : DateTime.Today.Add(a.TotalWorkTime.Value).ToString(@"HH\:mm", CultureInfo.InvariantCulture),
                    (a.CheckMin == null && a.CheckMax == null) ? (int)StatusAttendance.Miss : (a.CheckMax < (a.ShiftMax.Add(-a.ExpandTime)) && a.CheckMin > (a.ShiftMin.Add(a.ExpandTime))) ? (int)StatusAttendance.Bothviolate :
                    (a.CheckMax < (a.ShiftMax.Add(-a.ExpandTime))) ? (int)StatusAttendance.ReturnEarly : (a.CheckMin > (a.ShiftMin.Add(a.ExpandTime))) ? (int)StatusAttendance.ComeLate: (int)StatusAttendance.OnTime,
                    (a.ShiftMin >= startInDate && a.ShiftMin <= endInDate) ? (int)CurrentStatusEnum.Current : (a.ShiftMin < startInDate && a.ShiftMin <endInDate) ? (int)CurrentStatusEnum.Past : (int)CurrentStatusEnum.Future,
                    "Giờ vào: " + (a.CheckMin == null ? "chưa có" : a.CheckMin.Value.ToString("HH:mm")) + " - " + "Giờ ra: " + (a.CheckMax == null ? "chưa có" : a.CheckMax.Value.ToString("HH:mm")),

                }).ToList();

            return ExportReportToExcel(result, employeeName);

        }
        public ActionResult ExportReportToExcel(List<IConvertible[]> data, string empName)
        {
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo");
                #region header
                ws.Cells["A1:A2"].Merge = true;
                ws.Cells["A1:A2"].Value = "STT";
                ws.Cells["A1:A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1:A2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["A1:A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["A1:A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["B1:B2"].Merge = true;
                ws.Cells["B1:B2"].Value = "Ngày làm việc";
                ws.Cells["B1:B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["B1:B2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["B1:B2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["C1:C2"].Merge = true;
                ws.Cells["C1:C2"].Value = "Giờ bắt đầu";
                ws.Cells["C1:C2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["C1:C2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["C1:C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["C1:C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["D1:D2"].Merge = true;
                ws.Cells["D1:D2"].Value = "Giờ kết thúc";
                ws.Cells["D1:D2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["D1:D2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["D1:D2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["D1:D2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["E1:E2"].Merge = true;
                ws.Cells["E1:E2"].Value = "Tổng thời gian";
                ws.Cells["E1:E2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["E1:E2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["E1:E2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["E1:E2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["F1:F2"].Merge = true;
                ws.Cells["F1:F2"].Value = "Tình trạng";
                ws.Cells["F1:F2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["F1:F2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["F1:F2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["F1:F2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["G1:G2"].Merge = true;
                ws.Cells["G1:G2"].Value = "Thời gian";
                ws.Cells["G1:G2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["G1:G2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["G1:G2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["G1:G2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["H1:H2"].Merge = true;
                ws.Cells["H1:H2"].Value = "Chi tiết giờ vào - ra";
                ws.Cells["H1:H2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["H1:H2"].Style.Fill.BackgroundColor.SetColor(Color.ForestGreen);
                ws.Cells["H1:H2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["H1:H2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                #endregion
                //Set style for excel

                #region set value for cells

                int indexRow = 3;
                foreach (var item in data)
                {
                    int indexCol = 1;
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[0];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[1];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[2];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[3];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[4];
                    var checkStt = (int)item[5];
                    if (checkStt == (int)StatusAttendance.Miss)
                    {
                        ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = StatusAttendance.Miss.DisplayName();
                    }
                    else if (checkStt == (int)StatusAttendance.Bothviolate)
                    {
                        ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = StatusAttendance.Bothviolate.DisplayName();
                    }
                    else if (checkStt == (int)StatusAttendance.ComeLate)
                    {
                        ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = StatusAttendance.ComeLate.DisplayName();
                    }
                    else if (checkStt == (int)StatusAttendance.OnTime)
                    {
                        ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = StatusAttendance.OnTime.DisplayName();
                    }
                    else
                    {
                        ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = StatusAttendance.ReturnEarly.DisplayName();
                    }

                    var checkTime = (int)item[6];
                    if (checkTime == (int)CurrentStatusEnum.Current)
                    {
                        ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = CurrentStatusEnum.Current.DisplayName();
                    }
                    else if (checkTime == (int)CurrentStatusEnum.Future)
                    {
                        ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = CurrentStatusEnum.Future.DisplayName();
                    }
                    else
                    {
                        ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = CurrentStatusEnum.Past.DisplayName();
                    }

                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[7];
                    indexRow = indexRow + 1;
                }
                #endregion
                #region style
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[ws.Dimension.Address].Style.WrapText = true;
                string fileName = "ChiTietNhanVien_" + empName + ".xlsx";
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }
        public string GetColNameFromIndex(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }

        public ActionResult ExportToExcelReportEmpInStore(string storeSelected, int brandId)
        {
            int count = 1;
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();
            string storeName = "Toàn Hệ Thống";
            var storeId = 0;
            try
            {
                storeId = int.Parse(storeSelected);
                storeName = storeApi.Get(storeId).ShortName;
                if (storeId == 0)
                {
                    storeName = "Toàn Hệ Thống";
                }

            }
            catch (Exception) // Skip if Parse Fail
            {
            }
            var datatable = employeeApi.GetEmployeeByStoreId(storeId, brandId).Select(q => new IConvertible[]
            {
                    count++,
                    q.Name,
                    q.EmpEnrollNumber,
                    q.Phone == null ? "N/A" : q.Phone,
                    q.DateStartWork == null ? "N/A": q.DateStartWork.Value.ToString("dd/MM/yyyy"),
                    q.Salary,
            }).ToList();
            return ExportReportToExcelEmpInSore(datatable, storeName, "delete");
        }

        public ActionResult ExportToExcelReportEmpNonWorkingInStore(string storeSelected, int brandId)
        {
            int count = 1;
            var employeeApi = new EmployeeApi();
            var storeApi = new StoreApi();
            string storeName = "Toàn Hệ Thống";
            var storeId = 0;
            try
            {
                storeId = int.Parse(storeSelected);
                storeName = storeApi.Get(storeId).ShortName;
                if (storeId == 0)
                {
                    storeName = "Toàn Hệ Thống";
                }

            }
            catch (Exception) // Skip if Parse Fail
            {
            }
            var datatable = employeeApi.GetEmployeeNonWorkingByStoreId(storeId, brandId).Select(q => new IConvertible[]
            {
                count++,
                q.Name,
                q.EmpEnrollNumber,
                q.Phone == null ? "N/A" : q.Phone,
                q.DateStartWork == null ? "N/A": q.DateStartWork.Value.ToString("dd/MM/yyyy"),
                q.Salary,
            }).ToList();
            return ExportReportToExcelEmpInSore(datatable, storeName, "undelete");
        }
        public ActionResult ExportReportToExcelEmpInSore(List<IConvertible[]> data, string storeName, string action)
        {
            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Báo cáo");
                #region header
                ws.Cells["A1:A2"].Merge = true;
                ws.Cells["A1:A2"].Value = "STT";
                ws.Cells["A1:A2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1:A2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
                ws.Cells["A1:A2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["A1:A2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["B1:B2"].Merge = true;
                ws.Cells["B1:B2"].Value = "Tên nhân viên";
                ws.Cells["B1:B2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["B1:B2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
                ws.Cells["B1:B2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["B1:B2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["C1:C2"].Merge = true;
                ws.Cells["C1:C2"].Value = "Mã chấm công";
                ws.Cells["C1:C2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["C1:C2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
                ws.Cells["C1:C2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["C1:C2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["D1:D2"].Merge = true;
                ws.Cells["D1:D2"].Value = "Số điện thoại";
                ws.Cells["D1:D2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["D1:D2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
                ws.Cells["D1:D2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["D1:D2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["E1:E2"].Merge = true;
                ws.Cells["E1:E2"].Value = "Ngày bắt đầu";
                ws.Cells["E1:E2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["E1:E2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
                ws.Cells["E1:E2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["E1:E2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["F1:F2"].Merge = true;
                ws.Cells["F1:F2"].Value = "Tiền lương";
                ws.Cells["F1:F2"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["F1:F2"].Style.Fill.BackgroundColor.SetColor(Color.AntiqueWhite);
                ws.Cells["F1:F2"].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                ws.Cells["F1:F2"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;


                #endregion
                //Set style for excel

                #region set value for cells

                int indexRow = 3;
                foreach (var item in data)
                {
                    int indexCol = 1;
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[0];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[1];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[2];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[3];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[4];
                    ws.Cells["" + GetColNameFromIndex(indexCol++) + indexRow].Value = item[5];
                    indexRow = indexRow + 1;
                }
                #endregion
                #region style
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                ws.Cells[ws.Dimension.Address].Style.WrapText = true;
                string fileName = "";
                if (action.Equals("delete"))
                {
                    fileName = "Nhân viên của hàng _" + storeName + ".xlsx";
                }
                else
                {
                    fileName = "Nhân viên đã nghỉ việc của hàng _" + storeName + ".xlsx";
                }
                #endregion
                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                return this.File(ms, contentType, fileName);
            }
            #endregion
        }

        public ActionResult LoadCalendarAttendance2(int empId, int storeId, String startTime, String endTime)
        {
            var listAttendance = new List<CalendarAttendance>();
            DateTime startDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday).GetStartOfDate();
            DateTime endDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Saturday).GetEndOfDate();
            if (startTime.Length > 0)
            {
                startDate = startTime.ToDateTime().GetStartOfDate();
                endDate = endTime.ToDateTime().GetEndOfDate();
            }

            var apiAttendance = new AttendanceApi();
            var list = apiAttendance.GetActive().Where(p => p.EmployeeId == empId && p.StoreId == storeId && p.ShiftMin >= startDate && p.ShiftMin <= endDate).ToList();
            var datetimeNow = Utils.GetCurrentDateTime();
            foreach (var item in list)
            {
                var date = item.ShiftMin.Date.DayOfWeek.ToString();
                bool flag = true;
                if (listAttendance.Count() > 0)
                {
                    foreach (var item2 in listAttendance)
                    {

                        if (item2.date.Equals(date))
                        {
                            flag = false;
                        }
                    }
                }
                bool flagEx = false; //kiem tra mot ca co thuoc nhieu ngay hay khong
                double durationtodouble = 0;
                if (flag == true)
                {
                    var detail = new CalendarAttendance();
                    detail.date = date;
                    var listTimeslot = new List<TimeSlot>();
                    var timeSlot = new TimeSlot();

                    var duration = item.ShiftMax - item.ShiftMin;
                    var timesub = item.ShiftMin.GetStartOfDate();
                    double starHour = Math.Round(item.ShiftMin.Subtract(timesub).TotalHours, 1);
                    timeSlot.startHour = starHour;
                    double durationtofloat = Double.Parse(duration.TotalHours.ToString("F1"));
                    durationtodouble = (starHour + durationtofloat) - 24;
                    if (durationtodouble > 0)
                    {
                        timeSlot.duration = durationtofloat - durationtodouble;
                        flagEx = true;
                    }
                    else
                    {
                        timeSlot.duration = durationtofloat;
                    }
                    timeSlot.idAttendance = item.Id;
                    var totalWork = TimeSpan.FromHours(timeSlot.duration);
                    if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin < datetimeNow)
                    {
                        //absent
                        timeSlot.color = "#D9534F";
                    }
                    else if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin >= datetimeNow)
                    {
                        timeSlot.color = "#5BC0DE";
                        //N/A 
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax < (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre vs ve mau hong
                        timeSlot.Late = "#26a69a";
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre ve dung gio mau xanh duong
                        timeSlot.Late = "#26a69a";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //ve som di dung gio mau tim
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else
                    {
                        //chuan mau xanh la cay
                        timeSlot.color = "#5CB85C";
                    }
                    timeSlot.status = date;
                    listTimeslot.Add(timeSlot);
                    var timeSlotAllDay = new TimeSlot();
                    timeSlotAllDay.color = "#7cb342";
                    timeSlotAllDay.startHour = 24;
                    timeSlotAllDay.duration = 1;
                    timeSlotAllDay.status = "Tổng giờ :" + totalWork.ToString(@"hh\:mm");
                    listTimeslot.Add(timeSlotAllDay);
                    detail.timeslot = listTimeslot;
                    detail.totalWork = totalWork;
                    listAttendance.Add(detail);
                }
                else
                {
                    var detail = listAttendance.Where(p => p.date == date).FirstOrDefault();
                    int index = listAttendance.FindLastIndex(p => p.date == date);
                    var timeSlot = new TimeSlot();
                    var duration = item.ShiftMax - item.ShiftMin;
                    var timesub = item.ShiftMin.GetStartOfDate();
                    double starHour = Math.Round(item.ShiftMin.Subtract(timesub).TotalHours, 1);
                    timeSlot.startHour = starHour;
                    double durationtofloat = Double.Parse(duration.TotalHours.ToString("F1"));
                    durationtodouble = (starHour + durationtofloat) - 24;
                    if (durationtodouble > 0)
                    {
                        timeSlot.duration = durationtofloat - durationtodouble;
                        flagEx = true;
                    }
                    else
                    {
                        timeSlot.duration = durationtofloat;
                    }
                    timeSlot.idAttendance = item.Id;
                    if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin < datetimeNow)
                    {
                        //absent
                        timeSlot.color = "#D9534F";
                    }
                    else if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin >= datetimeNow)
                    {
                        timeSlot.color = "#5BC0DE";
                        //N/A
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax < (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre vs ve mau hong
                        //timeSlot.color = "#ec407a";
                        timeSlot.Late = "#26a69a";
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //di tre ve dung gio mau xanh duong
                        timeSlot.Late = "#26a69a";
                        timeSlot.color = "#F0AD4E";
                    }
                    else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                        (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                    {
                        //ve som di dung gio mau tim
                        timeSlot.LeaveEarly = "#7e57c2";
                        timeSlot.color = "#F0AD4E";
                    }
                    else
                    {
                        //chuan mau xanh la cay
                        timeSlot.color = "#5CB85C";
                    }
                    timeSlot.status = date;
                    var totalwork = TimeSpan.FromHours(timeSlot.duration) + detail.totalWork;
                    detail.totalWork = totalwork;
                    var totalallday = detail.timeslot.FindLastIndex(p => p.idAttendance == 0);
                    detail.timeslot.RemoveAt(totalallday);//xóa total work cũ
                    detail.timeslot.Add(timeSlot);
                    //thêm total work mới
                    var timeSlotAllDay = new TimeSlot();
                    timeSlotAllDay.color = "#7cb342";
                    timeSlotAllDay.startHour = 24;
                    timeSlotAllDay.duration = 1;
                    timeSlotAllDay.status = "Tổng giờ :" + totalwork.ToString(@"hh\:mm");
                    detail.timeslot.Add(timeSlotAllDay);
                    listAttendance.Add(detail);
                    listAttendance.RemoveAt(index);
                }
                if (flagEx == true)
                {
                    var dateEx = item.ShiftMin.Date.AddDays(1).DayOfWeek.ToString();
                    bool flag3 = true;
                    foreach (var item2 in listAttendance)
                    {
                        if (item2.date.Equals(dateEx))
                        {
                            flag3 = false;
                        }
                    }
                    if (flag3 == true)
                    {
                        var detail = new CalendarAttendance();
                        detail.date = dateEx;
                        var listTimeslot = new List<TimeSlot>();
                        var timeSlot = new TimeSlot();
                        timeSlot.startHour = 0;
                        timeSlot.duration = durationtodouble;
                        timeSlot.idAttendance = item.Id;
                        var totalWork = TimeSpan.FromHours(durationtodouble);
                        if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin < datetimeNow)
                        {
                            //absent
                            timeSlot.color = "#D9534F";
                        }
                        else if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin >= datetimeNow)
                        {
                            timeSlot.color = "#5BC0DE";
                            //N/A 
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                            (item.CheckMax < (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //di tre vs ve mau hong
                            timeSlot.Late = "#26a69a";
                            timeSlot.LeaveEarly = "#7e57c2";
                            timeSlot.color = "#F0AD4E";
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                            (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //di tre ve dung gio mau xanh duong
                            timeSlot.Late = "#26a69a";
                            timeSlot.color = "#F0AD4E";
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                            (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //ve som di dung gio mau tim
                            timeSlot.LeaveEarly = "#7e57c2";
                            timeSlot.color = "#F0AD4E";
                        }
                        else
                        {
                            //chuan mau xanh la cay
                            timeSlot.color = "#5CB85C";
                        }
                        timeSlot.status = dateEx;
                        listTimeslot.Add(timeSlot);
                        var timeSlotAllDay = new TimeSlot();
                        timeSlotAllDay.color = "#7cb342";
                        timeSlotAllDay.startHour = 24;
                        timeSlotAllDay.duration = 1;
                        timeSlotAllDay.status = "Tổng giờ :" + totalWork.ToString(@"hh\:mm");
                        timeSlotAllDay.idAttendance = 0;
                        listTimeslot.Add(timeSlotAllDay);
                        detail.timeslot = listTimeslot;
                        detail.totalWork = totalWork;
                        listAttendance.Add(detail);
                    }
                    else
                    {
                        var detail = listAttendance.Where(p => p.date == dateEx).FirstOrDefault();
                        int index = listAttendance.FindLastIndex(p => p.date == dateEx);
                        var timeSlot = new TimeSlot();
                        var duration = TimeSpan.FromHours(durationtodouble);
                        timeSlot.startHour = 0;
                        timeSlot.duration = durationtodouble;
                        timeSlot.idAttendance = item.Id;
                        if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin < datetimeNow)
                        {
                            //absent
                            timeSlot.color = "#D9534F";
                        }
                        else if (item.CheckMin == null && item.CheckMax == null && item.ShiftMin >= datetimeNow)
                        {
                            timeSlot.color = "#5BC0DE";
                            //N/A
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                            (item.CheckMax < (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //di tre vs ve mau hong
                            //timeSlot.color = "#ec407a";
                            timeSlot.Late = "#26a69a";
                            timeSlot.LeaveEarly = "#7e57c2";
                            timeSlot.color = "#F0AD4E";
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                            (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //di tre ve dung gio mau xanh duong
                            timeSlot.Late = "#26a69a";
                            timeSlot.color = "#F0AD4E";
                        }
                        else if ((item.CheckMin > (item.ShiftMin.Add(item.ExpandTime))) &&
                            (item.CheckMax >= (item.ShiftMax.Add(-item.ExpandTime))))
                        {
                            //ve som di dung gio mau tim
                            timeSlot.LeaveEarly = "#7e57c2";
                            timeSlot.color = "#F0AD4E";
                        }
                        else
                        {
                            //chuan mau xanh la cay
                            timeSlot.color = "#5CB85C";
                        }
                        timeSlot.status = dateEx;
                        var totalwork = TimeSpan.FromHours(timeSlot.duration) + detail.totalWork;
                        var totalallday = detail.timeslot.FindLastIndex(p => p.idAttendance == 0);
                        detail.totalWork = totalwork;
                        detail.timeslot.RemoveAt(totalallday);//xóa total work cũ
                        detail.timeslot.Add(timeSlot);
                        //thêm total work mới
                        var timeSlotAllDay = new TimeSlot();
                        timeSlotAllDay.color = "#7cb342";
                        timeSlotAllDay.startHour = 24;
                        timeSlotAllDay.duration = 1;
                        timeSlotAllDay.status = "Tổng giờ :" + totalwork.ToString(@"hh\:mm");
                        timeSlotAllDay.idAttendance = 0;
                        detail.timeslot.Add(timeSlotAllDay);
                        listAttendance.Add(detail);
                        listAttendance.RemoveAt(index);
                    }
                }
            }

            return Json(new { rs = listAttendance }, JsonRequestBehavior.AllowGet);
        }

    }

}