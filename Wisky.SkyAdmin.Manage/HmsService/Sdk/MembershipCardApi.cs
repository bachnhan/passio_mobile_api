using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class MembershipCardApi
    {
        public MembershipCardEditViewModel GetMembershipCardById(int id)
        {
            var membershipCard = this.BaseService.Get(id);
            if (membershipCard == null)
            {
                return null;
            }
            else
            {
                return new MembershipCardEditViewModel(membershipCard);
            }
        }
        public void UpdateMembershipCard(MembershipCardEditViewModel model)
        {
            var entity = this.BaseService.Get(model.Id);
            model.CopyToEntity(entity);
            var customerService = DependencyUtils.Resolve<ICustomerService>();
            entity.Customer = customerService.Get(model.CustomerId);
            this.BaseService.UpdateMembershipCard(entity);
        }

        public IEnumerable<MembershipCard> GetMembershipCardSample(int brandId)
        {
            return this.BaseService.Get(q => q.IsSample == true && q.Active == true && q.BrandId == brandId);
        }

        public MembershipCardViewModel GetMembershipCardBaseOnID(int cardId)
        {
            var result = new MembershipCardViewModel(this.BaseService.Get(cardId));
            return result;
        }

        public async Task<int> AddMembershipCardBaseOnCardSample(string membershipCardCode, MembershipCardViewModel cardSample, int customerId, int storeId)
        {
            var model = new MembershipCardViewModel();
            model.IsSample = false;
            model.MembershipTypeId = cardSample.MembershipTypeId;
            model.BrandId = cardSample.BrandId;
            model.CreatedTime = Utils.GetCurrentDateTime();
            model.CSV = "";
            model.Active = true;
            model.Status = (int)MembershipStatusEnum.Inactive;
            model.MembershipCardCode = membershipCardCode;
            model.CustomerId = customerId;
            model.StoreId = storeId;
            model.CreateBy = cardSample.CreateBy;
            await this.CreateAsync(model);
            return this.GetMembershipCardByCode(model.MembershipCardCode).Id;
        }

        public MembershipCard AddMembershipCard(string membershipCardCode, int brandId, int customerId,int membershipTypeId)
        {
            var model = new MembershipCardViewModel();
            model.IsSample = false;
            model.MembershipTypeId = membershipTypeId;
            model.BrandId = brandId;
            model.CreatedTime = Utils.GetCurrentDateTime();
            model.CSV = "";
            model.Active = true;
            model.Status = (int)MembershipStatusEnum.Active;
            model.MembershipCardCode = membershipCardCode;
            model.CustomerId = customerId;
            //model.StoreId = storeId;
            //model.CreateBy = cardSample.CreateBy;
            this.Create(model);
            return this.GetMembershipCardByCode(model.MembershipCardCode);
        }

        public async Task UpdateMembershipCard(MembershipCardViewModel model)
        {
            await this.EditAsync(model.Id, model);
        }

        public async Task UpdateMemberShipCardEntityAsync(MembershipCard card)
        {
            await this.BaseService.UpdateMemberShipCardEntityAsync(card);
        }

        public async Task DeleteMembershipCardAsync(MembershipCardViewModel model)
        {
            model.Active = false;
            await this.EditAsync(model.Id, model);
        }

        public MembershipCard GetMembershipCardByCode(string membershipCardCode)
        {
            return this.BaseService.GetMembershipCardByCode(membershipCardCode);
        }
        public MembershipCard GetMembershipCardByCodeByBrandId(string membershipCardCode,int brandId)
        {
            return this.BaseService.GetMembershipCardByCodeBrandId(membershipCardCode,brandId);
        }

        public async Task<IEnumerable<MembershipCard>> GetMembershipCardByCodeAsync(string membershipCardCode)
        {
            return await this.BaseService.GetMembershipCardByCodeAsync(membershipCardCode).ToListAsync();
        }

        public List<MembershipCard> GetMembershipCardDeactiveByBranId(int brandId)
        {
            return this.BaseService.GetMembershipCardDeactiveByBranId(brandId);
        }
        public IEnumerable<MembershipCard> GetMembershipCardActiveByCustomerIdByBrandId(int customerId, int brandId)
        {
            return this.BaseService.GetMembershipCardActiveByCustomerIdByBrandId(customerId, brandId);
        }

        public IQueryable<MembershipCard> GetMembershipCardActiveByCustomerId(int customerId)
        {
            return this.BaseService.Get(a => a.Active).Where(a => a.CustomerId != null && a.CustomerId == customerId);
        }

        // datct
        //public IEnumerable<MembershipCard> GetMembershipCardActiveByCustomerId(int customerId)
        //{
        //    return this.BaseService.Get(a => a.Active && a.MembershipCardTypeMappings.Count != 0 && a.CustomerId == customerId);
        //}
        //public IQueryable<MembershipCard> GetMembershipCardActiveByCustomerIdAndMemberId(int customerId, int membershipId)
        //{
        //    return this.BaseService.Get(a => a.Active && a.MembershipCardTypeMappings.Count != 0 && a.CustomerId == customerId && a.Id == membershipId);
        //}

        public async Task DeactivateOrActivateMembershipCard(int id)
        {
            var entity = await this.BaseService.GetAsync(id);
            if (entity.Status == (int)MembershipStatusEnum.Inactive || entity.Status == (int)MembershipStatusEnum.Suspensed)
            {
                entity.Status = (int)MembershipStatusEnum.Active;
            }
            else
            {
                entity.Status = (int)MembershipStatusEnum.Suspensed;
            }
            await this.BaseService.UpdateAsync(entity);
        }

        public IQueryable<MembershipCard> GetDeactiveListByBrandId(int brandId)
        {
            return this.BaseService.GetDeactiveListByBrandId(brandId);
        }
        public IQueryable<MembershipCard> GetDeactiveListByBrandAndType(int brandId, int typeId)
        {
            return this.BaseService.GetDeactiveListByBrandAndType(brandId, typeId);
        }
        public IQueryable<MembershipCard> GetActiveListByBrandId(int brandId)
        {
            return this.BaseService.GetActiveListByBrandId(brandId);
        }
        public IQueryable<MembershipCard> GetActiveListByBrandAndType(int brandId, int typeId)
        {
            return this.BaseService.GetActiveListByBrandAndType(brandId, typeId);
        }
        public IQueryable<MembershipCard> GetCloseListByBrandId(int brandId)
        {
            return this.BaseService.GetCloseListByBrandId(brandId);
        }
        public IQueryable<MembershipCard> GetCloseListByBrandAndType(int brandId, int typeId)
        {
            return this.BaseService.GetCloseListByBrandAndType(brandId, typeId);
        }
        public int CreateMembershipCard(string membershipCardCode, int memberTypeId, int brandId)
        {

            MembershipCardViewModel newCard = new MembershipCardViewModel();
            newCard.MembershipCardCode = membershipCardCode;
            newCard.CreatedTime = Utils.GetCurrentDateTime();
            if (memberTypeId != 0)
            {
                newCard.MembershipTypeId = memberTypeId;
            }
            newCard.Active = false;
            newCard.BrandId = brandId;
            newCard.Status = 0;
            this.Create(newCard);
            return newCard.Id;
        }

        public async Task<int> CreateMembershipCardAsync(MembershipCardViewModel model)
        {
            model.Active = false;
            model.Status = (int)MembershipStatusEnum.Inactive;

            await this.CreateAsync(model);
            return this.GetMembershipCardByCode(model.MembershipCardCode).Id;
        }

        public async Task DeactivateCardAsync(int id)
        {
            var entity = await this.BaseService.GetAsync(id);
            entity.CustomerId = null;
            entity.Status = (int)MembershipStatusEnum.Inactive;
            await this.BaseService.UpdateAsync(entity);
        }
        public IQueryable<MembershipCard> GetListDeactiveByFilter(int typeId, int brandId)
        {
            var membershipList = this.BaseService.GetDeactiveListByBrandAndType(brandId, typeId);
            return membershipList;
        }

        public bool ValidateCardName(string code)
        {
            var card = this.BaseService.FirstOrDefault(a => a.MembershipCardCode == code);
            if (card == null) return true;
            else return false;
        }

        //General membership card functions
        public MembershipCard GetGeneralMembershipCardByType(int typeId, int brandId)
        {
            return this.BaseService.GetGeneralMembershipCardByType(typeId, brandId);
        }

        public IQueryable<MembershipCard> GetAllMembershipCardSampleByBrandId(int brandId)
        {
            return this.BaseService.GetAllMembershipCardSampleByBrandId(brandId);
        }

        public IQueryable<MembershipCard> GetMembershipCardSampleActiveByBrandId(int brandId)
        {
            return this.BaseService.GetMembershipCardSampleActiveByBrandId(brandId);
        }

        public IQueryable<MembershipCard> GetMembershipCardSampleByBrandAndType(int brandId, int typeId)
        {
            return this.BaseService.GetMembershipCardSampleByBrandAndType(brandId, typeId);
        }
    }
}
