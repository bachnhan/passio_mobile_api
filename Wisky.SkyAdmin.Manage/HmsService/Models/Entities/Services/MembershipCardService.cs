using HmsService.Sdk;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IMembershipCardService
    {
        void UpdateMembershipCard(MembershipCard model);
        Task UpdateMemberShipCardEntityAsync(MembershipCard card);
        MembershipCard GetMembershipCardByCode(string code);
        MembershipCard GetMembershipCardByCodeBrandId(string code,int brandId);
        List<MembershipCard> GetMembershipCardDeactiveByBranId(int brandId);
        IQueryable<MembershipCard> GetMembershipCardActiveByCustomerIdByBrandId(int customerId, int brandId);
        IQueryable<MembershipCard> GetDeactiveListByBrandId(int brandId);
        IQueryable<MembershipCard> GetDeactiveListByBrandAndType(int brandId, int typeId);
        IQueryable<MembershipCard> GetActiveListByBrandId(int brandId);
        IQueryable<MembershipCard> GetActiveListByBrandAndType(int brandId, int typeId);
        IQueryable<MembershipCard> GetCloseListByBrandId(int brandId);
        IQueryable<MembershipCard> GetCloseListByBrandAndType(int brandId, int typeId);
        IQueryable<MembershipCard> GetAllMembershipCardSampleByBrandId(int brandId);
        IQueryable<MembershipCard> GetMembershipCardSampleByBrandAndType(int brandId, int typeId);
        IQueryable<MembershipCard> GetMembershipCardSampleActiveByBrandId(int brandId);
        IQueryable<MembershipCard> GetMembershipCardByCodeAsync(string code);
        MembershipCard GetGeneralMembershipCardByType(int typeId, int brandId);

    }
    public partial class MembershipCardService
    {
        public void UpdateMembershipCard(MembershipCard model)
        {
            this.Update(model);
            this.Save();
        }
        public Task UpdateMemberShipCardEntityAsync(MembershipCard card)
        {
            return this.UpdateAsync(card);
        }
        public MembershipCard GetMembershipCardByCode(string code)
        {
            return this.FirstOrDefault(q => q.MembershipCardCode != null && q.MembershipCardCode.Equals(code));
        }
        public MembershipCard GetMembershipCardByCodeBrandId(string code, int brandId)
        {
            return this.FirstOrDefault(q => q.MembershipCardCode != null && q.MembershipCardCode.Equals(code) && q.BrandId == brandId);
        }
        public IQueryable<MembershipCard> GetMembershipCardByCodeAsync(string code)
        {
            return this.GetActive(q => q.MembershipCardCode == code);
        }
        public List<MembershipCard> GetMembershipCardDeactiveByBranId(int brandId)
        {
            return this.GetActive(q => q.MembershipCardType.Active.Value && q.Status == 0 && q.BrandId ==  brandId).ToList();
        }
        public IQueryable<MembershipCard> GetMembershipCardActiveByCustomerIdByBrandId(int customerId, int brandId)
        {
            return this.GetActive(q => q.MembershipCardType.Active.Value && q.CustomerId == customerId && q.BrandId == brandId 
                    && q.Status == (int) MembershipStatusEnum.Active);
        }
        public IQueryable<MembershipCard> GetDeactiveListByBrandId(int brandId)
        {
            return this.GetActive(q => q.MembershipCardType.Active.Value && q.Status == 0 && q.BrandId == brandId);
        }
        public IQueryable<MembershipCard> GetDeactiveListByBrandAndType(int brandId, int typeId)
        {
            return this.GetActive(q => q.MembershipCardType.Active.Value && q.Status == (int)MembershipStatusEnum.Inactive && q.BrandId == brandId && q.MembershipTypeId == typeId);
        }
        public IQueryable<MembershipCard> GetActiveListByBrandId(int brandId)
        {
            return this.GetActive(q => q.MembershipCardType.Active.Value && q.Status == (int)MembershipStatusEnum.Active && q.BrandId == brandId);
        }
        public IQueryable<MembershipCard> GetActiveListByBrandAndType(int brandId, int typeId)
        {
            return this.GetActive(q => q.MembershipCardType.Active.Value && q.Status == 1 && q.BrandId == brandId && q.MembershipTypeId == typeId);
        }
        public IQueryable<MembershipCard> GetCloseListByBrandId(int brandId)
        {
            return this.GetActive(q => q.MembershipCardType.Active.Value && q.Status == 2 && q.BrandId == brandId);
        }
        public IQueryable<MembershipCard> GetCloseListByBrandAndType(int brandId, int typeId)
        {
            return this.GetActive(q => q.MembershipCardType.Active.Value && q.Status == 2 && q.BrandId == brandId && q.MembershipTypeId == typeId);
        }

        public MembershipCard GetGeneralMembershipCardByType(int typeId, int brandId)
        {
            return this.GetActive(q => q.BrandId == brandId && q.MembershipTypeId.Value == typeId && q.IsSample.Value).FirstOrDefault();
        }
        public IQueryable<MembershipCard> GetAllMembershipCardSampleByBrandId(int brandId)
        {
            return this.GetActive(q => q.MembershipCardType.Active.Value && q.BrandId == brandId && q.IsSample.Value);
        }
        public IQueryable<MembershipCard> GetMembershipCardSampleByBrandAndType(int brandId, int typeId)
        {
            return this.GetActive(q => q.MembershipCardType.Active.Value && q.BrandId == brandId && q.MembershipTypeId == typeId && q.IsSample.Value);
        }
        public IQueryable<MembershipCard> GetMembershipCardSampleActiveByBrandId(int brandId)
        {
            var membershipTypeService = DependencyUtils.Resolve<IMembershipCardTypeService>();
            var types = membershipTypeService.GetMembershipCardTypeByBrand(brandId);

            return this.GetActive(q => q.BrandId == brandId && q.IsSample.Value)
                .Join(types, p => p.MembershipTypeId, q => q.Id, (p, q) => p);
        }
    }
}
