using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{

    public partial class StoreWebRouteApi
    {

        public async Task<StoreWebRoutesViewModel> GetStoreRoutesAsync(int storeId)
        {
            var routes = await this.BaseService.GetByStore(storeId)
                .ProjectTo<StoreWebRouteWithModelsViewModel>(this.AutoMapperConfig)
                .ToListAsync();

            var result = new StoreWebRoutesViewModel()
            {
                StoreWebRoutes = routes,
            };

            // Copy the route
            foreach (var route in routes)
            {
                if (route.StoreWebRoute.StoreWebRouteCopyId != null)
                {
                    foreach (var copyingRoute in routes)
                    {
                        if (copyingRoute.StoreWebRoute.Id == route.StoreWebRoute.StoreWebRouteCopyId)
                        {
                            route.Models = copyingRoute.Models.ToList();
                        }
                    }
                }

                if (route.StoreWebRoute.Path == "[global]")
                {
                    result.GlobalRoute = route;
                }
            }

            if (result.GlobalRoute != null)
            {
                routes.Remove(result.GlobalRoute);
            }

            return result;
        }
         public void GenStoreWebRoute(int storeId, int? storeWebRouteCopyId,string path,string viewName
             ,string LayoutName, int position, bool Active)
        {
            
        }

        //Valid store web route id
        //return bool (storeWebRoute != null)
        public async Task<bool> ValidateStoreWebRoute(int storeWebRouteId, int storeId)
        {
            var storeWebRoute = await this.BaseService.GetByStoreIdAsync(storeWebRouteId, storeId);
            return storeWebRoute != null;
        }
    }

}
