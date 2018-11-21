using HmsService.Sdk;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IGroupMembershipCardService
    {
        IQueryable<GroupMembershipCard> GetAllActiveGroupByBrandId(int brandId);
        GroupMembershipCard GetGroupMembershipCardById(int id);
        GroupMembershipCard GetGroupMembershipCardByCode(string code);
    }
    public partial class GroupMembershipCardService
    {
        public IQueryable<GroupMembershipCard> GetAllActiveGroupByBrandId(int brandId)
        {
            return this.Get(c => c.Active == true && c.BrandId==brandId);
        }
        public GroupMembershipCard GetGroupMembershipCardById(int id)
        {
            return this.Get(id);
        }
        public GroupMembershipCard GetGroupMembershipCardByCode(string code)
        {
            return this.FirstOrDefault(q => q.GroupCode.Equals(code));
        }
    }
}
