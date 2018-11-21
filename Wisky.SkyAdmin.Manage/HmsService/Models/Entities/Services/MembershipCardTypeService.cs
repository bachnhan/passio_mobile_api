using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IMembershipCardTypeService
    {
        MembershipCardType GetMembershipCardTypeById(int id);
        IQueryable<MembershipCardType> GetMembershipCardTypeByBrand(int brandId);
        IQueryable<MembershipCardType> GetAllMembershipCardTypeByBrand(int brandId);
        void UpdateMembershipCardType(MembershipCardType model);
        Task<MembershipCardType> GetMembershipCardTypeByName(string name);
        MembershipCardType GetMembershipCardTypeByNameSync(string name);
        MembershipCardType GetMembershipCardTypeByAppendCode(string code);
        Task ChangeMembershipCardTypeActivation(int id, bool active);
    }

    public partial class MembershipCardTypeService
    {
        public MembershipCardType GetMembershipCardTypeById(int id)
        {
            return this.FirstOrDefault(q => q.Id == id);
        }

        public IQueryable<MembershipCardType> GetMembershipCardTypeByBrand(int brandId)
        {
            var result = this.Get(q => q.BrandId == brandId && (q.Active.HasValue ? q.Active.Value : true));
            return result;
        }

        public IQueryable<MembershipCardType> GetAllMembershipCardTypeByBrand(int brandId)
        {
            var result = this.Get(q => q.BrandId == brandId);
            return result;
        }

        public void UpdateMembershipCardType(MembershipCardType model)
        {
            this.Update(model);
            this.Save();
        }
        public async Task<MembershipCardType> GetMembershipCardTypeByName(string name)
        {
            return await this.FirstOrDefaultAsync(q => q.TypeName == name && q.Active == true);
        }
        public MembershipCardType GetMembershipCardTypeByNameSync(string name)
        {
            var nameSub = name.Replace(" ", String.Empty).ToString().ToLower();
            return this.FirstOrDefault(q => q.TypeName.Replace(" ", String.Empty).ToString().ToLower() == nameSub && q.Active == true);
        }
        public MembershipCardType GetMembershipCardTypeByAppendCode(string code)
        {
            var dataType = this.GetActive().Where(q => q.AppendCode == code);
            if (dataType != null)
            {
                return dataType.FirstOrDefault();
            }
            return null;
        }
        public async Task ChangeMembershipCardTypeActivation(int id, bool active)
        {
            var card = this.GetMembershipCardTypeById(id);
            card.Active = active;

            await this.UpdateAsync(card);
        }
    }
}
