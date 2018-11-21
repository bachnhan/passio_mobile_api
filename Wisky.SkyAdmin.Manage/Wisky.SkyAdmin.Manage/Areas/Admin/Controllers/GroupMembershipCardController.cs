using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class GroupMembershipCardController : DomainBasedController
    {
        // GET: Admin/GroupMembershipCard

        #region Index
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult LoadGroupMembership(JQueryDataTableParamModel param, int brandId)
        {
            var groupCardApi = new GroupMembershipCardApi();
            var result = groupCardApi.GetAllActiveGroupByBrandId(brandId);
            var count = 0;
            var rs = result
                .ToList()
                .Select(a => new IConvertible[]
                {
                    ++count,
                    a.GroupCode,
                    a.GroupName,
                    a.GroudId,
                });

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = result.Count(),
                iTotalDisplayRecords = result.Count(),
                aaData = rs,
            }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public async Task<JsonResult> Create(string code, string name, int brandId)
        {
            try
            {
                var groupApi = new GroupMembershipCardApi();
                GroupMembershipCardViewModel model = new GroupMembershipCardViewModel();
                if (groupApi.GetGroupMembershipCardByCode(code) == null)
                {
                    model.GroupCode = code;
                    model.GroupName = name;
                    model.BrandId = brandId;
                    model.Active = true;
                    await groupApi.CreateAsync(model);
                    return Json(new { success = true, message = "Tạo nhóm thành công." });
                }
                else
                {
                    return Json(new { success = false, message = "Mã nhóm bị trùng, xin chọn tạo lại bằng mã khác." });
                }
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Tạo nhóm thất bại." });
                throw e;
            }

        }
        public async Task<JsonResult> Edit(int groupId, string name)
        {
            try
            {
                var groupApi = new GroupMembershipCardApi();
                var group = groupApi.Get(groupId);
                GroupMembershipCardViewModel model = new GroupMembershipCardViewModel();
                model.GroudId = groupId;
                model.GroupName = name;
                model.GroupCode = group.GroupCode;
                model.BrandId = group.BrandId;
                model.Active = group.Active;
                await groupApi.EditAsync(groupId, model);
                return Json(new { success = true, message = "Cập nhập nhóm thành công." });
            }
            catch (Exception e)
            {
                return Json(new { success = false, message = "Cập nhập nhóm thất bại." });
                throw e;
            }
        }
        #endregion

        #region MembershipAssign

        public ActionResult MembershipAssign(string groupCode)
        {
            var groupMemCardApi = new GroupMembershipCardApi();

            var group = groupMemCardApi.GetGroupMembershipCardByCode(groupCode);
            ViewBag.GroupCode = groupCode;
            ViewBag.GroupName = group.GroupName;

            return View();
        }

        public JsonResult LoadMembershipCards(JQueryDataTableParamModel param, int brandId, string groupCode, int filterCard, int filterStatus, int filterGroup)
        {
            var membershipCardApi = new MembershipCardApi();
            var groupMemCardApi = new GroupMembershipCardApi();
            var accountApi = new AccountApi();

            var group = groupMemCardApi.GetGroupMembershipCardByCode(groupCode);
            var cardList = membershipCardApi.GetMembershipCardsInGroupAndNotInGroup(brandId, group.GroudId);
            var totalRecords = cardList.Count();

            // filter theo loại thẻ
            if (filterCard >= 0)
            {
                cardList = cardList.Join(
                        accountApi.GetAccountByBrandId(brandId),
                        p => p.Id,
                        q => q.MembershipCardId,
                        (p, q) => p
                    ).Where(q => q.MembershipTypeId == filterCard);
            }

            // filter theo tình trạng
            if (filterStatus >= 0)
            {
                if (filterStatus == (int)MembershipStatusEnum.Active)
                {
                    // Kích hoạt
                    cardList = cardList.Where(q => q.Status == (int)MembershipStatusEnum.Active);
                }
                else if (filterStatus == (int)MembershipStatusEnum.Inactive)
                {
                    // Chưa kích hoạt
                    cardList = cardList.Where(q => q.Status == (int)MembershipStatusEnum.Inactive);
                }
                else if (filterStatus == (int)MembershipStatusEnum.Suspensed)
                {
                    // Tạm dừng
                    cardList = cardList.Where(q => q.Status == (int)MembershipStatusEnum.Suspensed);
                }
            }

            if (filterGroup >= 0)
            {
                if (filterGroup == 0)
                {
                    // Chưa thuộc nhóm
                    cardList = cardList.Where(q => q.GroupId == null);
                }
                else if (filterGroup == 1)
                {
                    // Thuộc nhóm
                    cardList = cardList.Where(q => q.GroupId != null);
                }
            }

            var searchList = cardList.Where(a => string.IsNullOrEmpty(param.sSearch)
                        || (a.Customer != null && a.Customer.Name.ToLower().Contains(param.sSearch.ToLower())));

            int count = param.iDisplayStart;
            var rs = searchList
                    .OrderByDescending(q => q.Customer != null ? q.Customer.Name : "")
                    .ThenByDescending(q => q.CreatedTime)
                    .Skip(param.iDisplayStart)
                    .Take(param.iDisplayLength)
                    .ToList().Select(q => new IConvertible[]
                    {
                        ++count,
                        q.Customer != null ? q.Customer.Name : "N/A",
                        q.MembershipCardCode,
                        q.CreatedTime.ToString("dd/MM/yyyy"),
                        q.Status,
                        q.GroupId.HasValue,
                        Utils.DisplayName((MembershipStatusEnum) q.Status),
                        q.MembershipCardCode
                    });
            var totalDisplayRecords = searchList.Count();

            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = totalDisplayRecords,
                aaData = rs
            }, JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> ChangeGroupMembershipCard(string cardCode, string groupCode, bool setToGroup)
        {
            try
            {
                var memCardApi = new MembershipCardApi();
                var groupMemCardApi = new GroupMembershipCardApi();

                var group = groupMemCardApi.GetGroupMembershipCardByCode(groupCode);

                var memCard = memCardApi.GetMembershipCardByCode(cardCode);
                if (setToGroup)
                {
                    memCard.GroupId = group.GroudId;
                }
                else
                {
                    memCard.GroupId = null;
                }

                await memCardApi.UpdateMemberShipCardEntityAsync(memCard);

                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { success = false }, JsonRequestBehavior.AllowGet);
            }
        }

        //public async Task<JsonResult> ChangeGroupMembershipCardOnePage(string sSearch, int iDisplayLength, int iDisplayStart, int brandId, string groupCode, int filterCard, int filterStatus, int filterGroup, bool isActive)
        //{
        //    try
        //    {
        //        var membershipCardApi = new MembershipCardApi();
        //        var groupMemCardApi = new GroupMembershipCardApi();
        //        var accountApi = new AccountApi();

        //        var group = groupMemCardApi.GetGroupMembershipCardByCode(groupCode);
        //        var cardList = membershipCardApi.GetMembershipCardsInGroupAndNotInGroup(brandId, group.GroudId);
        //        var totalRecords = cardList.Count();

        //        // filter theo loại thẻ
        //        if (filterCard >= 0)
        //        {
        //            cardList = cardList.Join(
        //                    accountApi.GetAccountByBrandId(brandId).Where(a => a.AccountTypeId == filterCard),
        //                    p => p.Id,
        //                    q => q.MembershipCardId,
        //                    (p, q) => p
        //                );
        //        }

        //        // filter theo tình trạng
        //        if (filterStatus >= 0)
        //        {
        //            if (filterStatus == (int)MembershipStatusEnum.Active)
        //            {
        //                // Kích hoạt
        //                cardList = cardList.Where(q => q.Status == (int)MembershipStatusEnum.Active);
        //            }
        //            else if (filterStatus == (int)MembershipStatusEnum.Inactive)
        //            {
        //                // Chưa kích hoạt
        //                cardList = cardList.Where(q => q.Status == (int)MembershipStatusEnum.Inactive);
        //            }
        //            else if (filterStatus == (int)MembershipStatusEnum.Suspensed)
        //            {
        //                // Tạm dừng
        //                cardList = cardList.Where(q => q.Status == (int)MembershipStatusEnum.Suspensed);
        //            }
        //        }

        //        if (filterGroup >= 0)
        //        {
        //            if (filterGroup == 0)
        //            {
        //                // Chưa thuộc nhóm
        //                cardList = cardList.Where(q => q.GroupId == null);
        //            }
        //            else if (filterGroup == 1)
        //            {
        //                // Thuộc nhóm
        //                cardList = cardList.Where(q => q.GroupId != null);
        //            }
        //        }

        //        var searchList = cardList.Where(a => string.IsNullOrEmpty(sSearch)
        //                    || (a.Customer != null && a.Customer.Name.ToLower().Contains(sSearch.ToLower())));
                
        //        var rs = searchList
        //                .OrderByDescending(q => q.Customer != null ? q.Customer.Name : "")
        //                .ThenByDescending(q => q.CreatedTime)
        //                .Skip(iDisplayStart)
        //                .Take(iDisplayLength)
        //                .Select(q => q.MembershipCardCode).ToList();
        //        int? groupId = null;
        //        if (isActive)
        //        {
        //            groupId = group.GroudId;
        //        }

        //        await membershipCardApi.UpdateGroupIdByMemberShipCardCodeListAsync(rs, groupId);
        //        return Json(new { success = true });
        //    }
        //    catch (Exception)
        //    {
        //        return Json(new { success = false });
        //    }

        //}

        public JsonResult GetMemberShipType(int brandId)
        {
            MembershipCardTypeApi api = new MembershipCardTypeApi();
            List<MembershipCardTypeViewModel> model = api.GetMembershipCardTypeByBrand(brandId).ToList();
            List<string> names = model.Select(q => q.TypeName).ToList();
            List<int> ids = model.Select(q => q.Id).ToList();
            return Json(new {
                MembershipType = ids,
                Name = names,
            });
        }

        #endregion
    }
}