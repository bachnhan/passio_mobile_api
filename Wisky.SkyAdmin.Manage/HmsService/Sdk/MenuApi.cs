using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class MenuApi
    {

        public IQueryable<Menu> GetMenuEntitiesByMenuTypeCode(int menuTypeCode)
        {
            return BaseService.Get(q => q.MenuTypeCode == menuTypeCode || q.MenuTypeCode == (int)MenuTypeEnum.All);
        }

        public IEnumerable<int?> GetMenuFeaturesByMenuType(int menuType)
        {
            return GetMenuEntitiesByMenuTypeCode(menuType).Select(q => q.FeatureCode).ToList();
        }


        public int GetCurrentMenuLevel(int menuid)
        {
            var menu = this.Get(menuid);
            return menu.MenuLevel;
        }

        public IEnumerable<Menu> GetMenuItemsByFilter(int menuType, long filter)
        {
            var menuItems = BaseService.GetMenuItemsByFilter(menuType, filter);
            return menuItems;
        }

        public int GetKeyFreeBrand()
        {
            var menutype = (int)MenuTypeEnum.BrandMenu;
            var listCurrent = this.BaseService.Get(q => q.MenuTypeCode == menutype).OrderBy(q => q.FeatureCode);
            var current = 0;
            foreach (var item in listCurrent)
            {
                current++;
                if (item.FeatureCode - current > 1)
                {
                    return item.FeatureCode.Value + 1;
                }
            }
            return current + 1;
        }

        public int GetKeyFreeStore()
        {
            var menutype = (int)MenuTypeEnum.StoreMenu;
            var listCurrent = this.BaseService.Get(q => q.MenuTypeCode == menutype).OrderBy(q => q.FeatureCode);
            var current = 0;
            foreach (var item in listCurrent)
            {
                current++;
                if (item.FeatureCode - current > 1)
                {
                    return item.FeatureCode.Value + 1;
                }
            }
            return current + 1;
        }

        #region New Code



        public List<MenuViewModel> GetMenuViewModelsByFilter(int menuType, string filter)
        {

            //Lay tat ca Menu cap 0
            var menuItems = BaseService.GetMenusByMenuTypeCode(menuType).Where(q => q.ParentMenuId == null).ToList();
            var menuList = new List<MenuViewModel>();

            //Duyet tung phan tu de lay ListMenu va append vao cuoi cua list chung
            foreach (var menuItem in menuItems)
            {
                var subMenuList = GetMenuAndChildByFilter(menuItem, filter);
                if (subMenuList != null)
                    menuList = menuList.Concat(subMenuList).ToList();
            }

            return menuList;
        }

        public IEnumerable<MenuViewModel> GetMenuViewModelsByFilterAndRoles(int menuType, string filter, string[] roles, List<string> listArea)
        {
            var menuItems = new List<Menu>();
            foreach(var area in listArea)
            {
                var subList = BaseService.GetMenusByMenuTypeCode(menuType).Where(q => q.ParentMenuId == null && q.Area.Contains(area)).ToList();
                menuItems = menuItems.Concat(subList).ToList();
            }            

            //Filter Menu by roles
            //Chi filter o Menu Level 0
            //menuItems = menuItems.Where(q => q.AspNetRoles.Any(r => roles.Contains(r.Name))).ToList();
            var menuList = new List<MenuViewModel>();

            //Duyet tung phan tu de lay ListMenu va append vao cuoi cua list chung
            //foreach (var menuItem in menuItems)
            //{
            //    var subMenuList = GetMenuAndChildByFilter(menuItem, filter);
            //    if (subMenuList != null)
            //        menuList = menuList.Concat(subMenuList).ToList();
            //}
            for(var i = 0; i < menuItems.Count; i++)
            {
                var subMenuList = GetMenuAndChildByFilter(menuItems[i], filter);
                if (subMenuList != null)
                    menuList = menuList.Concat(subMenuList).ToList();
            }


            return menuList;



            //if (!roles.Any(q => q == "StoreManager" || q == "BrandManager"))
            //{
            //    if (roles.Any(q => q == "Reception"))
            //    {
            //        menuItems = menuItems.Where(q => q.AspNetRoles.Any(r => r.Name.Equals("Reception"))).ToList();
            //    }
            //    if (roles.Any(q => q == "Employee"))
            //    {
            //        menuItems = menuItems.Where(q => q.AspNetRoles.Any(r => r.Name.Equals("Employee"))).ToList();
            //    }
            //}

            //var menuList = new List<MenuViewModel>();
            ////Sau buoc nay, tiep tuc loc cac MenuItem theo StoreFilter


            //foreach (var item in menuItems)
            //{
            //    var model = new MenuViewModel(item, filter);
            //    //if (model.FeatureCode == null)
            //    //{
            //    //    model = FilterMenuItem(model, filter);
            //    //    model.isSelected = model.ChildrenMenus.Count(q => q.isSelected) > 0;
            //    //}
            //    //else
            //    //{
            //    //    long itemFeature = (long)(1 << model.FeatureCode);
            //    //    model.isSelected = (itemFeature & filter) == itemFeature;
            //    //}
            //    menuList.Add(model);
            //}
            //return menuList;
        }

        private List<MenuViewModel> GetMenuAndChildByFilter(Menu menu, string filter)
        {

            if (menu == null)
                return null;

            var menuList = new List<MenuViewModel>();
            //Kiem tra chinh item
            if (CheckValidMenuItem(menu, filter))
            {
                menuList.Add(new MenuViewModel(menu));
                //Duyet de qui cho Menu con cua item
                if (menu.Menu1 != null)
                {
                    var subList = new List<MenuViewModel>();
                    var orderSubList = menu.Menu1.OrderBy(q => q.DisplayOrder);
                    foreach (var subMenu in orderSubList)
                    {
                        //Goi de qui chinh ham nay
                        subList = GetMenuAndChildByFilter(subMenu, filter);
                        if (subList != null)
                        {
                            menuList = menuList.Concat(subList).ToList();
                        }
                    }
                }
                return menuList;

            }

            return null;

        }


        private bool CheckValidMenuItem(Menu menu, string filter)
        {       
            return menu.Area != null ? true : false;
        }




        #endregion

        #region Old Code 

        //public IEnumerable<MenuViewModel> GetMenuViewModelsByFilter(int menuType, string filter)
        //{
        //    //Lay tat ca Menu cap 0
        //    var menuItems = BaseService.GetMenusByMenuTypeCode(menuType).Where(q => q.ParentMenuId == null).ToList();
        //    var menuList = new List<MenuViewModel>();
        //    foreach (var item in menuItems)
        //    {
        //        var model = new MenuViewModel(item, filter);
        //        //if (model.FeatureCode == null)
        //        //{
        //        //    model = FilterMenuItem(model, filter);
        //        //    model.isSelected = model.ChildrenMenus.Count(q => q.isSelected) > 0;
        //        //}
        //        //else
        //        //{
        //        //    long itemFeature = (long)(1 << model.FeatureCode);
        //        //    model.isSelected = (itemFeature & filter) == itemFeature;
        //        //}
        //        menuList.Add(model);
        //    }
        //    return menuList;
        //}



        //public IEnumerable<MenuViewModel> GetMenuViewModelsByFilterAndRoles(int menuType, string filter, string[] roles)
        //{
        //    var menuItems = BaseService.GetMenusByMenuTypeCode(menuType).Where(q => q.ParentMenuId == null).ToList();

        //    if (!roles.Any(q => q == "StoreManager" || q == "BrandManager"))
        //    {
        //        if (roles.Any(q => q == "Reception"))
        //        {
        //            menuItems = menuItems.Where(q => q.AspNetRoles.Any(r => r.Name.Equals("Reception"))).ToList();
        //        }
        //        if (roles.Any(q => q == "Employee"))
        //        {
        //            menuItems = menuItems.Where(q => q.AspNetRoles.Any(r => r.Name.Equals("Employee"))).ToList();
        //        }
        //    }

        //    var menuList = new List<MenuViewModel>();
        //    //Sau buoc nay, tiep tuc loc cac MenuItem theo StoreFilter


        //    foreach (var item in menuItems)
        //    {
        //        var model = new MenuViewModel(item, filter);
        //        //if (model.FeatureCode == null)
        //        //{
        //        //    model = FilterMenuItem(model, filter);
        //        //    model.isSelected = model.ChildrenMenus.Count(q => q.isSelected) > 0;
        //        //}
        //        //else
        //        //{
        //        //    long itemFeature = (long)(1 << model.FeatureCode);
        //        //    model.isSelected = (itemFeature & filter) == itemFeature;
        //        //}
        //        menuList.Add(model);
        //    }
        //    return menuList;
        //}
        #endregion
        MenuViewModel FilterMenuItem(MenuViewModel menu, long filter)
        {
            MenuViewModel filteredMenu = menu;
            foreach (var item in menu.ChildrenMenus)
            {
                if (item.FeatureCode == null)
                {
                    filteredMenu.ChildrenMenus
                        .ToList().Add(FilterMenuItem(item, filter));
                    item.isSelected = item.ChildrenMenus.Count(q => q.isSelected) > 0;
                }
                else
                {
                    long itemFeature = (long)(1 << item.FeatureCode);
                    item.isSelected = ((itemFeature & filter) == itemFeature);
                }
            }

            return filteredMenu;
        }

    }
}
