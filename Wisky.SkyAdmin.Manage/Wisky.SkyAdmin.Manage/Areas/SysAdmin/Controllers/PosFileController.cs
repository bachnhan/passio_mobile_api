using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Controllers
{
    public class PosFileController : Controller
    {
        // GET: SysAdmin/PosFile
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetAllPosFile()
        {
            try
            {
                var posFileApi = new PosFileApi();
                var count = 1;
                var list = posFileApi.GetActive().Select(q => new IConvertible[] {
                    count++,
                    q.FileName,
                    q.Version,
                    q.Id
                }).ToList();

                if (list != null && count > 1)
                {
                    return Json(new { success = true, list = list }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = "Bảng không có dữ liệu" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Xin vui lòng liện hệ Admin." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetAllConfig(int posFileId)
        {
            try
            {
                var posConfigApi = new PosConfigApi();
                var count = 1;
                var list = posConfigApi.GetAllPosConfigByFileId(posFileId);

                if (list != null && count > 0)
                {
                    return Json(new { success = true, list = list }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { success = false, message = "Không có pos config" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra. Xin vui lòng liện hệ Admin." }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> ConfigPosFileViaLocalFile()
        {
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];
                var braceIndex = file.FileName.IndexOfAny(new char[] { '(','.'});
                var fileName = braceIndex >= 0 ? file.FileName.Substring(0, braceIndex).Trim() : file.FileName;
                using (var sr = new StreamReader(file.InputStream))
                using (var jsonTextReader = new JsonTextReader(sr))
                {
                    var posFileApi = new PosFileApi();
                    var storeApi = new StoreApi();
                    var jsonSerializer = new JsonSerializer();
                    var configKeyDict = jsonSerializer.Deserialize<Dictionary<string, string>>(jsonTextReader);
                    var stId = configKeyDict["StoreId"];
                    var version = configKeyDict["Version"];
                    var storeId = 0;
                    int.TryParse(stId, out storeId);
                    var posFile = posFileApi.GetActivePosFileByNameAndStore(fileName, storeId);
                    var posConfigList = new List<PosConfigViewModel>();
                    foreach (var keyValue in configKeyDict)
                    {
                        var posConfigsVM = new PosConfigViewModel
                        {
                            Key = keyValue.Key,
                            Value = keyValue.Value,
                        };
                        posConfigList.Add(posConfigsVM);
                    }
                    if (posFile == null) //ko co' pos file thi tao
                    {
                        var brandId = storeApi.Get(storeId).BrandId.Value;                       
                        var postFileVM = new PosFileViewModel()
                        {
                            FileName = fileName,
                            StoreId = storeId,
                            PosConfigs = posConfigList,
                            BrandId = brandId,
                            Version = version
                        };
                        await posFileApi.CreateAsync(postFileVM);
                        return Json(new { success = true, message = "Cập nhật thành công" }, JsonRequestBehavior.AllowGet);
                    }
                    else // co roi thi update
                    {
                        if(version.CompareTo(posFile.Version) > 0)//neu version dulieu tren DB cu hon thi update
                        {
                            posFile.Version = version;
                            posFile.PosConfigs = posConfigList;
                            if (posFileApi.UpdatePostFileWithAllConfigs(posFile))
                            {
                                return Json(new { success = true, message = "Cập nhật thành công" }, JsonRequestBehavior.AllowGet);
                            }
                        }
                        else
                        {
                            return Json(new { success = true, message = "Phiên bản file update thấp hơn phiên bản hiện tại" }, JsonRequestBehavior.AllowGet);
                        }
                        
                    }
                }
            }
            return Json(new { success = false, message = "Cập nhật fail" }, JsonRequestBehavior.AllowGet);

        }

        public ActionResult Edit(int posFileId)
        {
            var posFileApi = new PosFileApi();
            var posConfigApi = new PosConfigApi();
            var storeApi = new StoreApi();
            var brandApi = new BrandApi();
            //List<PosConfigViewModel> configList = posConfigApi.GetAllPosConfigByFileId(posFileId);
            var model = posFileApi.Get(posFileId);
            var store = new StoreViewModel(storeApi.GetStoreById(model.StoreId.Value));
            var brand = brandApi.GetBrandById(model.BrandId);
            ViewBag.currentStore = store;
            ViewBag.currentBrand = brand;
            ViewBag.StoreList = new List<StoreViewModel>();
            ViewBag.BrandList = new List<BrandViewModel>();
            //model.PosConfigs = configList == null ? new List<PosConfigViewModel>() : configList;
            return this.View(model);
        }

        public ActionResult Create()
        {
            var brandApi = new BrandApi();
            var brandList = brandApi.GetActive().Select(q => new { Id = q.Id, Name = q.BrandName });
            var model = new PosFileViewModel();
            ViewBag.currentStore = null;
            ViewBag.currentBrand = null;
            ViewBag.BrandList = brandList;
            return this.View(model);
        }

        public ActionResult CreatePosFile(PosFileEditViewModel model)
        {
            try
            {
                var posFileApi = new PosFileApi();
                var posConfigApi = new PosConfigApi();
                var createModel = new PosFileViewModel
                {
                    BrandId = model.BrandId,
                    StoreId = model.StoreId,
                    FileName = model.FileName,
                    Version = model.Version
                };

                var posFileId = posFileApi.CreateAndReturnId(createModel);

                if (posFileId != -1)
                {
                    var posConfigList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PosConfigEditViewModel>>(model.PosConfigList);
                    foreach (var config in posConfigList)
                    {
                        var configModel = new PosConfigViewModel
                        {
                            Key = config.Key,
                            Value = config.Value,
                            PosFileId = posFileId
                        };

                        posConfigApi.Create(configModel);
                    }

                    return Json(new { success = true, message = "Tạo POS file thành công." }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Tạo POS file không thành công." }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liên hệ admin." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditPosFile(int currentPosFileId, PosFileEditViewModel model)
        {
            try
            {
                var posFileApi = new PosFileApi();
                var posConfigApi = new PosConfigApi();
                var editModel = new PosFileViewModel
                {
                    Id = currentPosFileId,
                    BrandId = model.BrandId,
                    StoreId = model.StoreId,
                    FileName = model.FileName,
                    Version = model.Version
                };

                var isSuccessUpdate = posFileApi.UpdateAndReturnSuccess(editModel);

                if (isSuccessUpdate)
                {
                    var isSuccessDelete = posFileApi.DeleteAllConfigAndReturnSuccess(currentPosFileId);

                    if (isSuccessDelete)
                    {
                        var posConfigList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<PosConfigEditViewModel>>(model.PosConfigList);
                        foreach (var config in posConfigList)
                        {
                            var configModel = new PosConfigViewModel
                            {
                                Key = config.Key,
                                Value = config.Value,
                                PosFileId = currentPosFileId
                            };

                            posConfigApi.Create(configModel);
                        }

                        return Json(new { success = true, message = "Cập nhật POS file thành công." }, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(new { success = false, message = "Cập nhật POS file không thành công." }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liên hệ admin." }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult GetStoreList(int brandId)
        {
            try
            {
                var storeApi = new StoreApi();
                var storeList = storeApi.GetStoreByBrandId(brandId).ToList();
                if (storeList != null && storeList.Count > 0)
                {
                    return Json(new { success = true, StoreList = storeList }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, message = "Không có cửa hàng" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liên hệ admin." }, JsonRequestBehavior.AllowGet);
            }
        }

        //public ActionResult GetAllPosFile()
        //{
        //    try
        //    {
        //        var posFileApi = new PosFileApi();
        //        var fileList = posFileApi.GetActive();
        //        if (fileList != null && fileList.Count > 0)
        //        {
        //            return Json(new { success = true, FileList = fileList }, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json(new { success = false, message = "Không có POS file" }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return Json(new { success = false, message = "Có lỗi xảy ra, xin vui lòng liên hệ admin." }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public class PosFileEditViewModel
        {
            public string FileName { get; set; }
            public int StoreId { get; set; }
            public int BrandId { get; set; }
            //public PosConfigEditViewModel[] PosConfigList { get; set; }
            public string PosConfigList { get; set; }
            public string Version { get; set; }
        }

        public class PosConfigEditViewModel
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }
    }
}