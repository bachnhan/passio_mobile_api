using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public partial class AspNetUserEditViewModel : AspNetUserDetailsViewModel
    {

        public IEnumerable<SelectListItem> AvailableStores { get; set; }
        public IEnumerable<SelectListItem> AvailableRoles { get; set; }
        public IEnumerable<SelectListItem> AvailableBrands { get; set; }

        public string[] SelectedRoles { get; set; }

        public AspNetUserEditViewModel() : base() { }
        public AspNetUserEditViewModel(AspNetUserDetailsViewModel source, IMapper mapper) : this()
        {
            mapper.Map(source, this);
        }

    }
}
