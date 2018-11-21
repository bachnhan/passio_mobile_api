using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class MenuEditViewModel : MenuViewModel
    {
        public MenuEditViewModel() : base() { }
        public IEnumerable<SelectListItem> DropDownForAreas { get; set; }
    }
}
