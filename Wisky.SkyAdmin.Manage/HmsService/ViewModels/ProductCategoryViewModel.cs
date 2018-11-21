using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
   public partial class ProductCategoryViewModel
    {
        public ICollection<Product> Products { get; set; }
    }
}
