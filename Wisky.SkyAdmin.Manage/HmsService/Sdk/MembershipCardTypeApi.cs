using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.Sdk
{
    public partial class MembershipCardTypeApi
    {
        public IEnumerable<MembershipCardTypeViewModel> GetMembershipCardTypeByBrand(int brandId)
        {
            var MembershipCardType = this.BaseService.GetMembershipCardTypeByBrand(brandId)
                .ProjectTo<MembershipCardTypeViewModel>(this.AutoMapperConfig)
                .ToList();
            return MembershipCardType;
        }

        public IEnumerable<MembershipCardTypeViewModel> GetAllMembershipCardTypeByBrand(int brandId)
        {
            var MembershipCardType = this.BaseService.GetAllMembershipCardTypeByBrand(brandId)
                .ProjectTo<MembershipCardTypeViewModel>(this.AutoMapperConfig)
                .ToList();
            return MembershipCardType;
        }

        public async Task CreateMembershipCardTypeAsync(MembershipCardTypeViewModel model)
        {
            var entity = new MembershipCardType();
            entity = model.ToEntity();
            await this.BaseService.CreateAsync(entity);
        }
        public void UpdateMembershipCardType(MembershipCardTypeViewModel model)
        {
            this.BaseService.UpdateMembershipCardType(model.ToEntity());
        }
        public async Task UpdateMembershipCardTypeAsync(MembershipCardType entity)
        {
            await this.BaseService.UpdateAsync(entity);
        }
        public async Task DeleteMembershipCardTypeAsync(MembershipCardTypeViewModel model)
        {
            model.Active = false;
            await this.EditAsync(model.Id, model);
        }
        public async Task ChangeMembershipCardTypeActivation(int id, bool active)
        {
            await this.BaseService.ChangeMembershipCardTypeActivation(id, active);
        }
        public async Task<MembershipCardTypeViewModel> GetMembershipCardTypeByName(string name)
        {
            var MembershipCardType = await this.BaseService.GetMembershipCardTypeByName(name);
            if (MembershipCardType == null)
            {
                return null;
            }
            else
            {
                return new MembershipCardTypeViewModel(MembershipCardType);
            }
        }
        public IEnumerable<SelectListItem> GetMembershipCardTypeList()
        {
            var list = this.BaseService.Get(q => q.Active.Value).Select(q => new SelectListItem
            {
                Text = q.TypeName,
                Value = q.Id.ToString(),
            });
            return list;
        }
        public IQueryable<MembershipCardType> GetListActive()
        {
            return this.BaseService.Get(q => q.Active == true);
        }
        public MembershipCardType GetMembershipCardTypeByNameSync(string name)
        {
            var memberType = this.BaseService.GetMembershipCardTypeByNameSync(name);
            if (memberType == null)
            {
                return null;
            }
            else
            {
                return memberType;
            }
        }
        public MembershipCardType GetMembershipCardTypeByAppendCode(string code)
        {
            return this.BaseService.GetMembershipCardTypeByAppendCode(code);
        }
        public MembershipCardType GetMembershipCardTypeById(int id)
        {
            return this.BaseService.GetMembershipCardTypeById(id);
        }
        public string GetMembershipCardTypeNameByAppendCode(string code)
        {
            var membershipCardType = this.BaseService.GetMembershipCardTypeByAppendCode(code);
            if (membershipCardType != null)
            {
                return membershipCardType.TypeName;
            }
            else return "";
        }
    }
}
