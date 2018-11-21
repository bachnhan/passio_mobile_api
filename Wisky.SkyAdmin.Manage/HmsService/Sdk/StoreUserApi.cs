using AutoMapper;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class StoreUserApi
    {
        public IEnumerable<StoreUser> GetAllStoreUser()
        {
            return this.BaseService.GetAllStoreUser();
        }

        public async Task DeleteStoreUserAsync(string username, int id)
        {
            await this.BaseService.DeleteByUsernameAndStoreAsync(username, id);
        }

        public async Task<bool> DeleteAllStoreUserByUsername(string username)
        {
            try
            {
                var storeUsers = this.BaseService.GetActive(q => q.Username.Equals(username)).ToList();
                foreach (var user in storeUsers)
                {
                    await this.BaseService.DeleteAsync(user);
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetFirstStoreManager(int storeId)
        {
            string manager = this.BaseService.GetFirstStoreManager(storeId);
            if(manager==null)
            {
                return null;
            }
            else
            {
                return manager;
            }
        }

        public IEnumerable<string> GetStoreManagerByStoreId(int storeId)
        {
            var managers = this.BaseService.GetStoreUserWithStoreId(storeId).Select(q => q.Username);
            if(managers==null)
            {
                return null;
            }
            else
            {
                return managers;
            }
        }

        public IEnumerable<StoreUserViewModel> GetStoresFromUser(string username)
        {
            var result = BaseService.GetStoresFromUser(username)
                .ProjectTo<StoreUserViewModel>(this.AutoMapperConfig)
                .ToList();

            return result;
        }

        public IEnumerable<StoreUserViewModel> GetActiveStoresFromUser(string username)
        {
            var result = BaseService.GetActiveStoresFromUser(username)
                .ProjectTo<StoreUserViewModel>(this.AutoMapperConfig)
                .ToList();

            return result;
        }

        public IEnumerable<StoreUser> GetStoreUserWithStoreId(int id)
        {
            return this.BaseService.GetStoreUserWithStoreId(id);
                //.ProjectTo<StoreUserViewModel>(this.AutoMapperConfig);
        }
    }
}
