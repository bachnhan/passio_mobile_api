using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{
    public class CommentController : DomainBasedController
    {
        // GET: Admin/Comment
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LoadCmt(HmsService.Models.JQueryDataTableParamModel param, int choose)
        {
            string searchPattern = string.IsNullOrWhiteSpace(param.sSearch) ? null : param.sSearch;
            int idPro = -1;
            if (searchPattern != null)
            {
                idPro = int.Parse(searchPattern);
            }
            var ratingProductApi = new RatingProductApi();
            var favoritedApi = new FavoritedApi();
            var userApi = new AspNetUserApi();
            var listCmt = from ratingproductTable in ratingProductApi.Get()
                          join userTable in userApi.GetActive()
                          on ratingproductTable.UserId equals userTable.Id
                          where (choose == 0 ? (idPro == -1 ? (ratingproductTable.Active == false) : (ratingproductTable.Active == false && ratingproductTable.ProductId == idPro)) : (idPro == -1 ? (ratingproductTable.Active == true) : (ratingproductTable.Active == true && ratingproductTable.ProductId == idPro)))
                          select new { IdProduct = ratingproductTable.ProductId, IdUser = userTable.Id, UserName = userTable.UserName, Comment = ratingproductTable.ReviewContent, IdComment = ratingproductTable.Id };
            var totalRecords = listCmt.Count();
            var displayRecord = listCmt.Count();
            var count = param.iDisplayStart;
            var rs = listCmt
                .Skip(param.iDisplayStart)
                .Take(param.iDisplayLength)
                .Select(a => new IConvertible[]
            {
                ++count
                ,a.IdProduct,
                a.UserName,
                a.Comment,
                a.IdComment
            }).ToList();
            return Json(new
            {
                sEcho = param.sEcho,
                iTotalRecords = totalRecords,
                iTotalDisplayRecords = displayRecord,

                aaData = rs
            }, JsonRequestBehavior.AllowGet);

        }
        public ActionResult Check(int data)
        {
            var result = false;
            var ratingProductApi = new RatingProductApi();
            try
            {
                var comment = ratingProductApi.GetByCommentId(data);
                if (comment.Product != null)
                {
                    if (comment.Product.RatingTotal == null)
                    {
                        comment.Product.RatingTotal = 0;
                    }
                    if (comment.Product.NumOfUserVoted == null)
                    {
                        comment.Product.NumOfUserVoted = 0;
                    }
                    comment.Product.RatingTotal = comment.Product.RatingTotal + comment.Star;
                    comment.Product.NumOfUserVoted = comment.Product.NumOfUserVoted + 1;
                }

                ratingProductApi.Check(comment);
                result = true;
            }
            catch (Exception e)
            {
                return Json(new { success = result }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = result }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult unCheck(int data)
        {
            var result = false;
            var ratingProductApi = new RatingProductApi();
            try
            {
                var comment = ratingProductApi.GetByCommentId(data);
                if (comment.Product != null)
                {
                    if (comment.Product.RatingTotal == null)
                    {
                        comment.Product.RatingTotal = 0;
                    }
                    if (comment.Product.NumOfUserVoted == null)
                    {
                        comment.Product.NumOfUserVoted = 0;
                    }
                    if (comment.Product.RatingTotal != 0 && comment.Product.NumOfUserVoted != 0)
                    {
                        comment.Product.RatingTotal = comment.Product.RatingTotal - comment.Star;
                        comment.Product.NumOfUserVoted = comment.Product.NumOfUserVoted - 1;
                    }

                }
                ratingProductApi.unCheck(comment);
                result = true;
            }
            catch (Exception e)
            {
                return Json(new { success = result }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = result }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult DelCmt(int data)
        {
            var result = false;
            var ratingProductApi = new RatingProductApi();
            var productApi = new ProductApi();
            try
            {
                var comment = ratingProductApi.GetByCommentId(data);
                if (comment.Product != null)
                {
                    if (comment.Product.RatingTotal == null)
                    {
                        comment.Product.RatingTotal = 0;
                    }
                    if (comment.Product.NumOfUserVoted == null)
                    {
                        comment.Product.NumOfUserVoted = 0;
                    }
                    if (comment.Product.RatingTotal != 0 && comment.Product.NumOfUserVoted != 0)
                    {
                        comment.Product.RatingTotal = comment.Product.RatingTotal - comment.Star;
                        comment.Product.NumOfUserVoted = comment.Product.NumOfUserVoted - 1;
                    }
                    //productApi.EditProductEntity(comment.Product);
                }
                ratingProductApi.DeleteCmt(comment);
                result = true;
            }
            catch (Exception e)
            {
                return Json(new { success = result }, JsonRequestBehavior.AllowGet);

            }
            return Json(new { success = result }, JsonRequestBehavior.AllowGet);
        }
    }
}