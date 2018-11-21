using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public partial class ProductItemCategoryViewModel
    {
        public IEnumerable<SelectList> AvailbleProductCategories { get; set; }
    }
}
