using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{

    public partial class AspNetUserApi
    {
        public AspNetUser GetUserById(string userId)
        {
            return this.BaseService.Get(userId);
        }

        public IQueryable<AspNetUserDetailsViewModel> GetAllAccountBrandMananger()
        {
            return this.BaseService.GetAllAccountBrandManager().ProjectTo<AspNetUserDetailsViewModel>(this.AutoMapperConfig);
        }

        public IEnumerable<AspNetUser> GetAllAccountActiveByBrandID(int brandId)
        {
            return this.BaseService.Get(q => q.AspNetRoles.Any(a => a.Name.Equals("ActiveUser") && q.BrandId == brandId)).AsEnumerable();
        }

        public async Task<IEnumerable<AspNetUserDetailsViewModel>> GetDetails()
        {
            return await this.BaseService.GetDetails()
                .ProjectTo<AspNetUserDetailsViewModel>(this.AutoMapperConfig)
                .ToListAsync();
        }

        public IEnumerable<AspNetUser> GetAllAccountByStoreId(int storeId)
        {
            return this.BaseService.GetActive(q => q.Store.ID ==  storeId);
        }

        public async Task<AspNetUserDetailsViewModel> GetDetails(string id)
        {
            var entity = await this.BaseService.GetDetails(id);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new AspNetUserDetailsViewModel(entity);
            }
        }

        public async Task EditAsync(AspNetUserViewModel model, string[] roleIds, int? brandId)
        {
            var entity = await this.BaseService.GetAsync(model.Id);
            entity.BrandId = brandId;

            await this.BaseService.UpdateAsync(entity, roleIds);
        }

        public PagingViewModel<AspNetUserViewModel> GetAdminWithFilterAsync(string keyword, int currPage, int pageSize, KeyValuePair<string, bool> sortKeyAsc)
        {
            var pagedList = this.BaseService.GetAdminByAccountWithFilter(keyword, sortKeyAsc)
                .ProjectTo<AspNetUserViewModel>(this.AutoMapperConfig)
                .Page(currPage, pageSize);

            return new PagingViewModel<AspNetUserViewModel>(pagedList);
        }

        public AspNetUser GetUserByUsername(string username)
        {
            return this.BaseService.GetUserByUsernameSync(username);
        }

        public IEnumerable<AspNetUserViewModel> GetStoreManagers()
        {
            var result = this.BaseService.GetStoreManagers().ProjectTo<AspNetUserViewModel>(this.AutoMapperConfig);
            return result;
        }

        public IEnumerable<AspNetUserViewModel> GetStoreManagersByBrandId(int brandId)
        {
            var result = this.BaseService.GetStoreManagersByBrandId(brandId).ProjectTo<AspNetUserViewModel>(this.AutoMapperConfig);
            return result;
        }

        public IEnumerable<AccountApiViewModel> GetAccountApiList(int storeId, string token)
        {
            //var storeUserService = DependencyUtils.Resolve<IStoreUserService>();
            var storeUserApi = new StoreUserApi();
            var aspNetUserApi = new AspNetUserApi();
            var listUser = new List<AccountApiViewModel>();
            var storeUser = storeUserApi.GetStoreUserWithStoreId(storeId);

            foreach (var item in storeUser)
            {
                var user = aspNetUserApi.GetUserByUsername(item.Username);

                if(user != null)
                {
                    var account = new AccountApiViewModel()
                    {
                        AccountId = user.UserName,
                        AccountPassword = user.PasswordHash,
                        Role = user.AspNetRoles.Any(a => a.Name.Equals("StoreManager")) ? "StoreManager" : "Staff",
                        StaffName = user.FullName,
                        IsUsed = true,
                        StoreId = storeId,
                        Token = token
                    };
                    listUser.Add(account);
                }
            }

            //listUser.AddRange(
            //           from item in storeUser
            //           select this.BaseService.GetListAccountWithUserName(item.Username)
            //               into user
            //           where user != null
            //           select new AccountApiViewModel
            //           {
            //               AccountId = user.UserName,
            //               AccountPassword = user.PasswordHash,
            //               Role =
            //                   user.AspNetRoles.Any(a => a.Name.Equals("StoreManager")) ? "StoreManager" : "Staff",
            //               StaffName = user.FullName,
            //               IsUsed = true,
            //               StoreId = storeId,
            //               Token = token
            //           });



            return listUser;
        }

        public AspNetUserViewModel CheckLoginApi(string username, string password)
        {
            var user = this.BaseService.CheckLogin(username, password);
            var reuslt = new AspNetUserViewModel(user);
            return reuslt;
        }

        public string[] GetUserRoles(string username)
        {
            return this.BaseService.GetUserRoles(username);
        }

        public bool ChangeAccountPassword(AspNetUser user)
        {
            return this.BaseService.ChangeAccountPassword(user);
        }

        public IEnumerable<AspNetUser> GetListUserByStoreUsers(IEnumerable<StoreUser> listStoreUser)
        {
            List<AspNetUser> resultList = new List<AspNetUser>();
            foreach (var item in listStoreUser)
            {
                var user = this.BaseService.Get(q => q.UserName.Equals(item.Username)).FirstOrDefault();
                if(user != null)
                {
                    resultList.Add(user);
                }
            }

            return resultList;
        }
    }

}
