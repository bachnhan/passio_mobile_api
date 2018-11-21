using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface ICustomerFeedbackService
    {
        IQueryable<CustomerFeedback> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty);
        System.Threading.Tasks.Task<CustomerFeedback> GetActiveByStoreAsync(int id, int storeId);
    }

    public partial class CustomerFeedbackService
    {
        public IQueryable<CustomerFeedback> GetAdminByStoreWithFilter(int storeId, string keyword, KeyValuePair<string, bool> orderByProperty)
        {

            var result = this.GetActive(q =>
                q.StoreId == storeId &&
                (keyword == null || q.Title.Contains(keyword)));

            CustomerFeedbackSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case CustomerFeedbackSortableProperty.Id:
                        result = result.OrderBy(q => q.Id, orderByProperty.Value);
                        break;
                    case CustomerFeedbackSortableProperty.Title:
                        result = result.OrderBy(q => q.Title, orderByProperty.Value);
                        break;
                    case CustomerFeedbackSortableProperty.Fullname:
                        result = result.OrderBy(q => q.Fullname, orderByProperty.Value);
                        break;
                    case CustomerFeedbackSortableProperty.Email:
                        result = result.OrderBy(q => q.Email, orderByProperty.Value);
                        break;
                    case CustomerFeedbackSortableProperty.Phone:
                        result = result.OrderBy(q => q.Phone, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                result = result.OrderByDescending(q => q.Id);
            }

            return result.OrderByDescending(q => q.Id);
        }

        public async System.Threading.Tasks.Task<CustomerFeedback> GetActiveByStoreAsync(int id, int storeId)
        {
            return await this.FirstOrDefaultActiveAsync(q => q.Id == id && q.StoreId == storeId);
        }

    }

    public enum CustomerFeedbackSortableProperty
    {
        Id, Title, Fullname, Email, Phone
    }

}
