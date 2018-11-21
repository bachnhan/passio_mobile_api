using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyAdmin.Manage.Areas.MembershipCard.Controllers
{
    public class MembershipCardTypeController : Controller
    {
        // GET: MembershipCard/MembershipCardType
        public ActionResult Index()
        {
            return View();
        }

        public JsonResult GetMembershipTypes(int brandId)
        {
            var memCardTypeApi = new MembershipCardTypeApi();

            var count = 1;
            var typeList = memCardTypeApi.GetAllMembershipCardTypeByBrand(brandId)
                    .Select(q => new IConvertible[] {
                        count++,
                        q.TypeName,
                        q.AppendCode,
                        q.Id,
                        q.Active,
                    });

            return Json(new { typeList = typeList }, JsonRequestBehavior.AllowGet);
        }


        // Use for filter Membership card type by type
        // param: isActive: for load active Membership card type
        //        allData: for load all Membership card type
        public JsonResult FilterMembershipTypesByStatus(int brandId, bool isActive, bool allData)
        {
            try
            {
                var memCardTypeApi = new MembershipCardTypeApi();

                if (allData)
                {
                    var count = 1;
                    var typeList = memCardTypeApi.GetAllMembershipCardTypeByBrand(brandId)
                            .Select(q => new IConvertible[] {
                        count++,
                        q.TypeName,
                        q.AppendCode,
                        q.Id,
                        q.Active,
                            });
                    return Json(new { typeList = typeList, success = true }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    var count = 1;
                    var typeList = memCardTypeApi.GetAllMembershipCardTypeByBrand(brandId)
                            .Where(mc => mc.Active == isActive)
                            .Select(q => new IConvertible[] {
                        count++,
                        q.TypeName,
                        q.AppendCode,
                        q.Id,
                        q.Active,
                            });
                    return Json(new { typeList = typeList, success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e) { return Json(new { success = false }, JsonRequestBehavior.AllowGet); }
        }

        public async Task<JsonResult> CreateMembershipCardType(int brandId, string typeName, string appendCode)
        {
            try
            {
                var memCardTypeApi = new MembershipCardTypeApi();
                var cardTypeModel = new MembershipCardTypeViewModel();

                var cardType = memCardTypeApi.GetMembershipCardTypeByAppendCode(appendCode);
                if (cardType == null)
                {
                    cardTypeModel.TypeName = typeName;
                    cardTypeModel.AppendCode = appendCode;
                    cardTypeModel.BrandId = brandId;
                    cardTypeModel.Active = true;
                }
                else
                {
                    return Json(new { success = false, message = "Mã loại thẻ đã tồn tại, xin hãy nhập mã khác." });
                }

                await memCardTypeApi.CreateMembershipCardTypeAsync(cardTypeModel);
                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." });
            }
        }

        public async Task<JsonResult> EditMembershipCardType(int brandId, int cardTypeId, string newTypeName, string newCode)
        {
            try
            {
                var memCardTypeApi = new MembershipCardTypeApi();

                var oldCard = memCardTypeApi.GetMembershipCardTypeById(cardTypeId);

                // AppendCode có thể Null, so sánh Equal với Null sẽ bị lỗi
                oldCard.AppendCode = (oldCard.AppendCode != null) ? oldCard.AppendCode : "";
                if (oldCard.AppendCode.Equals(newCode))
                {
                    oldCard.TypeName = newTypeName;
                    await memCardTypeApi.UpdateMembershipCardTypeAsync(oldCard);
                }
                else
                {
                    var checkCard = memCardTypeApi.GetMembershipCardTypeByAppendCode(newCode);
                    if (checkCard == null)
                    {
                        oldCard.AppendCode = newCode;
                        oldCard.TypeName = newTypeName;
                        await memCardTypeApi.UpdateMembershipCardTypeAsync(oldCard);
                    }
                    else
                    {
                        return Json(new { success = false, message = "Mã loại thẻ đã tồn tại, xin hãy nhập mã khác." });
                    }
                }

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." });
            }
        }

        public async Task<JsonResult> ChangeMembershipCardTypeActivation(int cardTypeId, bool active)
        {
            try
            {
                var memCardTypeApi = new MembershipCardTypeApi();
                await memCardTypeApi.ChangeMembershipCardTypeActivation(cardTypeId, active);

                return Json(new { success = true });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "Có lỗi xảy ra, vui lòng thử lại." });
            }
        }

    }
}