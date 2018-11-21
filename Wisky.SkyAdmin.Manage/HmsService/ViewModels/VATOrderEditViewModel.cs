using AutoMapper;
using HmsService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class VATOrderEditViewModel : VATOrderViewModel
    {
        public VATOrderEditViewModel() : base() { }

        public VATOrderEditViewModel(VATOrderViewModel original, IMapper mapper) : this()
        {
            mapper.Map(original, this);
        }

        public VATOrderViewModel Item { get; set; }
        public IEnumerable<ProviderViewModel> Providers { get; set; }
        public IEnumerable<SelectListItem> AvailableProvider { get; set; }
        public IEnumerable<OrderViewModel> Orders { get; set; }
        public IEnumerable<SelectListItem> AvailableOrder { get; set; }
        public string[] SelectedProviders { get; set; }
    }
}
