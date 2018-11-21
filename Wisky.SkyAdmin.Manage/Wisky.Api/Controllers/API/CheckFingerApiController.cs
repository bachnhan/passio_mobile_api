using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Sdk;
using HmsService.Models;
using HmsService.ViewModels;
using Microsoft.Ajax.Utilities;

namespace Wisky.Api.Controllers.API
{
    public class CheckFingerApiController : Controller
    {
        // GET: CheckFingerApi
        //public ActionResult Index()
        //{
        //    return View();
        //}

        public bool CheckFinger(string fingerCode,string machineCode)
        {
            var empApi=new EmployeeApi();
            var emp = empApi.GetActive().Where(q => q.EmpEnrollNumber == fingerCode).ToList();
            var fingerScanMachineApi = new FingerScanMachineApi();
            var fingerScanMachine = fingerScanMachineApi.GetActive().Where(q=>q.MachineCode==machineCode).ToList();
            var checkFingerApi=new CheckFingerApi();
            try
            {
                checkFingerApi.Create(new CheckFingerViewModel()
                {
                    EmployeeId = emp[0].Id,
                    Active = true,
                    DateTime = DateTime.Now,
                    FingerScanMachineId = fingerScanMachine[0].Id,
                    StoreId = fingerScanMachine[0].StoreId
                });
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
            
        }
    }
}