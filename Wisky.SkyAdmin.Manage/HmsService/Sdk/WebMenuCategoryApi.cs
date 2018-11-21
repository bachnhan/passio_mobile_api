using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class WebMenuCategoryApi
    {
        public WebMenuCategoryViewModel GetMainMenu(int storeId)
        {
            var entity = this.Service<WebMenuCategoryService>().GetMainMenu(storeId);
            var mainMenu = new WebMenuCategoryViewModel(entity);
            return mainMenu;
        }
    }
}
