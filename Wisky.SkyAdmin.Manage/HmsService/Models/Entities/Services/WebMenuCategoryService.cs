using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial class WebMenuCategoryService
    {
        public WebMenuCategory GetMainMenu(int storeId)
        {
            var webMenuCategoryRepo = SkyWeb.DatVM.Mvc.Autofac.DependencyUtils.Resolve<HmsService.Models.Entities.Repositories.IWebMenuCategoryRepository>();
            webMenuCategoryRepo.GetActive();
            var mainMenu = webMenuCategoryRepo.Get(m => m.StoreId == storeId).FirstOrDefault();
            return mainMenu;
        }
    }
}
