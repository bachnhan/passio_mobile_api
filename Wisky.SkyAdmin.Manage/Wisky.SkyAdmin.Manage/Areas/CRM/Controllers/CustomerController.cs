using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.Sdk;
using HmsService.ViewModels;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.CRM.Controllers
{
    public class CustomerController : DomainBasedController
    {
        // GET: CRM/Customer
        public ActionResult Index()
        {
            return View();
        }


        //Load All Store ()
        //return Json contains Store.Name and Store.ID
        [HttpPost]
        public async Task<JsonResult> LoadAllStore(int brandID)
        {
            var storeApi = new StoreApi();
            //get stores 
            var storeList = await storeApi.GetAllStore(brandID)
                                               .Select(a => new
                                               {
                                                   StoreId = a.ID,
                                                   Name = a.Name
                                               }).ToListAsync();

            return Json(storeList);
        }


        //Load All Customer Type
        //return Json contains Name and ID
        [HttpPost]
        public JsonResult LoadAllCustomerType(int brandID)
        {
            var storeApi = new StoreApi();
            var customerTypeAPI = new CustomerTypeApi();
            //get customer types

            //set brain id = 1 to view page

            var typeList = customerTypeAPI.GetAllCustomerTypes(brandID).Select(a => new
            {
                CustomerTypeId = a.ID,
                Name = a.CustomerType1
            }).ToList();
            return Json(typeList);
        }


        //Load all filter
        //return Json contains Name and ID
        public async Task<JsonResult> LoadAllFilter(int brandID)
        {
            var customerFilterApi = new CustomerFilterApi();


            //get customer types
            //set brain id = 1 to view page

            var customerFilterList = await customerFilterApi.GetAllFilter(brandID).Select(a => new
            {
                FilterId = a.ID,
                Name = a.Name
            }).ToListAsync();


            return Json(customerFilterList);
        }


        //Load CustomerDetail base on id
        public ActionResult LoadCustomerDetail(int accountId)
        {
            var customerApi = new CustomerApi();
            var membershipCardApi = new MembershipCardApi();
            var accountApi = new AccountApi();
            var customerId = membershipCardApi.GetMembershipCardById(accountApi.Get(accountId)
                            .MembershipCardId.GetValueOrDefault()).CustomerId;
            var customer = customerApi.Get(customerId);

            if (customer != null)
            {
                return Json(new
                {
                    success = true,
                    customer = new
                    {
                        customerName = customer.Name,
                        Gender = customer.Gender == null ? "Không Xác định" : customer.Gender == true ? "Nam" : "Nữ",
                        dateofbirth = customer.BirthDay != null ? customer.BirthDay.Value.ToString("DD/mm/YYY") : "N/A",
                        phoneNumber = customer.Phone,
                        EmailAddress = customer.Email == "" || customer.Email == null ? "N/A" : customer.Email,
                        Address = customer.Address == "" || customer.Address == null ? "N/A" : customer.Address,
                        CardId = customer.IDCard == "" || customer.IDCard == null ? "N/A" : customer.IDCard,
                        District = customer.District == "" || customer.District == null ? "N/A" : customer.District,
                        City = customer.City == "" || customer.City == null ? "N/A" : customer.City
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, customer = customer }, JsonRequestBehavior.AllowGet);
            }

        }

        //Get list of customters depend on filter and customerType
        //return Json contain data to gen table result
        public async Task<JsonResult> GetListCustomer(JQueryDataTableParamModel param, int? storeId, int? filterId, int? customerTypeId, int brandId)
        {

            int count = 0;
            var accountApi = new AccountApi();

            var customerApi = new CustomerApi();

            var customerFilterApi = new CustomerFilterApi();
            var filter = customerFilterApi.BaseService.Get(filterId.Value);

            var customers = customerApi.GetCustomersByFilter(filter, brandId);

            if (customerTypeId.Value != -1)
            {
                customers = customers.Where(c => c.CustomerTypeId == customerTypeId.Value);
            }


            if (!string.IsNullOrEmpty(param.sSearch))
            {

                customers = customers.Where(a => //string.IsNullOrEmpty(param.sSearch)
                       (a.Name.Contains(param.sSearch)) || a.Phone.Contains(param.sSearch) || a.Address.Contains(param.sSearch));
            }
            int totalRecords = await customers.CountAsync();

            count = param.iDisplayStart + 1;

            var rs = customers.OrderBy(q=> q.CustomerID).Skip(param.iDisplayStart).Take(param.iDisplayLength)
                    .ToList()
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.Name) ? Properties.Resources.UNDEFINE_VN : a.Name,
                        a.Gender.HasValue ? (a.Gender.Value ? "Nam" : "Nữ") : Properties.Resources.UNDEFINE_VN,
                        string.IsNullOrEmpty(a.Email) ? Properties.Resources.UNDEFINE_VN : a.Email,
                        string.IsNullOrEmpty(a.Address) ? Properties.Resources.UNDEFINE_VN : a.Address, 
                        string.IsNullOrEmpty(a.Phone) ? Properties.Resources.UNDEFINE_VN : a.Phone,
                        a.CustomerID ,
                        });


            int totalDisplay = customers.Count();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplay,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);

        }

        //Return Create.cshtml when clicking on button "Them Khach Hang"
        public async Task<ActionResult> Create(int brandId)
        {
            CustomerViewModel model = new CustomerViewModel();
            //await this.PrepareCreate(model, brandId);

            var customerTypeApi = new CustomerTypeApi();
            var typeList = await customerTypeApi.GetActiveAsync();
            var customerTypeList = typeList.Select(q => new SelectListItem()
            {
                Text = q.CustomerType1,
                Value = q.ID.ToString(),
            });
            ViewBag.CustomerTypeList = customerTypeList;

            return View(model);
        }


        //Load list of gender and customerType to Create form
        //public async Task PrepareCreate(CustomerViewModel model, int brandId)
        //{
        //    var customerTypeAPI = new CustomerTypeApi();

        //    var typeList = await customerTypeAPI.GetActiveAsync();

        //    typeList = typeList.Where(t => t.BrandId == brandId).ToList();

        //    model.AvailableType = typeList.Select(q => new SelectListItem()
        //    {
        //        Selected = model.Type.Equals(q.ID),
        //        Text = q.CustomerType1,
        //        Value = q.ID.ToString(),
        //    });

        //    model.AvailableGender = new List<SelectListItem>();

        //    model.AvailableGender.Add(new SelectListItem()
        //    {
        //        Text = "Nam",
        //        Value = "true",
        //    });

        //    model.AvailableGender.Add(new SelectListItem()
        //    {
        //        Text = "Nữ",
        //        Value = "false",
        //    });

        //}

        //create and return json result to Create view 
        [HttpPost]
        public async Task<JsonResult> Create(int brandId, CustomerViewModel model)
        {
            try
            {
                var customerApi = new CustomerApi();
                //sua Customer Type = 1 list de load len Index
                //model.CustomerTypeId = 1;
                model.BrandId = brandId;
                await customerApi.CreateCustomer(model);
            }
            catch (Exception e)
            {

                return Json(new { success = false, message = Resources.Message_VI_Resources.CreateCustomerFailed });
            }

            return Json(new { success = true, message = Resources.Message_VI_Resources.CreateCustomerSuccessfully });
        }

        [HttpPost]
        public async Task<JsonResult> CreateAsync(int brandId, CustomerViewModel model)
        {
            int id = 0;
            try
            {
                model.BrandId = brandId;
                var customerApi = new CustomerApi();
                id = await customerApi.CreateCustomerReturnId(model);
            }
            catch (Exception e)
            {

                return Json(new { success = false, message = Resources.Message_VI_Resources.CreateCustomerFailed });
            }

            return Json(new
            {
                success = true,
                message = Resources.Message_VI_Resources.CreateCustomerSuccessfully,
                customer = new
                {
                    id = id,
                    name = model.Name,
                    phone = model.Phone
                }
            });
        }
        public async Task<JsonResult> CreateAsync2(int brandId, string name, int sex, DateTime date, string phone)
        {
            CustomerViewModel model = new CustomerViewModel
            {
                Name = name,
                Gender = Convert.ToBoolean(sex),
                BirthDay = date,
                Type = 0,
                Phone = phone
            };
            int id = 0;
            try
            {
                model.BrandId = brandId;
                var customerApi = new CustomerApi();
                id = await customerApi.CreateCustomerReturnId(model);
            }
            catch (Exception e)
            {

                return Json(new { success = false, message = Resources.Message_VI_Resources.CreateCustomerFailed });
            }

            return Json(new
            {
                success = true,
                message = Resources.Message_VI_Resources.CreateCustomerSuccessfully,
                customer = new
                {
                    id = id,
                    name = model.Name
                }
            });
        }

        //TODO: HiepBP: Comment, nhớ sửa
        public void PrepareIndex(CustomerViewModel model)
        {
            var accountApi = new AccountApi();
            var membershipCard = new MembershipCardApi();
            model.AvailableAccounts = accountApi.GetActiveAccountByCusId(model.CustomerID)
                .ToSelectList(q => q.AccountName, q => q.AccountID.ToString(), q => false);
            model.AllAccounts = accountApi.GetAccountByCusId(model.CustomerID)
               .ToSelectList(q => q.AccountName, q => q.AccountID.ToString(), q => false);
            model.MembershipCards = membershipCard.GetMembershipCardActiveByCustomerId(model.CustomerID);
            PrepareCount(model);
        }

        public void PrepareCount(CustomerViewModel model)
        {
            var cardApi = new MembershipCardApi();
            var accountApi = new AccountApi();

            model.creditCount = 0;
            model.giftCount = 0;
            model.memberCount = 0;
            model.unknownCount = 0;

            var listCards = cardApi.BaseService.Get(a => a.Active).Where(a => a.CustomerId == model.CustomerID);
            var listAccounts = (from cards in listCards
                                join accounts in accountApi.BaseService.Get(a => a.Active.Value)
                                on cards.Id equals accounts.MembershipCardId
                                group accounts by accounts.Type into g
                                select new
                                {
                                    Type = g.Key,
                                    Count = g.Count(),
                                })
                                .ToList();

            listAccounts.ForEach(a =>
            {
                if (a.Type == null) model.unknownCount += a.Count;
                else if (a.Type == (int)AccountTypeEnum.CreditAccount) model.creditCount += a.Count;
                else if (a.Type == (int)AccountTypeEnum.GiftAccount) model.giftCount += a.Count;
                else if (a.Type == (int)AccountTypeEnum.PointAccount) model.memberCount += a.Count;
            });
        }

        public async Task<ActionResult> CustomerDetail(int id)
        {
            //ViewBag.storeId = RouteData.Values["storeId"].ToString();
            //ViewBag.storeName = RouteData.Values["storeName"].ToString();
            var customerApi = new CustomerApi();
            var accountApi = new AccountApi();
            var customer = customerApi.GetCustomerById(id);
            if (customer == null)
            {
                return HttpNotFound();
            }
            if (customer.AccountID != null)
            {
                customer.DefaultAccount = await accountApi.GetAsync(customer.AccountID);
                customer.ContainAccount = true;
            }
            else
            {
                customer.DefaultAccount = null;
                if (await accountApi.GetActiveAccountByCusId(id).FirstOrDefaultAsync() != null)
                {
                    customer.ContainAccount = true;
                }
                else
                {
                    customer.ContainAccount = false;
                }
            }
            PrepareIndex(customer);
            return View(customer);
        }

        public async Task<ActionResult> SetDefaultAccount(CustomerViewModel model)
        {
            var customerApi = new CustomerApi();
            await customerApi.EditDefaultAccountAsync(model);
            return RedirectToAction("CustomerDetail", new { Id = model.CustomerID });
        }

        public async Task<JsonResult> SetDefaultAccountWithId(int Id)
        {
            var customerApi = new CustomerApi();
            var accountApi = new AccountApi();
            var customer = await customerApi.GetCustomerByAccountIdAsync(Id);
            try
            {
                await customerApi.EditDefaultAccountAsync(customer, Id);
                var defaultAcc = accountApi.Get(Id);

                defaultAcc.Active = true;
                await accountApi.EditAsync(Id, defaultAcc);

                var defaultAccId = defaultAcc.AccountID;
                var defaultAccName = defaultAcc.AccountName;
                var defaultAccBalance = defaultAcc.Balance.GetValueOrDefault().ToString("#,##");

                return Json(new { success = true, detail = new { accID = defaultAccId, accName = defaultAccName, balance = defaultAccBalance }, message = "Success" });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Error" }, JsonRequestBehavior.AllowGet);
            }
            //return RedirectToAction("Index", "Account", new { Id = customer.CustomerID });
        }

        public async Task<ActionResult> GetCustomerProductData(JQueryDataTableParamModel param, int Id)
        {
            var customerProductApi = new CustomerProductMappingApi();
            var customerProducts = customerProductApi.GetCustomerProductByCustomerId(Id);

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                customerProducts = customerProducts.Where(a =>
                                                    a.Product.ProductName.Contains(param.sSearch));
            }
            try
            {
                if (customerProducts.Count() > 0)
                {
                    int count = param.iDisplayStart + 1;
                    var rs = (await customerProducts
                        .OrderByDescending(q => q.UpdateDate)
                        .Skip(param.iDisplayStart)
                        .Take(param.iDisplayLength)
                        .ToListAsync())
                        .Select(q => new IConvertible[]
                            {
                                count++,
                                q.Product.ProductName,
                                q.TotalQuantity.ToString("#,##"),
                                q.UpdateDate.ToString("dd/MM/yyyy")
                            });

                    var totalRecords = await customerProducts.CountAsync();

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = totalRecords,
                        iTotalDisplayRecords = customerProducts.Count(),
                        aaData = rs
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rs = customerProducts.ToList().Select(q => new IConvertible[]
                    {
                        q.Product.ProductName,
                        q.TotalQuantity.ToString("#,##"),
                        q.UpdateDate.ToString("dd/MM/yyyy"),
                    });
                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = rs,
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch
            {
                return Json(new { success = false, message = Resources.Message_VI_Resources.ErrorOccured }, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult LoadStoreFromCustomerOrder(int brandId, int customerID)
        {
            try
            {
                var orderApi = new OrderApi();
                var storeApi = new StoreApi();
                var listStore = storeApi.GetAllActiveStore(brandId).Select(a => new { storeID = a.ID, storeName = a.Name });
                //var listStore = orderApi.getAllOrder().Where(a => a.CustomerID == customerID && a.OrderStatus == 2).Select(a => new { storeID = a.StoreID, storeName = a.Store.Name }).Distinct();
                return Json(new
                {
                    success = true,
                    list = listStore.ToArray(),
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new
                {
                    success = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult LoadOrder(JQueryDataTableParamModel param, int brandId, int customID, int selectedStoreID, string startTime, string endTime)
        {

            var orderApi = new OrderApi();
            IQueryable<Order> listOrder = null;
            var startDate = startTime.ToDateTime().GetStartOfDate();
            var endDate = endTime.ToDateTime().GetEndOfDate();
            var Orders = orderApi.GetAllFinishedOrdersByDateAndCustomer(startDate, endDate, brandId, customID);

            if (selectedStoreID == 0)
            {
                listOrder = Orders;
            }
            else
            {
                listOrder = Orders.Where(a => a.StoreID == selectedStoreID);
            }

            if (!string.IsNullOrWhiteSpace(param.sSearch))
            {
                listOrder = listOrder.Where(q => q.InvoiceID.ToLower().Contains(param.sSearch.ToLower()));
            }
            int count = 0;
            count = param.iDisplayStart + 1;

            //try
            //{
            var result = listOrder
                .OrderByDescending(q => q.CheckInDate)
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .ToList();
            var list = result.Select(a => new object[]
                    {
                        ++count, // 0
                        string.IsNullOrEmpty(a.InvoiceID) ? "N/A" : a.InvoiceID, // 1
                        a.OrderDetailsTotalQuantity, // 2
                        a.FinalAmount, // 3
                        a.CheckInDate.Value.ToString("dd/MM/yyyy HH:mm:ss"), // 4
                        a.OrderType, // 5
                        a.CheckInPerson, // 6
                        a.RentID, // 7
                        a.Store == null ? "N/A" : a.Store.Name, // 8
                        a.Customer!=null ? a.Customer.Name : "N/A", // 9
                        //string.IsNullOrEmpty(a.DeliveryAddress) ? "N/A" : a.DeliveryAddress, //10
                        //a.Customer!=null ? a.Customer.Phone : "N/A", //11
                        //a.Notes !=null? a.Notes : "", //12
                        //a.TotalAmount,// 12
                        //(a.Discount + a.DiscountOrderDetail), // 13
                        //a.Store.Name // 14
                    });
            var totalRecords = listOrder.Count();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = list
            }, JsonRequestBehavior.AllowGet);
        }



        public JsonResult LoadOrderDetail(int id)
        {
            var orderlApi = new OrderApi();
            var order = orderlApi.Get(id);
            var orderDetail = order.OrderDetails.OrderBy(q => q.OrderDate);
            int count = 1;
            var totalCount = orderDetail.Count();
            var list = orderDetail.Select(a => new IConvertible[]
            {
                count++,
                a.Product.ProductName,
                a.UnitPrice,
                a.Quantity,
                a.Status,
                a.Discount,
                a.FinalAmount,
                a.RentID,
            });

            var lblData = new
            {
                cusName = order.Customer != null ? order.Customer.Name : "N/A",
                cusAddr = string.IsNullOrEmpty(order.DeliveryAddress) ? "N/A" : order.DeliveryAddress,
                cusPhone = order.Customer != null ? order.Customer.Phone : "N/A",
                notes = order.Notes != null ? order.Notes : "",
                totalAmount = order.TotalAmount,
                totalDiscount = (order.Discount + order.DiscountOrderDetail),
                store = order.Store.Name,
                payment = order.Payments.GroupBy(q => q.Type)
                                .Select(a => new
                                {
                                    type = ((PaymentTypeEnum)a.Key).DisplayName(),
                                    amount = a.Sum(z => z.Amount)
                                }).ToArray()
            };

            return Json(new
            {
                dataTable = list,
                lblData = lblData
            }, JsonRequestBehavior.AllowGet);
        }

        // Hàm xuất file excel
        public ActionResult ExportOrderTableToExcel(int _id, int storeId, int brandId, string total, string final, string discount)
        {
            var storeService = this.Service<IStoreService>();
            var orderService = this.Service<IOrderService>();
            var orderDetailService = this.Service<IOrderDetailService>();
            var storeApi = new StoreApi();
            var orderApi = new OrderApi();
            var orderDetailApi = new OrderDetailApi();
            var storeName = "";
            if (storeId > 0)
            {
                storeName = storeService.Get(storeId).Name;
            }
            else
            {
                storeName = "Service";
            }

            if (storeId > 0)
            {
                var totalModel = orderDetailApi.GetOrderDetailsByRentId(_id)
                  .Select(g => new
                  {
                      productName = g.Product.ProductName,
                      Price = g.UnitPrice,
                      quality = g.Quantity,
                      discount = g.Discount,
                      finalAmount = g.FinalAmount,

                  });


                #region Export to Excel
                int count = 0;
                MemoryStream ms = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("ChiTietHoaDon");
                    int modelCount = totalModel.Count();
                    char StartHeaderChar = 'A';
                    int StartHeaderNumber = 1;
                    #region Headers
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên sản phẩm";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giá sản phẩm";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Tình trạng";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Thanh toán";
                    ws.Cells["" + ('A') + (modelCount + 2)].Value = "Tổng cộng";
                    var EndHeaderChar = StartHeaderChar;
                    var EndHeaderNumber = StartHeaderNumber;
                    StartHeaderChar = 'A';
                    StartHeaderNumber = 1;
                    #endregion
                    #region Set style for rows and columns
                    ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                        ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                    ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                        ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                    ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                        ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                        .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                        ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                        .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                    ws.View.FreezePanes(2, 1);
                    #endregion
                    #region Set values for cells    

                    foreach (var data in totalModel)
                    {
                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = ++count;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.productName;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.Price);
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.quality;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.discount);
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.finalAmount);

                        StartHeaderChar = 'A';
                    }

                    ws.Cells["" + ('B') + (modelCount + 2)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", total);
                    ws.Cells["" + ('E') + (modelCount + 2)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", discount);
                    ws.Cells["" + ('F') + (modelCount + 2)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", final);
                    #endregion

                    //Set style for excel
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    package.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var fileDownloadName = "ChiTietHoaDon_" + storeName + "_TổngQuanNgày.xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileDownloadName);
                }
                #endregion
            }
            else
            {
                var totalModel = orderDetailApi.GetOrderDetailsByRentId(_id)
                  .Select(g => new
                  {
                      productName = g.Product.ProductName,
                      Price = g.UnitPrice,
                      quality = g.Quantity,
                      discount = g.Discount,
                      finalAmount = g.FinalAmount
                  });

                #region Export to Excel
                int count = 0;

                MemoryStream ms = new MemoryStream();
                using (ExcelPackage package = new ExcelPackage(ms))
                {
                    ExcelWorksheet ws = package.Workbook.Worksheets.Add("TongQuanNhanVien");
                    int modelCount = totalModel.Count();
                    char StartHeaderChar = 'A';
                    int StartHeaderNumber = 1;
                    #region Headers
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên sản phẩm";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giá sản phẩm";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Số lượng";
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giảm giá";
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Thanh toán";
                    ws.Cells["" + ('A') + (modelCount + 2)].Value = "Tổng cộng";

                    var EndHeaderChar = StartHeaderChar;
                    var EndHeaderNumber = StartHeaderNumber;
                    StartHeaderChar = 'A';
                    StartHeaderNumber = 1;
                    #endregion
                    #region Set style for rows and columns
                    ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                        ":" + EndHeaderChar + EndHeaderNumber.ToString()].Style.Font.Bold = true;
                    ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                        ":" + EndHeaderChar + EndHeaderNumber.ToString()].AutoFitColumns();
                    ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                        ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                        .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells["" + StartHeaderChar + StartHeaderNumber.ToString() +
                        ":" + EndHeaderChar + EndHeaderNumber.ToString()]
                        .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                    ws.View.FreezePanes(2, 1);
                    #endregion
                    #region Set values for cells    

                    foreach (var data in totalModel)
                    {

                        ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = ++count;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.productName;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.Price);
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = data.quality;
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.discount);
                        ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", data.finalAmount);

                        StartHeaderChar = 'A';
                    }
                    ws.Cells["" + ('B') + (modelCount + 2)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", total);
                    ws.Cells["" + ('E') + (modelCount + 2)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", discount);
                    ws.Cells["" + ('F') + (modelCount + 2)].Value = string.Format(CultureInfo.InvariantCulture, "{0:0,0}", final);
                    #endregion

                    //Set style for excel
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();
                    ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                    package.SaveAs(ms);
                    ms.Seek(0, SeekOrigin.Begin);
                    var fileDownloadName = "ChiTietHoaDon_" + storeName + "_TổngQuanNgày.xlsx";
                    var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    return this.File(ms, contentType, fileDownloadName);
                }
                #endregion
            }
        }
        public async Task<ActionResult> GetCustomerStoreData(JQueryDataTableParamModel param, int Id)
        {
            var customerStoreApi = new CustomerStoreReportMappingApi();
            var customerStores = customerStoreApi.GetCustomerStoreByCustomerId(Id);

            if (!string.IsNullOrEmpty(param.sSearch))
            {
                customerStores = customerStores.Where(a =>
                        a.Store.Name.Contains(param.sSearch));
            }
            try
            {
                if (customerStores.Count() > 0)
                {
                    int count = param.iDisplayStart + 1;
                    var rs = (await customerStores
                        .OrderByDescending(q => q.LastVisitDate)
                        .Skip(param.iDisplayStart)
                        .Take(param.iDisplayLength)
                        .ToListAsync())
                        .Select(q => new IConvertible[]
                            {
                                count++,
                                q.Store.Name,
                                q.TotalOrder.GetValueOrDefault().ToString("#,##"),
                                q.TotalAmount.GetValueOrDefault().ToString("#,##"),
                                Math.Round(q.AverageAmount.GetValueOrDefault(),2).ToString("#,##") ,
                                //Math.Round(q.Frequency.GetValueOrDefault(),2).ToString("#,##"),
                                q.LastVisitDate.ToString("dd/MM/yyyy")
                            });


                    var totalRecords = await customerStores.CountAsync();

                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = totalRecords,
                        iTotalDisplayRecords = customerStores.Count(),
                        aaData = rs
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var rs = customerStores.ToList().Select(q => new IConvertible[]
                            {
                                q.Store.Name,
                                q.TotalOrder,
                                q.TotalAmount,
                                Math.Round(q.AverageAmount.GetValueOrDefault(),2) ,
                                //Math.Round(q.Frequency.GetValueOrDefault(),2),
                                q.LastVisitDate.ToString("dd/MM/yyyy")
                            }).ToList();
                    return Json(new
                    {
                        sEcho = param.sEcho,
                        iTotalRecords = 0,
                        iTotalDisplayRecords = 0,
                        aaData = rs
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = Resources.Message_VI_Resources.ErrorOccured }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetCustomerStoreHistoryData(int brandId, int customerId, string startTime, string endTime)
        {
            var orderApi = new OrderApi();
            var sTime = startTime.ToDateTime().GetStartOfDate();
            var eTime = endTime.ToDateTime().GetEndOfDate();
            var customerStores = orderApi.GetAllFinishedOrdersByDateAndCustomer(sTime, eTime, brandId, customerId);

            try
            {
                if (customerStores.Count() > 0)
                {

                    int count = 1;
                    int totalDays = (eTime - sTime).Days + 1;

                    var groupedCustomerStores = customerStores.GroupBy(q => q.StoreID);

                    var rs = (await groupedCustomerStores
                        .ToListAsync())
                        .Select(q => new IConvertible[]
                        {
                            count++,
                            q.FirstOrDefault().Store.Name,
                            q.Count(),
                            q.Sum(a => a.TotalAmount),
                            totalDays,
                        });

                    var totalRecords = await groupedCustomerStores.CountAsync();

                    return Json(new
                    {
                        success = true,
                        data = rs
                    }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new
                    {
                        success = true,
                        data = new List<string>()
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = Resources.Message_VI_Resources.ErrorOccured }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<ActionResult> GetCustomerAccountData(JQueryDataTableParamModel param, int Id)
        {
            var accountApi = new AccountApi();
            var customerAccounts = accountApi.GetAccountByCusId(Id);

            try
            {
                int count = 1;
                var rs = (await customerAccounts
                    .Where(a => string.IsNullOrEmpty(param.sSearch) ||
                        (!string.IsNullOrEmpty(param.sSearch)
                        && a.AccountName.Contains(param.sSearch.Trim())))
                    .OrderBy(q => q.AccountName)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToListAsync())
                    .Select(q => new IConvertible[]
                        {
                            count++,
                            q.AccountCode,
                            q.AccountName,
                            q.AccountID,
                        });


                var totalRecords = customerAccounts.CountAsync();

                return Json(new
                {
                    sEcho = param.sEcho,
                    iTotalRecords = totalRecords,
                    iTotalDisplayRecords = customerAccounts.Count(),
                    aaData = rs
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = Resources.Message_VI_Resources.ErrorOccured }, JsonRequestBehavior.AllowGet);
            }
        }

        //private string GetCurrencyById(int id, List<CurrencyViewModel> listCurrency)
        //{
        //    foreach (var item in listCurrency)
        //    {
        //        if (id == item.ID)
        //        {
        //            return item.Name;
        //        }
        //    }
        //    return null;
        //}

        public async Task<ActionResult> Edit(int id)
        {
            var customerApi = new CustomerApi();
            var entity = customerApi.Get(id);
            var model = this.Mapper.Map<CustomerEditViewModel>(entity);
            if (model.Email != null) model.Email = model.Email.Trim();
            await PrepareEditCustomer(model);
            return this.View(model);
        }

        //public async void PrepareEditCustomer(CustomerEditViewModel model)
        //{
        //    var customerTypeAPI = new CustomerTypeApi();
        //    //var typeList = await customerTypeAPI.();
        //}

        [HttpPost]
        public ActionResult Edit(CustomerViewModel model)
        {
            try
            {
                var customerApi = new CustomerApi();
                // Cho CustomerTypeId = 1 de khi Edit xong se load len trang Index
                //model.CustomerTypeId = 1;
                customerApi.UpdateCustomer(model);
                return Json(new { success = true, message = Resources.Message_VI_Resources.EditCustomerSuccessfully });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = Resources.Message_VI_Resources.EditCustomerFailed });
            }
        }
        public async Task PrepareEditCustomer(CustomerEditViewModel model)
        {
            var customerTypeAPI = new CustomerTypeApi();

            var typeList = await customerTypeAPI.GetActiveAsync();
            model.AvailableType = typeList.Select(q => new SelectListItem()
            {
                Selected = model.Type.Equals(q.ID),
                Text = q.CustomerType1,
                Value = q.ID.ToString(),
            });

            model.AvailableGender = new List<SelectListItem>();

            model.AvailableGender.Add(new SelectListItem()
            {
                Text = "Nam",
                Value = "true",
            });

            model.AvailableGender.Add(new SelectListItem()
            {
                Text = "Nữ",
                Value = "false",
            });
        }

        [HttpPost]
        public async Task<JsonResult> DeleteFilter(int id)
        {
            var customerFilterApi = new CustomerFilterApi();

            if (customerFilterApi.Get(id) == null)
            {
                return Json(new { success = false, message = Resources.Message_VI_Resources.DeleteFilterFailed });
            }

            try
            {
                await customerFilterApi.DeleteAsync(id);
                return Json(new { success = true, message = Resources.Message_VI_Resources.DeleteFilterSuccessfully });
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public async Task<JsonResult> DeleteCustomer(int id)
        {
            var customerApi = new CustomerApi();

            if (customerApi.Get(id) == null)
            {
                return Json(new { success = false, message = Resources.Message_VI_Resources.ErrorOccured });
            }

            try
            {
                //Customer không có attribute active, hoãn lại 
                await customerApi.DeactivateAsync(id);
                //customerApi.Deactivate(id);
                return Json(new { success = true, message = Resources.Message_VI_Resources.DeleteCustomerSuccessfully });
            }
            catch
            {
                return Json(new { success = false, message = Resources.Message_VI_Resources.ErrorOccured }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CreateAccount(CustomerViewModel model)
        {
            try
            {
                var customerApi = new CustomerApi();
                //customerApi.CreateAccount(model);
                return Json(new { success = true, msg = Resources.Message_VI_Resources.CreateAccountSuccessfully }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception e)
            {
                return Json(new { success = false, msg = Resources.Message_VI_Resources.CreateAccountFailed }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult CustomerFilter()
        {
            return this.View();
        }
        public JsonResult LoadAllCustomerFilter(JQueryDataTableParamModel param, int brandId)
        {
            int count = 0;
            IEnumerable<IConvertible[]> rs = null;
            int totalRecords;
            var customerFilterApi = new CustomerFilterApi();
            var customerFilters = customerFilterApi.GetAllFilter(brandId).ToList();

            var storeApi = new StoreApi();
            var customerTypeApi = new CustomerTypeApi();

            count = param.iDisplayStart + 1;


            //var searchList = customerFilters.Where(a => string.IsNullOrEmpty(param.sSearch) ||
            //            (!string.IsNullOrEmpty(param.sSearch)
            //            && a.Name.ToLower().Contains(param.sSearch.ToLower())));

            //rs = rs = (await searchList
            //        .OrderByDescending(a => a.ID)
            //        .Skip(param.iDisplayStart)
            //        .Take(param.iDisplayLength)
            //        .ToListAsync())

            rs = customerFilters
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        a.Name,
                        a.IsEnableAge == true ? ((a.AgeFrom == a.AgeTo)? a.AgeFrom.ToString(): (a.AgeFrom +" - "+ a.AgeTo)) : "Tất cả",
                        a.IsEnableBirthday == true ? (
                            a.BirthdayMonth == null  ?  (String.Format("{0:dd/MM}", a.BirthdayFrom) +" - "+String.Format("{0:dd/MM}", a.BirthdayTo)) : ("Tháng " + a.BirthdayMonth)) : "Tất cả",
                        //a.BirthdayMonth == null ? ("Từ " + a.BirthdayFrom.Value.ToString +" - "+ " đến tháng " + a.BirthdayTo.Value.Month) : ("Tháng " + a.BirthdayMonth)) : "Tất cả",
                        a.IsEnableVisitedTimes == true ? ((a.VisitedTimesFrom == a.VisitedTimesTo)? a.VisitedTimesFrom.ToString(): (a.VisitedTimesFrom +" - "+ a.VisitedTimesTo)) : "Tất cả",
                        a.IsEnableGender == true ? (a.Gender == true ? "Nam":"Nữ") : "Tất cả",
                        a.ID
                        });

            totalRecords = customerFilters.Count();

            //totalRecords = await customerFilters.CountAsync();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }
        public async Task<ActionResult> CreateCustomerFilter()
        {
            var model = new CustomerFilterEditViewModel();
            await PrepareCreateCustomerFilter(model);
            return this.View(model);
        }
        public async Task PrepareCreateCustomerFilter(CustomerFilterEditViewModel model)
        {
            var customerTypeAPI = new CustomerTypeApi();

            var typeList = await customerTypeAPI.GetActiveAsync();
            model.AvailableType = typeList.Select(q => new SelectListItem()
            {
                Selected = false,
                Text = q.CustomerType1,
                Value = q.ID.ToString(),
            });

            model.AvailableGender = new List<SelectListItem>();

            model.AvailableGender.Add(new SelectListItem()
            {
                Text = "Nam",
                Value = "true",
            });

            model.AvailableGender.Add(new SelectListItem()
            {
                Text = "Nữ",
                Value = "false",
            });

            model.AvailableBirthdayOptions = new List<SelectListItem>();
            model.AvailableBirthdayOptions.Add(new SelectListItem()
            {
                Text = "1 Tháng Trong Năm",
                Value = "1",
            });
            model.AvailableBirthdayOptions.Add(new SelectListItem()
            {
                Text = "Tháng Này",
                Value = "-1",
            });
            model.AvailableBirthdayOptions.Add(new SelectListItem()
            {
                Text = "Trong Khoảng",
                Value = "2",
            });
        }
        [HttpPost]
        public async Task<JsonResult> CreateCustomerFilter(CustomerFilterEditViewModel model)
        {

            var customerFilterApi = new CustomerFilterApi();


            //else if (model.BirthdayOption == 2)
            //    if (model.IsEnableGender != true)
            //{
            //    model.Gender = null;
            //}
            try
            {
                if (model.IsEnableBirthday == true)
                {
                    if (model.BirthdayOption == (int)BirthdayOptionFilterEnum.BirthdayMonth || model.BirthdayOption == (int)BirthdayOptionFilterEnum.ThisMonth)
                    {

                        int month = model.BirthdayMonth.Value;
                        model.BirthdayFrom = new DateTime(2000, month, 1);
                        model.BirthdayTo = new DateTime(2000, month, DateTime.DaysInMonth(2000, month));
                    }
                    else if (model.BirthdayOption == (int)BirthdayOptionFilterEnum.BirthdayRange)
                    {
                        model.BirthdayMonth = null;
                    }
                }
                else
                {
                    model.BirthdayMonth = null;
                    model.BirthdayFrom = null;
                    model.BirthdayTo = null;
                }



                await customerFilterApi.CreateAsync(model);
            }
            catch
            {
                return Json(new { success = false, message = "Cập nhật thất bại, xin vui lòng thử lại." });
            }
            return Json(new { success = true, message = "Cập nhật thành công !" });
        }



        public ActionResult FilterResult(int? id)
        {
            ViewBag.filterID = id.Value;
            return this.View();
        }
        public JsonResult LoadFilterResult(JQueryDataTableParamModel param, int? filterID, int brandId)
        {
            int count = 0;

            var customerApi = new CustomerApi();
            var customerFilterApi = new CustomerFilterApi();
            var filter = customerFilterApi.BaseService.Get(filterID.Value);
            var customers = customerApi.GetCustomersByFilter(filter, brandId);

            count = param.iDisplayStart + 1;
            if (!string.IsNullOrEmpty(param.sSearch))
            {
                customers = customers.Where(a => a.Name.Contains(param.sSearch));
            }

            var rs = customers
                      .Skip(param.iDisplayStart)
                      .Take(param.iDisplayLength)
                      .ToList()
                      .Select(a => new IConvertible[]
                          {
                        count++,
                        string.IsNullOrEmpty(a.Name) ? Properties.Resources.UNDEFINE_VN : a.Name,
                        string.IsNullOrEmpty(a.Email) ? Properties.Resources.UNDEFINE_VN : a.Email,
                        string.IsNullOrEmpty(a.Phone) ? Properties.Resources.UNDEFINE_VN : a.Phone,
                          });
            var totalRecords = customers.Count();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = customers.Count(),
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }


        public ActionResult RecentCustomer()
        {
            return View();
        }

        public async Task<JsonResult> GetListRecentCustomer(JQueryDataTableParamModel param, int? storeId, int? customerTypeId, DateTime? startDate, DateTime? endDate, int brandID)
        {
            int count = 0;
            IEnumerable<IConvertible[]> rs = null;
            int totalRecords;

            var customerApi = new CustomerApi();

            var customers = customerApi.GetCustomersByDateRange(startDate.Value, endDate.Value, brandID);


            //set brain id = 1 to view page

            if (storeId.HasValue && storeId.Value != -1)
            {
                customers = customerApi.GetCustomersByDateRangeAndStoreId(storeId.Value, startDate.Value, endDate.Value, brandID);
            }

            if (customerTypeId.HasValue && customerTypeId.Value != -1)
            {
                customers = customers.Where(c => c.Type == customerTypeId.Value);
            }

            count = param.iDisplayStart + 1;

            rs = (await customers.Where(a => string.IsNullOrEmpty(param.sSearch) ||
                        (!string.IsNullOrEmpty(param.sSearch)
                        && a.Name.ToLower().Contains(param.sSearch.ToLower())))
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToListAsync())
                    .Select(a => new IConvertible[]
                        {
                        count++,
                        string.IsNullOrEmpty(a.Name) ? "Không xác định" : a.Name,
                        a.Gender.HasValue ? a.Gender == true ? "Nam" : "Nữ" : "Không xác định",
                        string.IsNullOrEmpty(a.Phone) ? "Không xác định" : a.Phone,
                        a.CustomerID
                        });

            totalRecords = await customers.CountAsync();


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = customers.Count(),
                aaData = rs
            }, JsonRequestBehavior.AllowGet);

        }


        public async Task<ActionResult> EditCustomerFilter(int id)
        {
            var customerFilterApi = new CustomerFilterApi();
            var filter = customerFilterApi.Get(id);
            var model = this.Mapper.Map<CustomerFilterEditViewModel>(filter);
            await PrepareCreateCustomerFilter(model);
            return this.View(model);
        }
        [HttpPost]
        public async Task<JsonResult> EditCustomerFilter(CustomerFilterEditViewModel model)
        {
            var customerFilterApi = new CustomerFilterApi();
            model.Active = true;
            if (model.IsEnableBirthday == true)
            {
                if (model.BirthdayOption == 1)
                {
                    int month = model.BirthdayMonth.Value;
                    model.BirthdayFrom = new DateTime(2000, month, 1);
                    model.BirthdayTo = new DateTime(2000, month, DateTime.DaysInMonth(2000, month));
                }
                else if (model.BirthdayOption == 2)
                {
                    model.BirthdayMonth = null;
                }
            }
            try
            {
                await customerFilterApi.UpdateCustomerFilter(model);
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Cập nhật thất bại, xin vui lòng thử lại." });
            }

            return Json(new { success = true, message = "Cập nhật thành công !" });
        }
        [HttpPost]
        public JsonResult StoreOverview(int Id)
        {
            var customerStoreApi = new CustomerStoreReportMappingApi();

            var customerStores = customerStoreApi.GetCustomerStoreByCustomerId(Id);

            try
            {
                int? tOrder;
                double? tAmount, tFrequency;
                if (customerStores.Sum(q => q.TotalOrder) != null)
                {
                    tOrder = customerStores.Sum(q => q.TotalOrder);
                }
                else
                {
                    tOrder = 0;
                }
                if (customerStores.Sum(q => q.TotalAmount) != null)
                {
                    tAmount = customerStores.Sum(q => q.TotalAmount);
                }
                else
                {
                    tAmount = 0;
                }
                if (customerStores.Sum(q => q.Frequency) != null)
                {
                    tFrequency = Math.Round(customerStores.Sum(q => q.Frequency).Value, 2);
                }
                else
                {
                    tFrequency = 0;
                }

                string totalOrder = tOrder.GetValueOrDefault().ToString("#,##");
                string totalAmount = tAmount.GetValueOrDefault().ToString("#,##");
                string totalFrequency = tFrequency.GetValueOrDefault().ToString("#,##");

                return Json(new
                {
                    totalOrder,
                    totalAmount,
                    totalFrequency,
                });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }
        [HttpPost]
        public JsonResult TransactionOverview(int Id)
        {
            var accountApi = new AccountApi();
            var accounts = accountApi.GetAccountByCusId(Id).Where(q => q.Active == true);
            var transactionApi = new TransactionApi();
            var transactions = transactionApi.GetTransactionByCustomerId(Id);
            var AllIncrease = transactions.Where(q => q.IsIncreaseTransaction == true);
            var AllDecrease = transactions.Where(q => q.IsIncreaseTransaction == false);
            try
            {
                var tIncrease = AllIncrease.Count();
                var tDecrease = AllDecrease.Count();
                decimal? tBalance = 0, tProduct = 0, tPoint = 0;
                tBalance = accounts.Where(q => q.Type.HasValue && q.Type.Value == (int)AccountTypeEnum.CreditAccount).Sum(q => q.Balance);
                tProduct = accounts.Where(q => q.Type.HasValue && q.Type.Value == (int)AccountTypeEnum.GiftAccount).Sum(q => q.Balance);
                tPoint = accounts.Where(q => q.Type.HasValue && q.Type.Value == (int)AccountTypeEnum.PointAccount).Sum(q => q.Balance);
                string totalIncrease = tIncrease.ToString("#,##");
                string totalDecrease = tDecrease.ToString("#,##");
                string totalBalance = tBalance.GetValueOrDefault().ToString("#,##");
                string totalProduct = tProduct.GetValueOrDefault().ToString("#");
                string totalPoint = tPoint.GetValueOrDefault().ToString("#");
                if (totalBalance.Count() == 0)
                {
                    totalBalance = "0";
                }
                if (totalProduct.Count() == 0)
                {
                    totalProduct = "0";
                }
                if (totalPoint.Count() == 0)
                {
                    totalPoint = "0";
                }
                return Json(new
                {
                    totalIncrease,
                    totalDecrease,
                    totalBalance,
                    totalProduct,
                    totalPoint,
                });
            }
            catch (Exception)
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Export customer list to CSV file which is used for Mailchimp
        /// </summary>
        /// <param name="searchVal">Value of search textbox</param>
        /// <param name="filterId">Advance filter</param>
        /// <param name="customerTypeId">Customer type filter</param>
        /// <param name="brandId">ID of brand</param>
        public ActionResult ExportCustomerList(string searchVal, int? filterId, int? customerTypeId, int brandId)
        {
            // Declare variables
            int count = 1;

            // Declare APIs
            var customerApi = new CustomerApi();
            var customerFilterApi = new CustomerFilterApi();


            #region CSV body
            // Get filter by its ID
            var filter = customerFilterApi.BaseService.Get(filterId.Value);

            // Get list customer by brand's ID and filter
            var customers = customerApi.GetCustomersByFilter(filter, brandId);

            // Case filter is set
            if (customerTypeId.Value != -1)
            {
                customers = customers.Where(c => c.CustomerTypeId == customerTypeId.Value);
            }

            // Case search textbox has value
            if (!string.IsNullOrEmpty(searchVal))
            {
                customers = customers.Where(a =>
                       (a.Name.Contains(searchVal)));
            }

            #endregion


            #region Export to Excel
            MemoryStream ms = new MemoryStream();
            using (ExcelPackage package = new ExcelPackage(ms))
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("InStockInventory");
                char StartHeaderChar = 'A';
                int StartHeaderNumber = 1;
                #region Headers
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "STT";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Tên Khách hàng";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Email";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Điện thoại";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Giới tính";
                ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = "Sinh nhật";
                ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = "Địa chỉ";

                var EndHeaderChar = StartHeaderChar;
                var EndHeaderNumber = StartHeaderNumber;
                StartHeaderChar = 'A';
                StartHeaderNumber = 1;
                #endregion
                #region Set style for rows and columns
                ws.Cells["" + StartHeaderChar + StartHeaderNumber +
                    ":" + EndHeaderChar + EndHeaderNumber].Style.Font.Bold = true;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber +
                    ":" + EndHeaderChar + EndHeaderNumber].AutoFitColumns();
                ws.Cells["" + StartHeaderChar + StartHeaderNumber +
                    ":" + EndHeaderChar + EndHeaderNumber]
                    .Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                ws.Cells["" + StartHeaderChar + StartHeaderNumber +
                    ":" + EndHeaderChar + EndHeaderNumber]
                    .Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.GreenYellow);
                ws.View.FreezePanes(2, 1);
                #endregion
                #region Set values for cells                
                foreach (var data in customers)
                {
                    ws.Cells["" + (StartHeaderChar++) + (++StartHeaderNumber)].Value = count++;
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = (data.Name == null ? "" : data.Name);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = (data.Email == null ? "" : data.Email);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = (data.Phone == null ? "" : data.Phone);
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = (data.Gender == null ? "" : (data.Gender == true ? "Nam" : "Nữ"));
                    ws.Cells["" + (StartHeaderChar++) + (StartHeaderNumber)].Value = (data.BirthDay.HasValue ? (data.BirthDay.Value.ToString("dd'/'MM'/'yyyy")) : "");
                    ws.Cells["" + (StartHeaderChar) + (StartHeaderNumber)].Value = (data.Address == null ? "" : data.Address);


                    StartHeaderChar = 'A';
                }
                string brandName;
                var brandApi = new BrandApi();
                brandName = brandApi.GetBrandById(brandId).BrandName;
                string fileName = "Danh sách Khách hàng.xlsx";
                #endregion

                //Set style for excel
                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                ws.Cells[ws.Dimension.Address].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                ws.Cells[ws.Dimension.Address].Style.Border.Right.Style = ExcelBorderStyle.Thin;

                package.SaveAs(ms);
                ms.Seek(0, SeekOrigin.Begin);
                var contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                //Code cũ
                //    // Response 
                //    byte[] bytes = Encoding.UTF8.GetBytes(sb.ToString());
                //var fileDownloadName = Properties.Resources.CUSTOMER_LIST + Properties.Resources.UNDERSCORE + DateTime.Now.ToString(Properties.Resources.DATE_TIME_FORMAT) + ".csv";
                //var contentType = "text/csv";

                return File(ms, contentType, fileName);
                #endregion
            }
        }

    }
}


