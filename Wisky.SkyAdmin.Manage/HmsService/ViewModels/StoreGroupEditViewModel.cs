using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class StoreGroupEditViewModel : StoreGroupViewModel
    {
        public StoreGroupEditViewModel() : base() { }
        public StoreGroupEditViewModel(StoreGroupViewModel model, IMapper mapper) : this()
        {
            mapper.Map(model, this);
        }
        public StoreGroupEditViewModel(IEnumerable<StoreGroupViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
    }
}
