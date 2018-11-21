using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class BrandEditViewModel : BrandViewModel
    {
        public BrandEditViewModel() : base() { }
        public BrandEditViewModel(BrandViewModel model, IMapper mapper) : this()
        {
            mapper.Map(model, this);
        }
        public BrandEditViewModel(IEnumerable<BrandViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public IEnumerable<SelectedMenuItem> SelectedMenu { get; set; }
    }
}
