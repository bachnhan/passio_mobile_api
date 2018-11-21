using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class AccountEditViewModel : AccountViewModel
    {
        

        public AccountEditViewModel() { }
        public AccountEditViewModel(IEnumerable<AccountViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public AccountEditViewModel(AccountViewModel original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public IEnumerable<SelectListItem> AvailableCustomer { get; set; }
        public bool isServer { get; set; }
        public int ActiveTime; //thoi gian hoat dong
        public String ProductName { get; set; }
    }
}
