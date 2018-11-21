using AutoMapper;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Wisky.SkyUp.Website.Areas.SysAdmin.Models
{

    public partial class AspNetUserEditViewModel : AspNetUserDetailsViewModel
    {

        public IEnumerable<SelectListItem> AvailableStores { get; set; }
        public IEnumerable<SelectListItem> AvailableRoles { get; set; }

        public string[] SelectedRoles { get; set; }

        public AspNetUserEditViewModel() : base() { }
        public AspNetUserEditViewModel(AspNetUserDetailsViewModel source, IMapper mapper) : this()
        {
            mapper.Map(source, this);
        }

    }

}