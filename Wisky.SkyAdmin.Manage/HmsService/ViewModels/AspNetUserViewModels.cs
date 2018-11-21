using HmsService.Models.Entities.Services;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{

    public class AspNetUserDetailsViewModel : BaseEntityViewModel<AspNetUserDetails>
    {

        public AspNetUserViewModel AspNetUser { get; set; }
        public IEnumerable<AspNetRoleViewModel> Roles { get; set; }
        public int? AdminStoreId { get; set; }
        public string AdminStoreName { get; set; }

        public AspNetUserDetailsViewModel() : base() { }
        public AspNetUserDetailsViewModel(AspNetUserDetails entity) : base(entity) { }

    }

}
