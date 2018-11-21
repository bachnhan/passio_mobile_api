using SkyWeb.DatVM.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IMenuService
    {
        IQueryable<Menu> GetMenusByMenuTypeCode(int menuTypeCode);
        IEnumerable<Menu> GetMenuItemsByFilter(int menuType, long filter);
    }
    public partial class MenuService
    {
        public IQueryable<Menu> GetMenusByMenuTypeCode(int menuTypeCode)
        {
            return Get(q => q.MenuTypeCode == menuTypeCode || q.MenuTypeCode == (int)MenuTypeEnum.All);
        }

        public IEnumerable<Menu> GetMenuItemsByFilter(int menuType, long filter)
        {
            var menuItems = GetMenusByMenuTypeCode(menuType)
                .Where(q => q.ParentMenuId == null)
                .OrderBy(q => q.DisplayOrder).ToList();
            List<Menu> results = new List<Menu>();
            foreach (var item in menuItems)
            {
                long itemFeature = (long)(1 << item.FeatureCode);
                if ((itemFeature & filter) == itemFeature)
                {
                    var filteredItem = GetFilteredMenuItem(item, filter);
                    results.Add(filteredItem);
                }
            }


            return results;

        }

        Menu GetFilteredMenuItem(Menu menu, long filter)
        {
            Menu filteredMenu = menu;
            foreach (var item in menu.Menu1.ToList())
            {
                long itemFeature = (long)(1 << item.FeatureCode);
                if (filteredMenu != null)
                {
                    if ((itemFeature & filter) == itemFeature)
                    {
                        filteredMenu.Menu1
                            .ToList().Add(GetFilteredMenuItem(item, filter));
                        filteredMenu.Menu1 = filteredMenu.Menu1.OrderBy(q => q.DisplayOrder).ToList();
                    }
                    else
                    {
                        filteredMenu.Menu1 = filteredMenu.Menu1.Where(q => q.Id != item.Id).ToList();
                    }
                }
            }
            return filteredMenu;
        }
    }
}
