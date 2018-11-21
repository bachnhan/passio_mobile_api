using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class CustomerTypeEditViewModel: CustomerTypeViewModel
    {

        public CustomerTypeEditViewModel() { }

        public CustomerTypeEditViewModel(IEnumerable<CustomerTypeViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public CustomerTypeEditViewModel(CustomerTypeViewModel original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public string CustomerType { get; set; }
        public IEnumerable<SelectListItem> AvailableCustomerType { get; set; }
        

    }
}
