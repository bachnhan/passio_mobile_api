using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IStoreUserService
    {
        IEnumerable<StoreUser> GetAllStoreUser();
        Task DeleteByUsernameAndStoreAsync(string username, int id);
        IQueryable<StoreUser> GetStoreUserWithStoreId(int storeId);
        bool DeleteStoreUser(int? storeId, string username);
        string GetFirstStoreManager(int storeId);
        IQueryable<StoreUser> GetStoresFromUser(string username);
        IQueryable<StoreUser> GetActiveStoresFromUser(string username);
    }

    public partial class StoreUserService : IStoreUserService
    {
        public IEnumerable<StoreUser> GetAllStoreUser()
        {
            return this.GetActive();
        }

        public IQueryable<StoreUser> GetStoresFromUser(string username)
        {
            var result = this.Get(q => q.Username.Equals(username));
            return result;
        }

        public IQueryable<StoreUser> GetActiveStoresFromUser(string username)
        {
            var result = this.GetActive(q => q.Username.Equals(username));
            return result;
        }

        public async Task DeleteByUsernameAndStoreAsync(string username, int id)
        {
            var entity = this.Get(q => q.Username == username && q.StoreId == id).FirstOrDefault();
            await this.DeleteAsync(entity);
        }

        public bool DeleteStoreUser(int? storeId, string username)
        {

            try
            {

                var storeUser =
                    this.FirstOrDefault(
                        a =>
                            a.StoreId == storeId &&
                            a.Username.Equals(username));
                this.Delete(storeUser);
                //foreach (var storeUser in storeUsers)
                //{
                //    this.Delete(storeUser);
                //}
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public IQueryable<StoreUser> GetStoreUserWithStoreId(int storeId)
        {
            var result = this.Get(a => a.StoreId == storeId);
            return result;
        }

        public string GetFirstStoreManager(int storeId)
        {
            var manager = this.FirstOrDefault(q => (q.Username != null && q.Username != "") && q.StoreId == storeId);
            if(manager!=null)
            {
                return manager.Username;
            }
            return null;
        }
    }
}
