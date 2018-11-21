using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Sdk;
using HmsService.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.MembershipCard.Controllers
{
    public class MembershipCardSampleController : DomainBasedController
    {
        // GET: MembershipCard/MembershipCardSample
        #region Index
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult LoadAllMembershipType(int brandID)
        {
            var api = new MembershipCardTypeApi();
            //get customer types

            //set brain id = 1 to view page
            var typeList = api.GetMembershipCardTypeByBrand(brandID).Select(a => new
            {
                MembershipType = a.Id,
                Name = a.TypeName
            }).ToList();
            return Json(typeList);
        }

        [HttpPost]
        public JsonResult LoadAllSampleCard(int brandID)
        {
            var productApi = new ProductApi();

            var productList = productApi.GetActiveProductsEntitybyBrandId(brandID).Where(p => p.ProductType == (int)ProductTypeEnum.CardPayment).Select(p => new
            {
                ProductCode = p.Code,
                ProductName = p.ProductName
            }).ToList();
            return Json(productList);
        }

        public JsonResult LoadMembershipCardSample(JQueryDataTableParamModel param, int membershipTypeId, int brandId)
        {
            int totalRecords = 0;
            int totalDisplayRecords = 0;
            int count = param.iDisplayStart + 1;
            var membershipCardApi = new MembershipCardApi();
            var rs = new Object();

            if (membershipTypeId != -1)
            {
                var card = membershipCardApi.GetMembershipCardSampleByBrandAndType(brandId, membershipTypeId).ToList();

                var searchList = card.Where(a => string.IsNullOrEmpty(param.sSearch)
                                || (!string.IsNullOrEmpty(param.sSearch) && a.MembershipCardCode.ToLower()
                                .Contains(param.sSearch.ToLower())));

                rs = searchList.OrderByDescending(a => a.CreatedTime)
                           .Skip(param.iDisplayStart)
                           .Take(param.iDisplayLength)
                           .Select(a => new object[]
                           {
                                       count++,
                                       string.IsNullOrEmpty(a.MembershipCardCode) ? "---" : a.MembershipCardCode,
                                       a.CreatedTime.ToString("dd/MM/yyyy"),
                                       a.MembershipTypeId,
                                       a.Id
                           }).ToList();

                totalRecords = card.Count();
                totalDisplayRecords = searchList.Count();
            }
            else
            {
                var card = membershipCardApi.GetMembershipCardSampleActiveByBrandId(brandId).ToList();

                var searchList = card.Where(a => string.IsNullOrEmpty(param.sSearch)
                                || (!string.IsNullOrEmpty(param.sSearch) && a.MembershipCardCode.ToLower()
                                .Contains(param.sSearch.ToLower())));

                rs = searchList.OrderByDescending(a => a.CreatedTime)
                           .Skip(param.iDisplayStart)
                           .Take(param.iDisplayLength)
                           .Select(a => new object[]
                           {
                                       count++,
                                       string.IsNullOrEmpty(a.MembershipCardCode) ? "---" : a.MembershipCardCode,
                                       a.CreatedTime.ToString("dd/MM/yyyy"),
                                       a.MembershipTypeId,
                                       a.Id
                           }).ToList();

                totalRecords = card.Count();
                totalDisplayRecords = searchList.Count();
            }


            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplayRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #region Create
        public ActionResult Create()
        {
            return View();
        }

        public async Task<JsonResult> CreateCardSampleWithAccounts(int brandId, string memCardCode, string createTime, int memTypeId, string listAccounts, string productCode)
        {
            try
            {
                int memCardId;
                #region Create Membership Card
                var membershipCardApi = new MembershipCardApi();
                var productApi = new ProductApi();
                var accountApi = new AccountApi();
                var accounts = JsonConvert.DeserializeObject<List<dynamic>>(listAccounts);
                MembershipCardViewModel cardModel = new MembershipCardViewModel();

                var memCard = membershipCardApi.GetMembershipCardByCode(memCardCode);
                var sampleCard = productApi.BaseService.FirstOrDefault(q=>q.Code.Equals(productCode));
                if (memCard == null)
                {
                    cardModel.MembershipCardCode = memCardCode;
                    cardModel.MembershipTypeId = memTypeId;
                    cardModel.CreatedTime = createTime.ToDateTime();
                    cardModel.BrandId = brandId;
                    cardModel.ProductCode = productCode;
                    cardModel.IsSample = true;
                    if (sampleCard!= null)
                    {
                        cardModel.InitialValue = sampleCard.Price;
                    } else
                    {
                        cardModel.InitialValue = 0;
                    }
                    memCardId = await membershipCardApi.CreateMembershipCardAsync(cardModel);
                }
                else
                {
                    return Json(new { success = false, message = "Mã thẻ đã tồn tại, xin nhập mã thẻ khác." });
                }

                #endregion

                #region Create Accounts

                foreach (var data in accounts)
                {
                    AccountViewModel newAccount = new AccountViewModel();
                    newAccount.AccountCode = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    newAccount.AccountName = data[1];
                    newAccount.Type = data[2];

                    string accStartDate = data[5];
                    if (!string.IsNullOrEmpty(accStartDate) && !accStartDate.Equals("N/A"))
                    {
                        newAccount.StartDate = accStartDate.ToDateTime();
                    }

                    string accFinishDate = data[7];
                    if (!string.IsNullOrEmpty(accFinishDate))
                    {
                        newAccount.FinishDate = accFinishDate.ToDateTime().GetEndOfDate();
                    }

                    newAccount.Balance = data[3];

                    string productCode2 = data[8];
                    if (data[2] == (int)AccountTypeEnum.GiftAccount && !string.IsNullOrWhiteSpace(productCode2))
                    {
                        newAccount.ProductCode = productCode2;
                    }


                    string level = data[9];
                    if (data[2] == (int)AccountTypeEnum.PointAccount && !string.IsNullOrWhiteSpace(level))
                    {
                        newAccount.Level_ = data[9];
                    }
                    else
                    {
                        newAccount.Level_ = 0;
                    }

                    newAccount.MembershipCardId = memCardId;
                    newAccount.BrandId = brandId;
                    newAccount.Active = true;

                    await accountApi.CreateAsync(newAccount);
                }
                #endregion
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        #region Edit
        public ActionResult Edit(int id, int brandId)
        {
            var membershipCardApi = new MembershipCardApi();
            var accountApi = new AccountApi();
            var productApi = new ProductApi();
            var card = membershipCardApi.GetMembershipCardById(id);
            var sampleCard= membershipCardApi.GetAllMembershipCardSampleByBrandId(brandId).FirstOrDefault(m=>m.Id==id);
            List<Account> accounts = accountApi.GetAllAccountsByMembershipId(id).ToList();
            int count = 0;
            var dataTable = accounts.Select(q => new IConvertible[] {
                                     ++count,
                                     q.AccountCode,
                                     q.AccountName,
                                     q.Type,
                                     q.Balance,
                                     q.ProductCode != null ? productApi.GetProductByCode(q.ProductCode).ProductName : "N/A",
                                     q.StartDate != null ? q.StartDate.Value.ToString("dd/MM/yyyy") : "N/A",
                                     q.Active,
                                     q.FinishDate != null ? q.FinishDate.Value.ToString("dd/MM/yyyy") : "",
                                     q.ProductCode,
                                     q.Level_,
                                     true // bool để phân biệt account đã tạo và chưa tạo
                            });
            var model = new MembershipCardEditViewModel3
            {
                membershipCard = card,
                accountList = accounts,
                dataTable = dataTable.ToList(),
                productCode = sampleCard.ProductCode
            };

            ViewBag.AccountTypes = accounts.Select(q => q.Type).ToList();
            var accountTypes = Enum.GetValues(typeof(AccountTypeEnum));
            var accountTypeList = new List<AccountTypeViewModel>();
            foreach (var type in accountTypes)
            {
                accountTypeList.Add(new AccountTypeViewModel
                {
                    Name = ((AccountTypeEnum)type).DisplayName(),
                    Value = (int)type
                });
            }
            ViewBag.AllAccountTypes = accountTypeList;
            return View(model);
        }

        public class MembershipCardEditViewModel2
        {
            public MembershipCardEditViewModel membershipCard { get; set; }

            public List<Account> accountList { get; set; }
            public List<IConvertible[]> dataTable { get; set; }
        }

        public class MembershipCardEditViewModel3
        {
            public MembershipCardEditViewModel membershipCard { get; set; }
            public string productCode { get; set; }
            public List<Account> accountList { get; set; }
            public List<IConvertible[]> dataTable { get; set; }
        }

        public class AccountTypeViewModel
        {
            public string Name { get; set; }
            public int Value { get; set; }
        }
        #endregion
    }
}