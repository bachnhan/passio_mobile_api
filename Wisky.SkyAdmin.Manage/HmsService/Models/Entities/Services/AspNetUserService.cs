using HmsService.Models.Entities.Repositories;
using SkyWeb.DatVM.Data;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IAspNetUserService
    {
        IQueryable<AspNetUserDetails> GetAllAccountBrandManager();
        IQueryable<AspNetUserDetails> GetDetails(bool prioritizeAdminAccount = true);
        Task<AspNetUserDetails> GetDetails(string id);
        System.Threading.Tasks.Task UpdateAsync(AspNetUser entity, string[] roleIds);
        IQueryable<AspNetUser> GetStoreManagers();
        IQueryable<AspNetUser> GetStoreManagersByBrandId(int brandId);
        IQueryable<AspNetUser> GetAdminByAccountWithFilter(string keyword, KeyValuePair<string, bool> orderByProperty);

        IQueryable<AspNetUser> GetAllEmployee();

        IQueryable<AspNetUser> GetAllEmployeeByBrandId(int brandId);

        List<AspNetUser> GetEmployeeByStoreId(int id);

        string[] GetWorkedEmployeeByBrandId(int? brandId);
        AspNetUser GetEmployeeByUsername(string username);

        AspNetUser GetListAccountWithUserName(string userName);

        AspNetUser CheckLogin(string username, string password);

        string[] GetUserRoles(string username);

        bool ChangeAccountPassword(AspNetUser user);

        #region StoreProduct
        Task<AspNetUser> GetUserByUsername(string username);
        AspNetUser GetUserByUsernameSync(string username);
        #endregion


    }

    public partial class AspNetUserService
    {
        public IQueryable<AspNetUser> GetAllEmployee()
        {
            return this.GetActive().Where(a => a.AspNetRoles.Count(b => b.Name.Equals("Reception")) > 0);
        }

        public IQueryable<AspNetUser> GetAllEmployeeByBrandId(int brandId)
        {
            return this.GetActive(q => q.Store.BrandId == brandId);
        }

        public AspNetUser GetEmployeeByUsername(string username)
        {
            return this.FirstOrDefaultActive(q => q.UserName.Equals(username));
        }

        public List<AspNetUser> GetEmployeeByStoreId(int id)
        {
            var storeUserService = DependencyUtils.Resolve<IStoreUserService>();

            var storeUsers = storeUserService.GetActive(q => q.StoreId == id);

            var users = this.GetActive();

            List<AspNetUser> listUsers = new List<AspNetUser>();
            foreach (var storeUser in storeUsers)
            {
                foreach (var user in users)
                {
                    if (user.UserName.Equals(storeUser.Username))
                    {
                        listUsers.Add(user);
                    }
                }
            }
            return listUsers;
        }

        public string[] GetWorkedEmployeeByBrandId(int? brandId)
        {
            var storeUserService = DependencyUtils.Resolve<IStoreUserService>();

            var storeUsers = storeUserService.GetActive()
                .Select(q => q.Username).Distinct().ToArray();

            var users = this.GetAllEmployee().Select(a => a.UserName).ToArray();



            return users.Except(storeUsers).ToArray();
        }

        public IQueryable<AspNetUserDetails> GetAllAccountBrandManager()
        {
            return this.GetActive().Select(q => new AspNetUserDetails()
            {
                AspNetUser = q,
                Roles = q.AspNetRoles,
                AdminStoreName = q.Store == null ? null : q.Store.Name,
                AdminStoreId = q.AdminStoreId,
            }); ;
        }

        public IQueryable<AspNetUserDetails> GetDetails(bool prioritizeAdminAccount = true)
        {
            var query = this.Get()
                .Select(q => new AspNetUserDetails()
                {
                    AspNetUser = q,
                    Roles = q.AspNetRoles,
                    AdminStoreName = q.Store == null ? null : q.Store.Name,
                    AdminStoreId = q.AdminStoreId,
                });

            if (prioritizeAdminAccount)
            {
                query = query
                    .OrderByDescending(q => q.AspNetUser.AspNetRoles.Any(p => p.Name == Utils.SysAdminRole))
                    .ThenByDescending(q => q.AspNetUser.AspNetRoles.Any(p => p.Name == Utils.AdminRole))
                    .ThenBy(q => q.AspNetUser.UserName);
            }
            else
            {
                query = query.OrderBy(q => q.AspNetUser.UserName);
            }

            return query;
        }

        public async Task<AspNetUserDetails> GetDetails(string id)
        {
            var entity = await this.GetAsync(id);

            if (entity == null)
            {
                return null;
            }
            else
            {
                return new AspNetUserDetails()
                {
                    AspNetUser = entity,
                    Roles = entity.AspNetRoles,
                    AdminStoreName = entity.Store?.Name,
                };
            }
        }

        public async System.Threading.Tasks.Task UpdateAsync(AspNetUser entity, string[] roleIds)
        {
            var roleRepo = DependencyUtils.Resolve<IAspNetRoleRepository>();

            entity.AspNetRoles.Clear();
            foreach (var roleId in roleIds)
            {
                var role = await roleRepo.GetAsync(roleId);
                entity.AspNetRoles.Add(role);
            }

            await this.UpdateAsync(entity);
        }

        public IQueryable<AspNetUser> GetAdminByAccountWithFilter(string keyword, KeyValuePair<string, bool> orderByProperty)
        {
            var result = this.GetActive(q =>
                (keyword == null || q.FullName.Contains(keyword) || q.UserName.Contains(keyword)));

            StoreSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case StoreSortableProperty.Id:
                        result = result.OrderBy(q => q.Id, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.Name:
                        result = result.OrderBy(q => q.UserName, orderByProperty.Value);
                        break;
                    case StoreSortableProperty.ShortName:
                        result = result.OrderBy(q => q.FullName, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                result = result.OrderBy(q => q.Id);
            }

            return result;
        }

        //public IQueryable<AspNetUser> GetStoreManagers()
        //{
        //    var manager = this.Get().Where(
        //            a => a.AspNetRoles.Any(b => b.Name == "ActiveUser") && (a.AspNetRoles.Any(b => b.Name == "StoreManager") || a.AspNetRoles.Any(b => b.Name == "StoreReportViewer") || a.AspNetRoles.Any(b => b.Name == "BrandManager")))
        //                .Select(a => a.UserName);
        //    return this.repository.Get(a => manager.Contains(a.UserName));
        //}

        public IQueryable<AspNetUser> GetStoreManagers()
        {
            var manager = this.Get().Where(
                    a => a.AspNetRoles.Any(b => b.Name == "ActiveUser") && (a.AspNetRoles.Any(b => b.Name == "StoreManager") || a.AspNetRoles.Any(b => b.Name == "StoreReportViewer") || a.AspNetRoles.Any(b => b.Name == "BrandManager")));
            return manager;
        }

        public IQueryable<AspNetUser> GetStoreManagersByBrandId(int brandId)
        {
            var manager = this.Get().Where(
                    a => a.BrandId == brandId && a.AspNetRoles.Any(b => b.Name == "ActiveUser") && (a.AspNetRoles.Any(b => b.Name == "StoreManager") || a.AspNetRoles.Any(b => b.Name == "BrandManager")));
            return manager;
        }

        public AspNetUser GetListAccountWithUserName(string userName)
        {
            var result = this.Get(a => a.UserName == userName).FirstOrDefault();
            return result;
        }

        public AspNetUser CheckLogin(string username, string password)
        {
            return this.Get(a => a.UserName.Equals(username) && a.PasswordHash.Equals(password)).FirstOrDefault();
        }

        public string[] GetUserRoles(string username)
        {
            var user = this.Get(a => a.UserName.Equals(username)).FirstOrDefault();
            return user.AspNetRoles.Select(a => a.Name).ToArray();
        }

        public bool ChangeAccountPassword(AspNetUser user)
        {
            try
            {
                this.Update(user);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #region StoreReport
        public async Task<AspNetUser> GetUserByUsername(string username)
        {
            return await this.FirstOrDefaultAsync(a => a.UserName.Equals(username));
        }

        public AspNetUser GetUserByUsernameSync(string username)
        {
            return this.Get(a => a.UserName.Equals(username)).FirstOrDefault();
        }

        #endregion

        //public AspNetUser GetUserByUsername(string username)
        //{

        //    return this.Get(a => a.UserName.Equals(username)).FirstOrDefault();
        //}
    }

    public class AspNetUserDetails : IEntity
    {

        public AspNetUser AspNetUser { get; set; }
        public ICollection<AspNetRole> Roles { get; set; }
        public string AdminStoreName { get; set; }
        public int? AdminStoreId { get; set; }

    }

}
