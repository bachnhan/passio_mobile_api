using HmsService.Models;
using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Wisky.SkyAdmin.Manage.Controllers;
using Wisky.SkyAdmin.Manage.Models;

namespace Wisky.SkyAdmin.Manage.Areas.Admin.Controllers
{

    //[Authorize(Roles = Utils.AdminAuthorizeRoles)]
    public class BlogPostCollectionController : DomainBasedController
    {

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult IndexList(BootgridRequestViewModel request, int storeId)
        {
            var result = new BlogPostCollectionApi().GetAdminWithFilterAsync(
                storeId, request.searchPhrase,
                request.current, request.rowCount, request.FirstSortTerm);

            var model = new BootgridResponseViewModel<BlogPostCollectionDetailsViewModel>(result);
            return this.Json(model, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> Create(int storeId)
        {
            var model = new BlogPostCollectionEditViewModel();
            await PrepareCreate(model, storeId);
            ViewBag.Flag = "Create";
            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> Create(BlogPostCollectionEditViewModel model, int storeId)
        {
            if (!this.ModelState.IsValid)
            {
                await this.PrepareCreate(model,storeId);
                return this.View(model);
            }

            model.StoreId = storeId;
            //model.Active = true;

            var api = new BlogPostCollectionApi();
            await api.CreateAsync(model);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

        private async Task PrepareCreate(BlogPostCollectionEditViewModel model, int storeId)
        {
            var result = await new BlogPostCollectionApi()
                .GetActiveByStoreIdAsync(storeId);
            var select = result.Where(q => q.ParentId == null)
                               .ToSelectList(q => q.Name, q => q.Id.ToString(), q => model.ParentId == q.Id);/*, true, "--Không có--"*/
            model.AvailableBlogCollections = select;
        }

        public async Task<ActionResult> Edit(int? id, int storeId)
        {
            var info = new BlogPostCollectionViewModel();
            info = null;
            var IEinfo = await new BlogPostCollectionApi()
                       .GetActiveByStoreIdAsync(storeId);
            //.GetByStoreIdAsync(id.GetValueOrDefault(), this.CurrentStore.ID);
            foreach (var item in IEinfo)
            {
                if (item.Id == id.GetValueOrDefault())
                {
                    info = item;
                }
            }
            if (info == null)
            {
                return this.IdNotFound();
            }

            var model = new BlogPostCollectionEditViewModel(info, this.Mapper);

            await this.PrepareEdit(model,storeId);
            ViewBag.Flag = "Edit";
            return this.View(model);
        }

        [HttpPost, ValidateAntiForgeryToken, ValidateInput(false)]
        public async Task<ActionResult> Edit(BlogPostCollectionEditViewModel model, int storeId)
        {
            var api = new BlogPostCollectionApi();

            // Validate
            var IEinfo = await api
                .GetActiveByStoreIdAsync(storeId);
            var info = new BlogPostCollectionViewModel();
            info = null;
            foreach (var item in IEinfo)
            {
                if (item.Id == model.Id)
                {
                    info = item;
                }
            }
            if (info == null)
            {
                return this.IdNotFound();
            }

            if (!this.ModelState.IsValid)
            {
                await this.PrepareEdit(model,storeId);
                return this.View(model);
            }

            model.StoreId = storeId;
            model.Active = true;
            await api.EditAsync(model.Id, model);

            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

        public async Task PrepareEdit(BlogPostCollectionEditViewModel model, int storeId)
        {
            model.AvailableBlogCollections = (await new BlogPostCollectionApi()
                .GetActiveByStoreIdAsync(storeId))
                .Where(q => q.ParentId == null)
                .ToSelectList(q => q.Name, q => q.Id.ToString(), q => model.ParentId == q.Id);
        }

        public async Task<ActionResult> Delete(int? id, int storeId)
        {
            var api = new BlogPostCollectionApi();
            var IEinfo = await api
                .GetActiveByStoreIdAsync(storeId);
            var info = new BlogPostCollectionViewModel();
            foreach (var item in IEinfo)
            {
                if (item.Id == id.GetValueOrDefault())
                {
                    info = item;
                }
            }
            if (info == null)
            {
                return this.IdNotFound();
            }

            //Nhóm cha - mặc định khi code (ParentId = NULL) không deactive.
            //Deactive những Nhóm con (ParentId != NULL).
            if (info.ParentId != null)
            {
                await api.DeactivateAsync(id.Value);
            }
            return this.RedirectToAction("Index", new { parameters = this.CurrentPageDomain.Directory });
        }

    }
}