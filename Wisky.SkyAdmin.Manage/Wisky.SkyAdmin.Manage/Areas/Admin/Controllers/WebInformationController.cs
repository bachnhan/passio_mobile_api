using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using System;
using System.Linq;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using Wisky.SkyAdmin.Manage.Models;
using Wisky.SkyUp.Website.Models;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    [Authorize(Roles = Utils.AdminAuthorizeRoles)]
    public class WebInformationController : DomainBasedController
    {
        //
        // GET: /Admin/WebInformation/
        public ActionResult Index()
        {
            return View();
        }

        #region Gerenal Information
        public ActionResult GeneralInformation()
        {
            return View();
        }
        [HttpGet]
        public JsonResult LoadGeneralInfo(int storeId)
        {
            var storeInfo = new StoreApi().Get().Where(s => s.ID == storeId && s.isAvailable == true).FirstOrDefault();

            #region dynamic setting
            string title = null,
                slogan = null,
                description = null;

            var settingApi = new StoreWebSettingApi();


            var titleSetting = settingApi.Get().FirstOrDefault(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Title.ToString()));
            if (titleSetting != null)
            {
                title = titleSetting.Value;
            }

            var sloganSetting = settingApi.Get().FirstOrDefault(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Slogan.ToString()));
            if (sloganSetting != null)
            {
                slogan = sloganSetting.Value;
            }

            var descriptionSetting = settingApi.Get().FirstOrDefault(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Description.ToString()));
            if (descriptionSetting != null)
            {
                description = descriptionSetting.Value;
            }

            #endregion


            return Json(new
            {
                info = storeInfo,
                title,
                slogan,
                description,
                
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateGeneralInfo()
        {
            try
            {
                int storeId = this.CurrentStore.ID;
                #region update static info
                var name = Request.Params["name"];
                var address = Request.Params["address"];
                var phone = Request.Params["phone"];
                var fax = Request.Params["fax"];
                var email = Request.Params["email"];
                StoreViewModel model = new StoreApi().Get().Where(s => s.ID == storeId).FirstOrDefault();
                if (model!=null)
                {
                    model.Name = name;
                    model.Address = address;
                    model.Phone = phone;
                    model.Fax = fax;
                    model.Email = email;
                    new StoreApi().Edit(storeId, model);
                }
              
                #endregion

                #region update dynamic info
                var settingApi = new StoreWebSettingApi();
                var title = Request.Params["title"];
                var slogan = Request.Params["slogan"];
                var description = Request.Params["description"];
                //neu null tuc la admin quy định ko có mục setting đó
                if (title != null)
                {
                    var titleSetting = settingApi.Get().FirstOrDefault(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Title.ToString()));
                    titleSetting.Value = title;
                    settingApi.Edit(titleSetting.Id, titleSetting);
                }
                if (slogan != null)
                {
                    var sloganSetting = settingApi.Get().FirstOrDefault(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Slogan.ToString()));
                    sloganSetting.Value = slogan;
                    settingApi.Edit(sloganSetting.Id, sloganSetting);
                }
                if (description != null)
                {
                    var descriptionSetting = settingApi.Get().FirstOrDefault(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Description.ToString()));
                    descriptionSetting.Value = description;
                    settingApi.Edit(descriptionSetting.Id, descriptionSetting);
                }


                #endregion
                return Json(new
                {
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false
                }, JsonRequestBehavior.AllowGet);
            }

        }
        #endregion


        #region Social Network
        public ActionResult SocialNetwork()
        {
            return View();
        }

        [HttpGet]
        public JsonResult LoadSocialDetail()
        {
            try
            {
                var storeId = this.CurrentStore.ID;
                var api = new StoreWebSettingApi();
                string facebookLink = null,
                    youtubeLink = null,
                    twitterLink = null,
                    zaloLink = null;
              
                var facebookSetting = api.Get().Where(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Facebook.ToString())).FirstOrDefault();
                if (facebookSetting != null)
                {
                    facebookLink = facebookSetting.Value;
                }

                var youtubeSetting = api.Get().Where(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Youtube.ToString())).FirstOrDefault();
                if (youtubeSetting != null)
                {
                    youtubeLink = youtubeSetting.Value;
                }
                
                var zaloSetting = api.Get().Where(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Zalo.ToString())).FirstOrDefault();
                if (zaloSetting != null)
                {
                    zaloLink = zaloSetting.Value;
                }

                var twitterSetting = api.Get().Where(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Twitter.ToString())).FirstOrDefault();
                if (twitterSetting != null)
                {
                    twitterLink = twitterSetting.Value;
                }
                return Json(new
                {
                    facebook = facebookLink,
                    zalo = zaloLink,
                    youtube = youtubeLink,
                    twitter = twitterLink,
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                }, JsonRequestBehavior.AllowGet);

            }

        }

        [HttpPost]
        public JsonResult UpdateSocialInfo()
        {
            try
            {
                int storeId = this.CurrentStore.ID;
                var api = new StoreWebSettingApi();

                var facebook = Request.Params["facebook"];
                var zalo = Request.Params["zalo"];
                var youtube = Request.Params["youtube"];
                var twitter = Request.Params["twitter"];
                //neu null tuc la admin quy định ko có mục setting đó
                if (facebook!=null)
                {
                    var facebookSetting = api.Get().FirstOrDefault(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Facebook.ToString()));
                    facebookSetting.Value = facebook;
                    api.Edit(facebookSetting.Id, facebookSetting);
                }

                if (zalo != null)
                {
                    var zaloSetting = api.Get().FirstOrDefault(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Zalo.ToString()));
                    zaloSetting.Value = zalo;
                    api.Edit(zaloSetting.Id, zaloSetting);
                }

                if (youtube != null)
                {
                    var youtubeSetting = api.Get().FirstOrDefault(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Youtube.ToString()));
                    youtubeSetting.Value = youtube;
                    api.Edit(youtubeSetting.Id, youtubeSetting);
                }

                if (twitter != null)
                {
                    var twitterSetting = api.Get().FirstOrDefault(s => s.StoreId == storeId && s.Name.Equals(WebSetting.Twitter.ToString()));
                    twitterSetting.Value = twitter;
                    api.Edit(twitterSetting.Id, twitterSetting);
                }

                return Json(new
                {
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    success=false,
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion


        #region View Count

        public ActionResult ViewCount()
        {
            return View();
        }

        public JsonResult LoadViewCount()
        {
            try
            {
                int storeId = this.CurrentStore.ID;
                var viewCount = new ViewCounterApi().Get().FirstOrDefault(v => v.StoreId == storeId); 

                return Json(new
                {
                    info = viewCount,
                    success = true,
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new { 
                    success=false,
                },JsonRequestBehavior.AllowGet);
            }
        }

         [HttpPost]
        public JsonResult UpdateViewCount()
        {
            try
            {
                int storeId = this.CurrentStore.ID;
                var online = Int32.Parse(Request.Params["online"]);
                var today =  Int32.Parse(Request.Params["today"]);
                var thisMonth =  Int32.Parse(Request.Params["thisMonth"]);
                var thisWeek =  Int32.Parse(Request.Params["thisWeek"]);
                var total =  Int32.Parse(Request.Params["total"]);

               
                ViewCounterViewModel model = new ViewCounterApi().Get().FirstOrDefault(s => s.StoreId == storeId);
                // có rồi thì update
                if (model!=null)
                {
                    model.OnlineCount = online;
                    model.ThisMonthCount = thisMonth;
                    model.TodayCount = today;
                    model.ThisWeekCount = thisWeek;
                    model.TotalCount = total;
                    new ViewCounterApi().Edit(model.Id, model);
                }
                //chưa có thì tạo mới
                else
                {
                    model = new ViewCounterViewModel()
                    {
                        OnlineCount = online,
                        ThisWeekCount = thisWeek,
                        ThisMonthCount = thisMonth,
                        TodayCount = today,
                        TotalCount = total,
                    };
                }
                return Json(new
                {
                    success = true
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(new
                {
                    success = false,
                }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Slider
        public ActionResult Slider()
        {
            return View();
        }
        #endregion

    }
}