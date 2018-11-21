//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace HmsService.Models.Entities.Services
//{
//    public partial interface IMembershipCardTypeMappingService
//    {
//        MembershipCardTypeMapping GetByCardIdAndType(int cardId, int typeId);
//        IQueryable<MembershipCardTypeMapping> GetListByCardId(int cardId);
//        IQueryable<MembershipCardTypeMapping> GetDeactiveListByBrandIdAndTypeId(int brandId, int typeId);
//        IQueryable<MembershipCardTypeMapping> GetActiveListByBrandIdAndTypeId(int brandId, int typeId);
//        IQueryable<MembershipCardTypeMapping> GetCloseListByBrandIdAndTypeId(int brandId, int typeId);
//    }
//    public partial class MembershipCardTypeMappingService
//    {
//        public MembershipCardTypeMapping GetByCardIdAndType(int cardId, int typeId)
//        {
//            return this.FirstOrDefault(q => q.MembershipCardId == cardId && q.MembershipTypeId == typeId);
//        }
//        public IQueryable<MembershipCardTypeMapping> GetListByCardId(int cardId)
//        {
//            return this.Get(a => a.MembershipCardId == cardId && a.Active == true);
//        }
//        public IQueryable<MembershipCardTypeMapping> GetDeactiveListByBrandIdAndTypeId(int brandId, int typeId)
//        {
//            return this.Get(q => q.MembershipCard.Status == 0 && q.MembershipCard.Active == true && q.MembershipTypeId == typeId && q.MembershipCard.BrandId == brandId);
//        }
//        public IQueryable<MembershipCardTypeMapping> GetActiveListByBrandIdAndTypeId(int brandId, int typeId)
//        {
//            return this.Get(q => q.MembershipCard.Status == 1 && q.MembershipCard.Active == true && q.MembershipTypeId == typeId && q.MembershipCard.BrandId == brandId);
//        }
//        public IQueryable<MembershipCardTypeMapping> GetCloseListByBrandIdAndTypeId(int brandId, int typeId)
//        {
//            return this.Get(q => q.MembershipCard.Status == 2 && q.MembershipCard.Active == true && q.MembershipTypeId == typeId && q.MembershipCard.BrandId == brandId);
//        }
//    }
//}
