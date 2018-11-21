using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class RatingProductApi
    {
        public RatingProduct GetRatingProductByUserAndProductId(string userId, int ProductId)
        {
            return this.BaseService.Get(q => q.UserId == userId && q.ProductId == ProductId).FirstOrDefault();
        }
        public void EditRating(RatingProduct model)
        {
            this.BaseService.Update(model);
        }
        public RatingProduct GetByCommentId(int CommentId)
        {
            return this.BaseService.Get(q => q.Id == CommentId).FirstOrDefault();
        }
        public void Check(RatingProduct model)
        {
            model.Active = true;
            this.BaseService.Update(model);
        }
        public void unCheck(RatingProduct model)
        {
            model.Active = false;
            this.BaseService.Update(model);
        }
        public void DeleteCmt(RatingProduct model)
        {
            this.BaseService.Delete(model);
        }
        public bool CreateRatingProduct(RatingProduct model)
        {
            try
            {
                this.BaseService.Create(model);
                return true;
            }
            catch (Exception e)
            {
                return false;
            }

        }
    }
}
