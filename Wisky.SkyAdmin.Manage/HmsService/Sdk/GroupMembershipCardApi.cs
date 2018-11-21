using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    partial class GroupMembershipCardApi
    {
        public IQueryable<GroupMembershipCard> GetAllActiveGroupByBrandId(int brandId)
        {
            return this.BaseService.GetAllActiveGroupByBrandId(brandId);
        }
        public GroupMembershipCard GetGroupMembershipCardById(int id)
        {
            return this.BaseService.GetGroupMembershipCardById(id);
        }
        public GroupMembershipCard GetGroupMembershipCardByCode(string code)
        {
            return this.BaseService.GetGroupMembershipCardByCode(code);
        }
    }
}
