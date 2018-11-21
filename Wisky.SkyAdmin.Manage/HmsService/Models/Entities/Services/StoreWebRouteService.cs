using SkyWeb.DatVM.Data;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IStoreWebRouteService
    {

        IQueryable<StoreWebRouteWithModels> GetByStore(int storeId);
        Task<StoreWebRoute> GetByStoreIdAsync(int storeWebRouteId, int storeId);
    }

    public partial class StoreWebRouteService
    {

        public IQueryable<StoreWebRouteWithModels> GetByStore(int storeId)
        {
            return this.GetActive(q => q.StoreId == storeId)
                .Include(q => q.StoreWebRouteModels)
                .OrderBy(q => q.Position)
                .Select(q => new StoreWebRouteWithModels()
                {
                    StoreWebRoute = q,
                    Models = q.StoreWebRouteModels.Where(p => p.Active).AsQueryable(),
                });
        }

        // valid exist storeId 
        public async Task<StoreWebRoute> GetByStoreIdAsync(int storeWebRouteId, int storeId)
        {
            return await this.FirstOrDefaultActiveAsync(q => q.Id == storeWebRouteId && q.StoreId == storeId);
        }
    }

    public class StoreWebRouteWithModels
    {
        public StoreWebRoute StoreWebRoute { get; set; }
        public IQueryable<StoreWebRouteModel> Models { get; set; }
    }

}
