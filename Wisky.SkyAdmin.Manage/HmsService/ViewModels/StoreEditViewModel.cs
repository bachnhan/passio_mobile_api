using AutoMapper;
using HmsService.Models;
using HmsService.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class StoreEditViewModel : StoreViewModel
    {
        public StoreEditViewModel() : base() { }

        public StoreEditViewModel(StoreViewModel model, IMapper mapper) : this()
        {
            mapper.Map(model, this);
        }
        public StoreEditViewModel(IEnumerable<StoreViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public IQueryable<SelectListItem> AvailableStoreType { get; set; }

        public StoreTypeEnum StoreTypeEnum { get; set; }

        [Required, MaxLength(50)]
        public override string Name
        {
            get
            {
                return base.Name;
            }

            set
            {
                base.Name = value;
            }
        }

        [Required, MaxLength(150)]
        public override string Address
        {
            get
            {
                return base.Address;
            }

            set
            {
                base.Address = value;
            }
        }

        [MaxLength(100)]
        public override string ShortName
        {
            get
            {
                return base.ShortName;
            }

            set
            {
                base.ShortName = value;
            }
        }

        public IEnumerable<SelectedMenuItem> SelectedMenu { get; set; }
    }
}

