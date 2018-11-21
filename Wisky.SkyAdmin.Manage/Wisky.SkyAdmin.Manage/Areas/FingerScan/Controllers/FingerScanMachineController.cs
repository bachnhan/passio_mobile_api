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
    public class FingerScanMachineController : Controller
    {
        // GET: FingerScan/FingerScanMachine
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetFingerScanMachine(int storeId)
        {
            int count = 1;
            var api = new FingerScanMachineApi();
            var datatable = api.GetActive().Where(q => q.StoreId == storeId).ToList().Select(q => new IConvertible[] {
                count++,
                q.MachineCode,
                q.Name,
                q.Ip,
                q.BrandOfMachine,
                q.DateOfManufacture ==null ? "N/A": q.DateOfManufacture.Value.ToShortDateString(),
                q.Id
            });
            return Json(new { datatable }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CreateFingerScanMachine(int storeId, String code, String name, String ip, String brand, String dateOfMa)
        {
            int result = 0;
            var fingerScanMachineApi = new FingerScanMachineApi();
            var checkFingerApi = new CheckFingerApi();
            var checkDuplicate = fingerScanMachineApi.GetActive().Where(q => q.MachineCode == code).ToList();
            if (checkDuplicate.Count > 0)
            {
                result = 2;
                return Json(result);
            }
            var model = new FingerScanMachineViewModel()
            {
                StoreId = storeId,
                MachineCode = code,
                Name = name,
                Ip = ip,
                BrandOfMachine = brand,
                DateOfManufacture = Utils.ToDateTime(dateOfMa),
                Active = true,

            };
            try
            {
                fingerScanMachineApi.Create(model);
                var listCheckFinger = checkFingerApi.GetActive().Where(q => q.MachineNumber == model.MachineCode && q.FingerScanMachineId == null);
                foreach (var item in listCheckFinger)
                {
                    item.IsUpdated = false;
                    checkFingerApi.Edit(item.Id, item);
                }
                result = 1;
            }
            catch (Exception e)
            {
                result = 0;
            }
            return Json(result);
        }

        public ActionResult prepareEdit(int FingerScanMachineId)
        {
            var fingerScanMachineApi = new FingerScanMachineApi();
            var model = fingerScanMachineApi.Get(FingerScanMachineId);
            var tmpModel = new
            {
                machineId = model.Id,
                machineCode = model.MachineCode,
                machineName = model.Name,
                ip = model.Ip,
                brandOfMachine = model.BrandOfMachine,
                dateOfManufacture = model.DateOfManufacture == null ? "N/A" : model.DateOfManufacture.Value.ToString("dd/MM/yyyy")
            };
            return Json(new { rs = tmpModel }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult EditFingerScanMachine(int id, int storeId, String code,String name, String ip, String brand, string dateOfMaStr)
        {
            // Dong nay AnLH viet de controller nhan du lieu tu View --- Sua tren ban cua HaiNV
            var dateOfMa = Utils.ToDateTime(dateOfMaStr);

            int result = 0;
            var api = new FingerScanMachineApi();
            var checkFingerApi = new CheckFingerApi();
            var checkDuplicate = api.GetActive().Where(q => q.MachineCode == code && q.Id != id && q.Name == name && q.Ip == ip && q.BrandOfMachine == brand && q.DateOfManufacture == dateOfMa).ToList();
            if (checkDuplicate.Count > 0)
            {
                result = 2;
                return Json(result);
            }
            var model = new FingerScanMachineViewModel()
            {
                Id = id,
                StoreId = storeId,
                MachineCode = code,
                Name = name,
                Ip = ip,
                BrandOfMachine = brand,
                DateOfManufacture = dateOfMa,
                Active = true

            };
            try
            {
                api.Edit(id, model);
                var listCheckFinger = checkFingerApi.GetActive().Where(q => q.MachineNumber == model.MachineCode && q.FingerScanMachineId == null);
                foreach (var item in listCheckFinger)
                {
                    item.IsUpdated = false;
                    checkFingerApi.Edit(item.Id, item);
                }
                result = 1;
            }
            catch (Exception e)
            {
                result = 0;
            }
            return Json(result);
        }

        public ActionResult DeleteFingerScanMachine(int FingerScanMachineId)
        {
            var result = true;
            var api = new FingerScanMachineApi();
            var entity = api.Get(FingerScanMachineId);

            if (entity == null)
            {
                result = false;

            }
            try
            {
                entity.Active = false;
                api.Edit(FingerScanMachineId, entity);
            }
            catch (Exception ex)
            {
                result = false;
            }
            return Json(result);
        }
    }
}