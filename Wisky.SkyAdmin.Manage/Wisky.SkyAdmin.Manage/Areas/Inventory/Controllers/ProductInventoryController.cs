using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HmsService.Sdk;
using HmsService.Models;
using System.Threading.Tasks;
using HmsService.ViewModels;
using Newtonsoft.Json;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.Models.Entities;

namespace WiSky.SkyAdmin.Manage.Areas.Inventory.Controllers
{
    [Authorize(Roles = "BrandManager, Inventory, StoreManager, Manager")]
    public class ProductInventoryController : DomainBasedController
    {
        #region Nhập hàng
        public ActionResult ListImportInventory(string storeId)
        {
            var api = new InventoryReceiptApi();
            var inventoryReceipts = api.GetImportInventoryReceiptByStore(int.Parse(storeId));
            var model = new InventoryReceiptEditViewModel(inventoryReceipts, this.Mapper);
            ViewBag.storeId = storeId;
            return View(model);
        }

        public JsonResult LoadImportInventory(JQueryDataTableParamModel param, int? status, int storeId)
        {
            // TODO: remove hard-code - duydt
            var api = new InventoryReceiptApi();
            var inventoryReceipts = api.GetImportInventoryReceiptByStore(storeId).OrderByDescending(a => a.CreateDate);
            if (status != 3)
            {
                inventoryReceipts = api.GetImportInventoryReceiptByStore(storeId).Where(q => q.Status == status).OrderByDescending(a => a.CreateDate);
            }
            int count = 1;
            try
            {
                var rs = inventoryReceipts
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Name.ToLower().Contains(param.sSearch.Trim().ToLower()))
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.Name) ? "Không xác định" : a.Name,
                        a.Creator,
                        a.Amount,
                        a.CreateDate?.ToString("dd/MM/yyyy") ?? "---",
                        a.Status,
                        a.Notes,
                        a.Provider.ProviderName,
                        a.ReceiptID                        
                        });
                var totalRecords = inventoryReceipts.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }
        }

        public async Task<ActionResult> ImportInventory(int brandId, string storeId)
        {
            ViewBag.storeId = storeId;
            var model = new InventoryReceiptEditViewModel();
            PrepareImportInventoryReceipt(model, brandId);
            return View(model);
        }
        #endregion

        #region Prepare
        private void PrepareImportInventoryReceipt(InventoryReceiptEditViewModel model, int brandId)
        {

            var providers = new ProviderApi()
                .GetProvidersByBrand(brandId);
            var creators = new AspNetUserApi().GetActive().Where(q => q.BrandId == brandId);
            var api = new ProviderProductItemMappingApi();
            List<ProviderViewModel> providerList = new List<ProviderViewModel>();
            List<AspNetUserViewModel> creatorList = new List<AspNetUserViewModel>();


            foreach (var item in providers)
            {
                var categories = api.GetProviderProductItems()
                    .Where(q => q.Active && q.ProviderID == item.Id && q.ProductItem.ItemCategory.BrandId == brandId);
                if (categories.Count() != 0)
                {
                    providerList.Add(item);
                }
            }
            foreach (var item in creators)
            {
                creatorList.Add(item);
            }
            model.AvailableCreator = creatorList
                .ToSelectList(q => q.UserName, q => q.UserName.ToString(), q => false);
            model.AvailableProvider = providerList
                .ToSelectList(q => q.ProviderName, q => q.Id.ToString(), q => false);
        }
        private void PrepareExportInventoryReceipt(InventoryReceiptEditViewModel model, int brandId, int storeId)
        {
            //Chỉ lấy những category có productitem
            var categories = new ProductItemCategoryApi()
                .GetItemCategoryByBrand(brandId).ToList();

            var creators = new AspNetUserApi().GetActive().Where(q => q.BrandId == brandId);
            var api = new ProductItemApi();
            List<ProductItemCategoryViewModel> categoriesList = new List<ProductItemCategoryViewModel>();
            List<AspNetUserViewModel> creatorList = new List<AspNetUserViewModel>();
            foreach (var item in categories)
            {
                var productItems = api.GetProductItemsByCategoryId(item.CateID);
                if (productItems.Count() != 0)
                {
                    categoriesList.Add(item);
                }
            }
            foreach (var item in creators)
            {
                creatorList.Add(item);
            }
            model.AvailableCreator = creatorList
                .ToSelectList(q => q.UserName, q => q.UserName.ToString(), q => false);

            model.AvailableItemCategory = categoriesList
                .ToSelectList(q => q.CateName, q => q.CateID.ToString(), q => false);

            model.AvailableStore = (new StoreApi()
                .GetAllStore(brandId).Where(q => q.isAvailable == true && q.ID != storeId))
                .ToSelectList(q => q.Name, q => q.ID.ToString(), q => false);
        }
        private void PrepareTransferInventoryReceipt(InventoryReceiptEditViewModel model, int brandId, int storeId)
        {
            //Chỉ lấy những category có productitem
            var categories = new ProductItemCategoryApi()
                .GetItemCategoryByBrand(brandId).ToList();
            var api = new ProductItemApi();
            List<ProductItemCategoryViewModel> categoriesList = new List<ProductItemCategoryViewModel>();
            foreach (var item in categories)
            {
                var productItems = api.GetProductItemsByCategoryId(item.CateID);
                if (productItems.Count() != 0)
                {
                    categoriesList.Add(item);
                }
            }
            model.AvailableItemCategory = categoriesList
                .ToSelectList(q => q.CateName, q => q.CateID.ToString(), q => false);
            model.AvailableStore = (new StoreApi()
                .GetAllStore(brandId).Where(q => q.isAvailable == true && q.ID != storeId))
                .ToSelectList(q => q.Name, q => q.ID.ToString(), q => false);
        }
        #endregion

        #region View detail of receipt
        public async Task<ActionResult> InventoryReceiptItem(int id, string storeId)
        {
            ViewBag.storeId = storeId;
            var api = new InventoryReceiptItemApi();
            var receiptApi = new InventoryReceiptApi();
            var storeApi = new StoreApi();
            var inventoryReceipt = await receiptApi.GetInventoryReceiptById(id);
            var invetoryReceiptItem = api.GetItemReceiptById(id);
            var model = new InventoryReceiptEditViewModel(inventoryReceipt, this.Mapper);
            model.InventoryReceiptItem = invetoryReceiptItem;
            if (inventoryReceipt.InStoreId != null)
            {
                model.InStoreName = storeApi.GetStoreNameByID((int)inventoryReceipt.InStoreId);
                model.InStoreId = (int)inventoryReceipt.InStoreId;
            }
            if (inventoryReceipt.OutStoreId != null)
            {
                model.OutStoreName = storeApi.GetStoreNameByID((int)inventoryReceipt.OutStoreId);
                model.OutStoreId = (int)inventoryReceipt.OutStoreId;
            }
            return View(model);
        }

        //Do chưa có đang nhập nên tạo 1 controller khác cho instore
        public async Task<ActionResult> InStoreInventoryReceiptItem(int id, string storeId)
        {
            var api = new InventoryReceiptItemApi();
            var receiptApi = new InventoryReceiptApi();
            var storeApi = new StoreApi();
            var inventoryReceipt = await receiptApi.GetInventoryReceiptById(id);
            var invetoryReceiptItem = api.GetItemReceiptById(id);
            var model = new InventoryReceiptEditViewModel(inventoryReceipt, this.Mapper);
            model.InventoryReceiptItem = invetoryReceiptItem;
            model.OutStoreName = storeApi.GetStoreNameByID((int)inventoryReceipt.OutStoreId);
            ViewBag.storeId = storeId;
            return View(model);
        }
        #endregion

        #region ChangeStatus of Import, ExportReceipt
        [HttpPost]
        public async Task<ActionResult> AcceptReceipt(int id, int storeId, int brandId, int receiptType, int otherStoreId)
        {
            var inventoryReceiptApi = new InventoryReceiptApi();
            var costCategoryApi = new CostCategoryApi();
            var costApi = new CostApi();
            var paymentApi = new PaymentApi();

            //Get inventory
            var inventoryReceipt = inventoryReceiptApi.GetInventoryReceiptByIdIqueryable(id).FirstOrDefault();
            //Back up status hien tai cua receipt
            var currentStatus = inventoryReceipt.Status;

            if (inventoryReceipt == null)
            {
                return this.HttpNotFound();
            }

            var sumReceiptAmount = inventoryReceipt.InventoryReceiptItems.Sum(q => (q.Quantity * q.Price));
            var spendingCostInInventoryCategory = costCategoryApi.GetActiveCostCategoriesByBrandId(brandId, (int)SpendingCostTypeEnum.InInventory).FirstOrDefault();
            var receiveCostOutInventoryCategory = costCategoryApi.GetActiveCostCategoriesByBrandId(brandId, (int)ReceiveCostTypeEnum.OutInventory).FirstOrDefault();

            int receivePaymenId = -1;
            int spendPaymenId = -1;
            var costInventoryMappingRevceive = new CostInventoryMapping();
            var costInventoryMappingSpending = new CostInventoryMapping();

            CostViewModel spendCostModel = new CostViewModel //Phieu chi nhap hang
            {
                Amount = sumReceiptAmount,
                CatID = spendingCostInInventoryCategory.CatID,
                CostCode = "PC-NK-" + inventoryReceipt.ReceiptID,
                CostDate = Utils.GetCurrentDateTime(),
                CostStatus = (int)CostStatusEnum.Approved,
                CostDescription = "Phiếu chi nhập kho: " + inventoryReceipt.ReceiptID,
                LoggedPerson = User.Identity.Name,
                StoreId = storeId,
                CostCategoryType = spendingCostInInventoryCategory.Type,
                CostType = (int)CostTypeEnum.SpendingCostTranferIn
            };
            CostViewModel receiveCostModel = new CostViewModel //Phieu thu xuat hang
            {
                Amount = sumReceiptAmount,
                CatID = receiveCostOutInventoryCategory.CatID,
                CostCode = "PT-XK-" + inventoryReceipt.ReceiptID,
                CostDate = Utils.GetCurrentDateTime(),
                CostStatus = (int)CostStatusEnum.Approved,
                CostDescription = "Phiếu thu xuất kho: " + inventoryReceipt.ReceiptID,
                LoggedPerson = User.Identity.Name,
                StoreId = storeId,
                CostCategoryType = receiveCostOutInventoryCategory.Type,
                CostType = (int)CostTypeEnum.ReceiveCostTranferOut
            };

            //Tao Cost
            try
            {
                if (receiptType == (int)ReceiptType.InInventory)//Nhap kho
                {
                    //Phieu chi nhap hang
                    costApi.Create(spendCostModel);
                }
                else if (receiptType == (int)ReceiptType.SoldProduct || receiptType == (int)ReceiptType.OutInventory || receiptType == (int)ReceiptType.DraftInventory)//Xuat kho
                {
                    //Phieu thu xuat hang
                    costApi.Create(receiveCostModel);
                }
                else if (receiptType == (int)ReceiptType.OutChangeInventory)//Chuyen kho di
                {
                    //Phieu thu xuat hang cho kho hien tai
                    costApi.Create(receiveCostModel);
                    //Phieu chi nhap hang cho store dc chuyen den 
                    spendCostModel.StoreId = otherStoreId;
                    costApi.Create(spendCostModel);

                }
                else//Chuyen kho den
                {
                    //Phieu chi nhap hang cho store hien tai
                    costApi.Create(spendCostModel);
                    //Phieu thu xuat hang cho kho chuyen den
                    receiveCostModel.StoreId = otherStoreId;
                    costApi.Create(receiveCostModel);
                }
            }
            catch
            {
                if (receiveCostModel.CostID != 0)
                {
                    costApi.Delete(receiveCostModel.CostID);
                }
                if (spendCostModel.CostID != 0)
                {
                    costApi.Delete(spendCostModel.CostID);
                }
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
            //Tao payment cho cost
            try
            {
                //Payment cho phieu thu
                if (receiveCostModel.CostID != 0)
                {
                    var payment = new HmsService.Models.Entities.Payment()
                    {
                        Amount = sumReceiptAmount,
                        CurrencyCode = "VND",
                        FCAmount = (decimal)sumReceiptAmount,
                        PayTime = Utils.GetCurrentDateTime(),
                        Status = (int)PaymentStatusEnum.Approved,
                        Type = (int)PaymentTypeEnum.Cash,
                        CostID = receiveCostModel.CostID
                    };
                    receivePaymenId = paymentApi.CreatePaymentReturnId(payment);
                }

                //Payment cho phieu chi
                if (spendCostModel.CostID != 0)
                {
                    var payment = new HmsService.Models.Entities.Payment()
                    {
                        Amount = sumReceiptAmount,
                        CurrencyCode = "VND",
                        FCAmount = (decimal)sumReceiptAmount,
                        PayTime = Utils.GetCurrentDateTime(),
                        Status = (int)PaymentStatusEnum.Approved,
                        Type = (int)PaymentTypeEnum.Cash,
                        CostID = spendCostModel.CostID
                    };
                    spendPaymenId = paymentApi.CreatePaymentReturnId(payment);
                }

            }
            catch
            {
                if (receivePaymenId != -1)
                {
                    costApi.Delete(receivePaymenId);
                }
                if (spendPaymenId != -1)
                {
                    costApi.Delete(spendPaymenId);
                }
                if (receiveCostModel.CostID != 0)
                {
                    costApi.Delete(receiveCostModel.CostID);
                }
                if (spendCostModel.CostID != 0)
                {
                    costApi.Delete(spendCostModel.CostID);
                }
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            //Tao bang mapping
            var costInventoryMappingApi = new CostInventoryMappingApi();
            try
            {
                //Mapping cho phieu thu
                if (receiveCostModel.CostID != 0)
                {
                    var costInventoryMappingModel = new CostInventoryMappingViewModel
                    {
                        CostID = receiveCostModel.CostID,
                        ReceiptID = inventoryReceipt.ReceiptID,
                        ProviderID = inventoryReceipt.ProviderId,
                        StoreId = receiveCostModel.StoreId,
                    };
                    costInventoryMappingRevceive = costInventoryMappingApi.CreateReturnCostInventoryMapping(costInventoryMappingModel.ToEntity());
                }
                //Mapping cho phieu chi
                if (spendCostModel.CostID != 0)
                {
                    var costInventoryMappingModel = new CostInventoryMappingViewModel
                    {
                        CostID = spendCostModel.CostID,
                        ReceiptID = inventoryReceipt.ReceiptID,
                        ProviderID = inventoryReceipt.ProviderId,
                        StoreId = spendCostModel.StoreId,
                    };
                    costInventoryMappingSpending = costInventoryMappingApi.CreateReturnCostInventoryMapping(costInventoryMappingModel.ToEntity());
                }

            }
            catch
            {
                if (costInventoryMappingRevceive != null)
                {
                    costInventoryMappingApi.DeleteCostInventoryMapping(costInventoryMappingRevceive);
                }
                if (costInventoryMappingSpending != null)
                {
                    costInventoryMappingApi.DeleteCostInventoryMapping(costInventoryMappingSpending);
                }
                if (receivePaymenId != -1)
                {
                    costApi.Delete(receivePaymenId);
                }
                if (spendPaymenId != -1)
                {
                    costApi.Delete(spendPaymenId);
                }
                if (receiveCostModel.CostID != 0)
                {
                    costApi.Delete(receiveCostModel.CostID);
                }
                if (spendCostModel.CostID != 0)
                {
                    costApi.Delete(spendCostModel.CostID);
                }

                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }


            try
            {
                inventoryReceipt.Status = (int)InventoryReceiptStatusEnum.Approved;
                await inventoryReceiptApi.EditAsync(id, new InventoryReceiptViewModel(inventoryReceipt));

                if (inventoryReceipt.ReceiptType == 0)
                {
                    //return this.RedirectToAction("ListImportInventory", "ProductInventory");
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else if (inventoryReceipt.ReceiptType == 1)
                {
                    //return this.RedirectToAction("ListGetTransferInventory", "ProductInventory");
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else if (inventoryReceipt.ReceiptType == 2)
                {
                    //return this.RedirectToAction("ListTransferInventory", "ProductInventory");
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    //return this.RedirectToAction("ListExportInventory", "ProductInventory");
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {

                if (costInventoryMappingRevceive != null)
                {
                    costInventoryMappingApi.DeleteCostInventoryMapping(costInventoryMappingRevceive);
                }
                if (costInventoryMappingSpending != null)
                {
                    costInventoryMappingApi.DeleteCostInventoryMapping(costInventoryMappingSpending);
                }
                if (receivePaymenId != -1)
                {
                    costApi.Delete(receivePaymenId);
                }
                if (spendPaymenId != -1)
                {
                    costApi.Delete(spendPaymenId);
                }
                if (receiveCostModel.CostID != 0)
                {
                    costApi.Delete(receiveCostModel.CostID);
                }
                if (spendCostModel.CostID != 0)
                {
                    costApi.Delete(spendCostModel.CostID);
                }
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }




        }

        [HttpPost]
        public async Task<ActionResult> RejectReceipt(int id)
        {
            var api = new InventoryReceiptApi();
            var inventoryReceipt = await api.GetInventoryReceiptById(id);
            if (inventoryReceipt == null)
            {
                return this.HttpNotFound();
            }
            inventoryReceipt.Status = (int)InventoryReceiptStatusEnum.Reject;
            await api.EditAsync(id, inventoryReceipt);
            return this.RedirectToAction("ListImportInventory", "ProductInventory");
        }

        [HttpPost]
        public async Task<ActionResult> CancelReceipt(int id)
        {
            var api = new InventoryReceiptApi();
            var inventoryReceipt = await api.GetInventoryReceiptById(id);
            inventoryReceipt.Status = (int)InventoryReceiptStatusEnum.Canceled;
            await api.EditAsync(id, inventoryReceipt);
            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region ChangeStatus of Transfer, GetTransferInventory
        [HttpPost]
        public async Task<ActionResult> AcceptCancel(int id)
        {
            var api = new InventoryReceiptApi();
            var costApi = new CostApi();
            var paymentApi = new PaymentApi();
            var inventoryReceipt = await api.GetInventoryReceiptById(id);
            if (inventoryReceipt == null)
            {
                return this.HttpNotFound();
            }

            var listCostIventoryMapping = api.GetInventoryReceiptByIdIqueryable(id).FirstOrDefault().CostInventoryMappings.ToList();

            try
            {
                foreach (var item in listCostIventoryMapping)
                {
                    var listPayment = paymentApi.GetPaymentByCostId(item.CostID).ToList();
                    foreach (var item2 in listPayment)
                    {
                        paymentApi.Delete(item2.PaymentID);
                    }
                    costApi.Delete(item.CostID);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }

            try
            {
                inventoryReceipt.Status = (int)InventoryReceiptStatusEnum.Canceled;
                await api.EditAsync(id, inventoryReceipt);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }



            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> InStoreCancelRequest(int id)
        {
            var api = new InventoryReceiptApi();
            var inventoryReceipt = await api.GetInventoryReceiptById(id);
            if (inventoryReceipt == null)
            {
                return this.HttpNotFound();
            }
            inventoryReceipt.Status = (int)InventoryReceiptStatusEnum.InStoreCancelRequested;
            await api.EditAsync(id, inventoryReceipt);

            return this.RedirectToAction("ListTransferInventory", "ProductInventory");
        }

        [HttpPost]
        public async Task<ActionResult> OutStoreCancelRequest(int id)
        {
            var api = new InventoryReceiptApi();
            var inventoryReceipt = await api.GetInventoryReceiptById(id);
            if (inventoryReceipt == null)
            {
                return this.HttpNotFound();
            }
            inventoryReceipt.Status = (int)InventoryReceiptStatusEnum.OutStoreCancelRequested;
            await api.EditAsync(id, inventoryReceipt);

            return this.RedirectToAction("ListGetTransferInventory", "ProductInventory");
        }
        #endregion

        #region JsonImportInventory
        [HttpPost]
        public JsonResult GetCurrentProviderCategories(int ProviderId, int brandId)
        {
            //var count = 0;
            List<dynamic> listDt = new List<dynamic>();
            var api = new ProviderProductItemMappingApi();
            var categories = api.GetProviderProductItems()
                .Where(q => q.Active && q.ProviderID == ProviderId && q.ProductItem.ItemCategory.BrandId == brandId)
                .Select(q => new
                {
                    CategoryId = q.ProductItem.CatID,
                    CategoryName = q.ProductItem.ItemCategory.CateName,
                }).Distinct();

            return Json(new { categories = categories.ToArray() }, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        public JsonResult GetListProvider()
        {
            //var count = 0;
            List<dynamic> listDt = new List<dynamic>();
            var api = new ProviderApi();
            var providers = api.GetProviders()
                .Where(q => q.IsAvailable == true)
                .Select(q => new
                {
                    Value = q.Id,
                    Text = q.ProviderName,
                }).Distinct();

            return Json(new { providers = providers.ToArray() }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SelectChangeItemByProviderId(int itemCatId, int? ProviderId)
        {
            IEnumerable<ProviderProductItemMappingViewModel> ProductItems = null;

            var productItemApi = new ProductItemApi();
            var providerProductItemApi = new ProviderProductItemMappingApi();
            if (ProviderId == null)
            {
                //ProductItems = _providerProductItemService.GetProviderProductItems().Where(q => q.Active);
                ProductItems = productItemApi.GetProductItemsByCategoryId(itemCatId).Select(q => new ProviderProductItemMappingViewModel()
                {
                    ProductItemID = q.ItemID,
                    ProductItem = q,
                });
            }
            else
            {
                ProductItems = providerProductItemApi.GetProviderProductItems().Where(q => q.Active
                && q.ProviderID == ProviderId && q.ProductItem.CatID == itemCatId);
            }
            var data = ProductItems.Select(a => new
            {
                ItemId = a.ProductItemID,
                a.ProductItem.ItemName,
                a.ProductItem.Unit,
                a.ProductItem.Unit2,
                a.ProductItem.Price,
                a.ProductItem.UnitRate,
            });
            return Json(new
            {
                data = data.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        public JsonResult SelectChangeItemByCategoryId(int itemCatId)
        {
            IEnumerable<ProductItemViewModel> ProductItems = null;
            var api = new ProductItemApi();
            ProductItems = api.GetProductItemsByCategoryId(itemCatId);
            var data = ProductItems.Select(a => new
            {
                ItemId = a.ItemID,
                a.ItemName,
                a.Unit,
                a.Unit2,
                a.Price,
                IndexPriority = a.IndexPriority ?? 1,
                a.UnitRate
            });
            return Json(new
            {
                data = data.ToArray()
            }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Xuất hàng
        public ActionResult ListExportInventory(string storeId)
        {
            ViewBag.storeId = storeId;
            var api = new InventoryReceiptApi();
            var inventoryReceipts = api.GetExportInventoryReceiptByStore(int.Parse(storeId));
            var model = new InventoryReceiptEditViewModel(inventoryReceipts, this.Mapper);

            return View(model);
        }

        public JsonResult LoadExportInventory(JQueryDataTableParamModel param, int? status, int storeId)
        {
            var api = new InventoryReceiptApi();
            var inventoryReceipts = api.GetExportInventoryReceiptByStore(storeId).OrderByDescending(a => a.CreateDate);
            if (status != 3)
            {
                inventoryReceipts = api.GetExportInventoryReceiptByStore(storeId).Where(q => q.Status == status).OrderByDescending(a => a.CreateDate);
            }
            int count = 1;
            try
            {
                var rs = inventoryReceipts
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Name.ToLower().Contains(param.sSearch.Trim().ToLower()))
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.Name) ? "Không xác định" : a.Name,
                        a.Creator,
                        a.Amount,
                        a.CreateDate?.ToString("dd/MM/yyyy") ?? "---",
                        a.Status,
                        a.Notes,
                        a.ReceiptType,
                        a.ReceiptID
                        });
                var totalRecords = inventoryReceipts.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }
        }

        public async Task<ActionResult> ExportInventory(int brandId, string storeId)
        {
            ViewBag.storeId = storeId;
            var model = new InventoryReceiptEditViewModel();
            PrepareExportInventoryReceipt(model, brandId, int.Parse(storeId));
            return View(model);
        }
        #endregion

        #region Chuyển hàng đi
        public ActionResult ListTransferInventory(string storeId)
        {
            ViewBag.storeId = storeId;
            var api = new InventoryReceiptApi();
            var inventoryReceipts = api.GetOutStoreInventoryReceiptByStore(int.Parse(storeId));
            var model = new InventoryReceiptEditViewModel(inventoryReceipts, this.Mapper);

            return View(model);
        }

        public JsonResult LoadTransferInventory(JQueryDataTableParamModel param, int? status, int storeId)
        {
            var api = new InventoryReceiptApi();
            var inventoryReceipts = api.GetOutStoreInventoryReceiptByStore(storeId).OrderByDescending(a => a.CreateDate);
            if (status != 3)
            {
                inventoryReceipts = api.GetOutStoreInventoryReceiptByStore(storeId).Where(q => q.Status == status).OrderByDescending(a => a.CreateDate);
            }
            var storeApi = new StoreApi();
            string name = "";
            int count = 1;
            try
            {
                var rs = inventoryReceipts
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Name.ToLower().Contains(param.sSearch.Trim().ToLower()))
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.Name) ? "Không xác định" : a.Name,
                        a.Creator,
                        a.Amount,
                        a.CreateDate?.ToString("dd/MM/yyyy") ?? "---",
                        a.Status,
                        a.Notes,
                        name = storeApi.GetStoreNameByID((int)a.InStoreId).ToString(),
                        a.ReceiptType,
                        a.ReceiptID
                        });
                var totalRecords = inventoryReceipts.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }
        }

        public async Task<ActionResult> TransferInventory(int brandId, string storeId)
        {
            ViewBag.storeId = storeId;
            var model = new InventoryReceiptEditViewModel();
            PrepareTransferInventoryReceipt(model, brandId, int.Parse(storeId));
            return View(model);
        }
        #endregion

        #region Chuyển hàng đến
        public ActionResult ListGetTransferInventory(int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            var api = new InventoryReceiptApi();
            var inventoryReceipts = api.GetInStoreInventoryReceiptByStore(storeId);
            var model = new InventoryReceiptEditViewModel(inventoryReceipts, this.Mapper);
            return View(model);
        }

        public JsonResult LoadGetTransferInventory(JQueryDataTableParamModel param, int status, int storeId)
        {
            var api = new InventoryReceiptApi();
            var inventoryReceipts = api.GetInStoreInventoryReceiptByStore(storeId).OrderByDescending(a => a.CreateDate);
            if (status != 3)
            {
                inventoryReceipts = api.GetInStoreInventoryReceiptByStore(storeId).Where(q => q.Status == status).OrderByDescending(a => a.CreateDate);
            }
            var storeApi = new StoreApi();
            int count = 1;
            try
            {
                var rs = inventoryReceipts
                    .Where(a => string.IsNullOrEmpty(param.sSearch) || a.Name.ToLower().Contains(param.sSearch.Trim().ToLower()))
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.Name) ? "Không xác định" : a.Name,
                        a.Creator,
                        a.Amount,
                        a.CreateDate?.ToString("dd/MM/yyyy") ?? "---",
                        a.Status,
                        a.Notes,
                        storeApi.GetStoreNameByID((int)a.OutStoreId),
                        a.ReceiptType,
                        a.ReceiptID,
                        a.OutStoreId
                        });
                var totalRecords = inventoryReceipts.Count();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch
            {
                return Json(new { success = false, message = "Error" });
            }
        }
        #endregion

        #region Thêm đơn hàng
        //Post import data in database
        [HttpPost]
        public JsonResult ImportInventory(string data, int storeId)
        {
            var model = JsonConvert.DeserializeObject<ImportExportItemModel>(data);
            var inventoryReceiptApi = new InventoryReceiptApi();
            var itemApi = new InventoryReceiptItemApi();
            //var id = Session["storeId"].ToString();
            //var storeId = Convert.ToInt32(id);
            var time = Utils.GetCurrentDateTime().Ticks;
            try
            {
                //Push InventoryReceipt
                var receiptInventory = new InventoryReceiptViewModel
                {
                    CreateDate = Utils.GetCurrentDateTime(),
                    ReceiptType = (int)ReceiptType.InInventory,
                    //ReceiptType = model.ReceiptTypeId == 2 ? 1 : model.ReceiptTypeId,
                    Status = (int)InventoryReceiptStatusEnum.New, //0 Giá trị mặc định là chờ duyệt,
                    Notes = model.Notes,
                    Name = "PNK-" + time,
                    InvoiceNumber = model.InvoiceNumber,
                    Creator = model.Creator,
                    StoreId = storeId,
                    ProviderId = model.ProviderId,
                    ChangeDate = Convert.ToDateTime(model.ImportDate),
                    Amount = model.Amount
                };
                inventoryReceiptApi.Create(receiptInventory);

                var listReceiptItem = new List<InventoryReceiptItemViewModel>();
                try
                {
                    //Push InventoryReceiptItem
                    foreach (var item in model.ReceiptItems)
                    {
                        var receiptItem = new InventoryReceiptItemViewModel()
                        {
                            ReceiptID = receiptInventory.ReceiptID,
                            ItemID = item.Id,
                            Quantity = (int)item.Quantity,
                            Price = item.Price,
                            DateExpired = item.ExpDate.ToDateTime().GetStartOfDate()
                        };
                        itemApi.Create(receiptItem);
                        listReceiptItem.Add(receiptItem);
                    }
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {
                    foreach (var item in listReceiptItem)
                    {
                        itemApi.DeleteByEntity(item.ToEntity());
                    }
                    inventoryReceiptApi.Delete(receiptInventory.ReceiptID);
                    return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại!" }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ExportInventory(string data, int storeId)
        {
            var model = JsonConvert.DeserializeObject<ImportExportItemModel>(data);
            try
            {
                var api = new InventoryReceiptApi();
                var itemApi = new InventoryReceiptItemApi();
                //var id = Session["storeId"].ToString();
                //var storeId = Convert.ToInt32(id);
                var time = Utils.GetCurrentDateTime().Ticks;
                //Push InventoryReceipt
                var receiptInventory = new InventoryReceiptViewModel
                {
                    CreateDate = Utils.GetCurrentDateTime(),
                    ReceiptType = model.ReceiptTypeId,
                    Status = (int)InventoryReceiptStatusEnum.New, //0 Giá trị mặc định là chờ duyệt,
                    Notes = model.Notes,
                    Name = "PXK-" + time,
                    Creator = model.Creator,
                    StoreId = storeId,
                    ChangeDate = Convert.ToDateTime(model.ExportDate),
                    Amount = model.Amount
                };
                api.Create(receiptInventory);

                var listReceiptItem = new List<InventoryReceiptItemViewModel>();
                try
                {
                    //Push InventoryReceiptItem
                    foreach (var item in model.ReceiptItems)
                    {
                        var receiptItem = new InventoryReceiptItemViewModel()
                        {
                            ReceiptID = receiptInventory.ReceiptID,
                            ItemID = item.Id,
                            Quantity = (int)item.Quantity,
                            Price = item.Price,
                            DateExpired = item.ExpDate.ToDateTime().GetStartOfDate()
                        };
                        itemApi.Create(receiptItem);
                        listReceiptItem.Add(receiptItem);
                    }
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {
                    foreach (var item in listReceiptItem)
                    {
                        itemApi.DeleteByEntity(item.ToEntity());
                    }
                    api.Delete(receiptInventory.ReceiptID);
                    return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại!" }, JsonRequestBehavior.AllowGet);
            }
        }

        //Chuyển kho đi
        [HttpPost]
        public JsonResult TransferInventory(string data, int storeId)
        {
            var costApi = new CostApi();
            var paymentApi = new PaymentApi();
            var costInventoryMappingApi = new CostInventoryMappingApi();
            var model = JsonConvert.DeserializeObject<ImportExportItemModel>(data);
            //Tạo receipt inventory
            try
            {
                var api = new InventoryReceiptApi();
                var itemApi = new InventoryReceiptItemApi();
                //var id = Session["storeId"].ToString();
                //var storeId = Convert.ToInt32(id);
                var time = Utils.GetCurrentDateTime().Ticks;
                //Push InventoryReceipt
                var receiptInventory = new InventoryReceiptViewModel
                {
                    CreateDate = Utils.GetCurrentDateTime(),
                    ReceiptType = (int)ReceiptType.OutChangeInventory,
                    Status = (int)InventoryReceiptStatusEnum.New, //0 Giá trị mặc định là chờ duyệt,
                    Notes = model.Notes,
                    Name = "PCK-" + time,
                    Creator = model.Creator,
                    StoreId = storeId,
                    InStoreId = model.InStoreId,
                    OutStoreId = storeId,
                    ChangeDate = Convert.ToDateTime(model.ExportDate),
                    Amount = model.Amount
                };
                api.Create(receiptInventory);

                var listReceiptItem = new List<InventoryReceiptItemViewModel>();
                //Tạo receipt inventory item
                try
                {
                    //Push InventoryReceiptItem
                    foreach (var item in model.ReceiptItems)
                    {
                        var receiptItem = new InventoryReceiptItemViewModel()
                        {
                            ReceiptID = receiptInventory.ReceiptID,
                            ItemID = item.Id,
                            Quantity = (int)item.Quantity,
                            Price = item.Price,
                            DateExpired = item.ExpDate.ToDateTime().GetStartOfDate()
                        };
                        itemApi.Create(receiptItem);
                        listReceiptItem.Add(receiptItem);
                    }
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
                catch (Exception)
                {
                    foreach (var item in listReceiptItem)
                    {
                        itemApi.DeleteByEntity(item.ToEntity());
                    }
                    api.Delete(receiptInventory.ReceiptID);
                    return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại!" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại!" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion
    }
}