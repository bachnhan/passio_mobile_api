using HmsService.Models;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using HmsService.ViewModels;
using Newtonsoft.Json.Linq;
using Wisky.SkyAdmin.Manage.Printer;

namespace Wisky.SkyAdmin.Manage.Areas.VATOrders.Controllers
{
    public class VATOrderController : DomainBasedController
    {
        // GET: VATOrders/VATOrder
        public ActionResult Index(int storeId)
        {
            ViewBag.storeId = storeId.ToString();
            return View();
        }

        public JsonResult LoadVATOrder(JQueryDataTableParamModel param, int brandId, string _date)
        {

            var VATorderApi = new VATOrderApi();
            var providerApi = new ProviderApi();

            var rpDate = _date.ToDateTime();
            var startTime = rpDate.GetStartOfDate();
            var endTime = rpDate.GetEndOfDate();
            //filter status and type
            var listVATOrder = VATorderApi.GetRentsByTimeRange(startTime, endTime);

            int count = 0;
            count = param.iDisplayStart + 1;
            var totalRecords = listVATOrder.Count();
            try
            {
                var list = listVATOrder
                    .Where(a => string.IsNullOrEmpty(param.sSearch)
                    || a.InvoiceID.ToString().ToLower().Contains(param.sSearch.ToLower()))
                    .ToList()
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.InvoiceNo) ? "Không xác định" : a.InvoiceNo,
                        a.BrandID,
                        a.Total,
                        a.VATAmount,
                        a.Type,
                        a.CheckInDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        a.CheckInPerson,
                        a.InvoiceID,
                        a.Notes,
                        a.Provider.ProviderName,
                        a.Provider.Address,
                        a.Provider.Phone,
                        });

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = totalRecords,
                    aaData = list
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public JsonResult LoadVATOrderDetail(JQueryDataTableParamModel param, int id)
        {
            var VATOrderMappingApi = new VATOrderMappingApi();
            var VATOrderMapping = VATOrderMappingApi.GetVATOrderMappingByInvoiceId(id)
                .OrderBy(a => a.VATOrder.InvoiceNo)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength);
            //load JSon
            var vatOrderApi = new VATOrderApi();
            var VATOrder = vatOrderApi.GetVATOrderByInvoiceId(id);
            int count = 1;
            var jsonDetail = VATOrder.VATOrderDetail;
            var totalCountJSon = 0;
            List<IConvertible[]> resultList = new List<IConvertible[]>();
            //parse JSon
            if (jsonDetail != null)
            {
                dynamic arr = JObject.Parse(jsonDetail);
                foreach (JProperty o in arr)
                {
                    totalCountJSon++;
                    string name = o.Name;
                    if (!name.Equals("0"))
                    {
                        var value = o.Value.ToList();
                        var element = new IConvertible[]
                        {
                        count++,
                        value[0].ToString(),
                        value[3].ToString(),
                        Double.Parse(value[1].ToString()),
                        int.Parse(value[2].ToString()),
                        int.Parse(value[1].ToString()) * int.Parse(value[2].ToString()) * 0.1,
                        id,
                        };
                        resultList.Add(element);
                    }
                }
            }
            //end parse
            #region vatordermapping
            /*
            var totalCount = VATOrderMapping.Count();
            if (totalCount > 0)
            {
                var rentId = VATOrderMapping.FirstOrDefault().RentID;
                try
                {
                    var orderApi = new OrderApi();
                    var order = orderApi.BaseService.Get().Where(o => o.RentID == rentId).FirstOrDefault();


                    var orderDetails = order.OrderDetails;

                    var list = orderDetails.Select(a => new IConvertible[]
                    {
                count++,
                a.Product.ProductName,
                a.UnitPrice,
                a.Quantity,
                a.UnitPrice * a.Quantity * 0.1,
                a.RentID,
                    }).ToList();
                    if (totalCount > 1)
                    {
                        for (int i = 1; i < totalCount; i++)
                        {
                            rentId = VATOrderMapping.Skip(1).FirstOrDefault().RentID;
                            order = orderApi.BaseService.Get().Where(o => o.RentID == rentId).FirstOrDefault();

                            orderDetails = order.OrderDetails;

                            var tmpList = orderDetails.Select(a => new IConvertible[]
                            {
                        count++,
                        a.Product.ProductName,
                        a.UnitPrice,
                        a.Quantity,
                        a.UnitPrice * a.Quantity * 0.1,
                        a.RentID,
                            }).ToList();
                            list.AddRange(tmpList);
                            totalCountJSon = totalCountJSon + totalCount;
                        }
                    }
                    /*return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = totalCount,
                        iTotalDisplayRecords = totalCount,
                        aaData = list
                    }, JsonRequestBehavior.AllowGet);
                    resultList.AddRange(list);
                }
                catch (Exception)
                {

                    throw;
                }
            }*/
            #endregion
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalCountJSon,
                iTotalDisplayRecords = totalCountJSon,
                aaData = resultList
            }, JsonRequestBehavior.AllowGet);
        }

        //Add new VAT Order
        public ActionResult AddVATOrder(int brandId)
        {
            ViewBag.brandId = brandId.ToString();
            var model = new VATOrderEditViewModel();
            this.PrepareCreate(model, brandId);
            return View(model);
        }

        private void PrepareCreate(VATOrderEditViewModel model, int brandId)
        {
            var vatOrderApi = new VATOrderApi();
            var providerApi = new ProviderApi();
            var orderApi = new OrderApi();
            model.AvailableProvider = providerApi.GetProvidersByBrand(brandId).Select(q => new SelectListItem()
            {
                Text = q.ProviderName,
                Value = q.Id.ToString(),
                Selected = false,
            });
            model.AvailableOrder = orderApi.GetOrderByBrand(brandId).Select(a => new SelectListItem()
            {
                Text = a.InvoiceID,
                Value = a.RentID.ToString() + "+" + a.TotalAmount.ToString(),
                Selected = false,
            });
        }

        [HttpPost]
        public async Task<JsonResult> Create(VATOrderEditViewModel vatOrderModel)
        {
            var vatOrderApi = new VATOrderApi();
            vatOrderModel.CheckInDate = DateTime.Now;
            try
            {
                await vatOrderApi.CreateVATOrder(vatOrderModel);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Thêm mới hóa đơn thất bại." });
            }

            return Json(new { success = true, message = "Thêm mới hóa đơn thành công." });
        }

        [HttpPost]
        public async Task<JsonResult> CreateProvider(ProviderViewModel providerModel)
        {
            var providerApi = new ProviderApi();
            providerModel.IsAvailable = true;
            try
            {
                await providerApi.CreateProviderAsync(providerModel);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Thêm mới đối tác thất bại." });
            }
            //var provider = providerApi.BaseService.Get().Where(o => o.ProviderName == providerModel.ProviderName).LastOrDefault();
            var list = providerApi.GetProviders().Select(q => new SelectListItem()
            {
                Text = q.ProviderName,
                Value = q.Id.ToString(),
                Selected = false,
            });


            int id = providerModel.Id;
            return Json(new { success = true, message = "Thêm mới đối tác thành công.", newdata = list });
        }

        public JsonResult checkInvoiceNo(string code)
        {
            var vatOrderApi = new VATOrderApi();
            var result = vatOrderApi.Get()
                .Where(q => q.InvoiceNo == code)
                .ToList();

            if (result.Count == 0)
            {
                return Json(new { success = true });
            }
            return Json(new { success = false });
        }

        public ActionResult getItemOfOrder(int id)
        {
            var orderDetailApi = new OrderDetailApi();
            var orderDetail = orderDetailApi.GetOrderDetailsByRentId(id).ToList();
            var totalCount = orderDetail.Count();
            var list = orderDetail.Select(a => new IConvertible[]
            {
                a.Product.ProductName,
                a.UnitPrice,
                a.Quantity,
                a.RentID,
            });
            try
            {
                return Json(new
                {
                    result = list,
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }


        // GET: VATOrders/ExportVATInvoice
        public ActionResult ExportVATInvoice(int invoiceId, int mode)
        {
            // Declare APIs
            var vatOrderApi = new VATOrderApi();
            try
            {
                // Get VAT order by its ID
                var vatOrder = vatOrderApi.GetVATOrderByInvoiceId(invoiceId);

                if (vatOrder != null)
                {
                    PDFPrinter printer = new PDFPrinter();

                    byte[] bytes = printer.PrintPDF(vatOrder);
                    var fileDownloadName = Properties.Resources.VAT_INVOICE_FILE_NAME+Properties.Resources.UNDERSCORE + DateTime.Now.ToString(Properties.Resources.DATE_TIME_FORMAT) + ".pdf";
                    var contentType = "application/pdf";

                    // Download pdf
                    if (mode == 1)
                    {
                        return File(bytes, contentType, fileDownloadName);
                    }

                    // View and print directly
                    Response.AddHeader("Content-Disposition", "inline; filename=" + fileDownloadName);

                    return File(bytes, contentType);
                }
            }
            catch (Exception ex)
            {
                // Handle exception
                Debug.WriteLine(ex.StackTrace);

                return Json(new { success = false, message = Properties.Resources.ERROR_MESSAGE });
            }

            // return error message in case VAT order is not existed
            return Json(new { success = false, message = Properties.Resources.ERROR_MESSAGE });
        }
    }
}