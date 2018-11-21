using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using Wisky.SkyAdmin.Manage.Models;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{

    [Authorize(Roles = Utils.AdminAuthorizeRoles)]
    //[Authorize(Roles = Website.Models.Utils.AdminAuthorizeRoles)]
    public class BlogPostController : DomainBasedController
    {

        public ActionResult Index()
        {
            return this.View();
        }

        //public ActionResult IndexList(BootgridRequestViewModel request)
        //{
        //    var result = new BlogPostApi().GetAdminWithFilterAsync(
        //        this.CurrentStore.ID, request.searchPhrase,
        //        request.current, request.rowCount, request.FirstSortTerm);

        //    var model = new BootgridResponseViewModel<BlogPostDetailsViewModel>(result);
        //    return this.Json(model, JsonRequestBehavior.AllowGet);
        //}
        public ActionResult IndexList(BootgridRequestViewModel request, string startime, string endtime, int storeId)
        {
            var stime = startime.ToDateTime().GetStartOfDate();
            var eTime = endtime.ToDateTime().GetEndOfDate();
            var result = new BlogPostApi().GetAdminWithFilterAsync(
                storeId, request.searchPhrase,
                request.current, request.rowCount, request.FirstSortTerm, stime, eTime);
            var model = new BootgridResponseViewModel<BlogPostDetailsViewModel>(result);
            var jsonResult = this.Json(model, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;
        }

        public async Task<ActionResult> Create(int storeId)
        {
            var model = new BlogPostEditViewModel()
            {
                BlogPost = new BlogPostViewModel(),
            };

            await this.PrepareCreate(model, storeId);
            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> Create(BlogPostEditViewModel model, int storeId)
        {
            if (!this.ModelState.IsValid)
            {
                await this.PrepareCreate(model, storeId);
                return this.View(model);
            }

            model.BlogPost.StoreId = storeId;
            model.BlogPost.Active = true;
            model.BlogPost.CreatedTime = Utils.GetCurrentDateTime();
            model.BlogPost.Author = System.Web.HttpContext.Current.User.Identity.Name != null ? System.Web.HttpContext.Current.User.Identity.Name : "???";
            var api = new BlogPostApi();
            await api.CreateAsync(model.BlogPost,
                model.SelectedBlogPostCollections ?? new int[0],
                model.SelectedImages ?? new string[0]);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

        private async Task PrepareCreate(BlogPostEditViewModel model, int storeId)
        {
            model.AvailableCollections = (await new BlogPostCollectionApi()
                .GetActiveByStoreIdAsync(storeId))
                .ToSelectList(q => q.Name, q => q.Id.ToString(), q => false);
        }

        public async Task<ActionResult> Edit(int? id, int storeId)
        {
            var info = await new BlogPostApi()
                .GetDetailsByStoreIdAsync(id.GetValueOrDefault(), storeId);

            if (info == null)
            {
                return this.IdNotFound();
            }

            var model = new BlogPostEditViewModel(info, this.Mapper);

            await this.PrepareEdit(model,storeId);
            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> Edit(BlogPostEditViewModel model, int storeId)
        {
            var api = new BlogPostApi();
            // Validate
            var info = await api
                .GetByStoreIdAsync(model.BlogPost.Id, storeId);

            if (info == null)
            {
                return this.IdNotFound();
            }

            if (!this.ModelState.IsValid)
            {
                await this.PrepareEdit(model, storeId);
                return this.View(model);
            }

            model.BlogPost.StoreId = storeId;
            model.BlogPost.CreatedTime = info.CreatedTime;
            model.BlogPost.UpdatedTime = Utils.GetCurrentDateTime();
            model.BlogPost.Active = true;
            await api.EditAsync(model.BlogPost,
                model.SelectedBlogPostCollections ?? new int[0],
                model.SelectedImages ?? new string[0]);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

        public async Task PrepareEdit(BlogPostEditViewModel model, int storeId)
        {
            model.AvailableCollections = (await new BlogPostCollectionApi()
                .GetActiveByStoreIdAsync(storeId))
                .ToSelectList(q => q.Name, q => q.Id.ToString(), q => model.SelectedBlogPostCollections.Contains(q.Id));
        }

        public async Task<ActionResult> Delete(int? id, int storeId)
        {
            var productApi = new BlogPostApi();
            var info = await productApi
                .GetByStoreIdAsync(id.GetValueOrDefault(), storeId);

            if (info == null)
            {
                return this.IdNotFound();
            }

            await productApi.DeactivateAsync(id.Value);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }
        public async Task<ActionResult> ActiveBlog(int? id)
        {
            var productApi = new BlogPostApi();
            var info = await productApi
                .GetAsync(id.GetValueOrDefault());

            if (info == null)
            {
                return this.IdNotFound();
            }

            info.Active = true;
            await productApi.EditAsync(info.Id, info);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }
    }
}