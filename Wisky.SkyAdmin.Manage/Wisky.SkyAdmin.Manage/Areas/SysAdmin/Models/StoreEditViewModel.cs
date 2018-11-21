using System.ComponentModel.DataAnnotations;
using AutoMapper;
using HmsService.ViewModels;

namespace Wisky.SkyAdmin.Manage.Areas.SysAdmin.Models
{

    public class StoreEditViewModel : StoreViewModel
    {

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

        [Required, MaxLength(100)]
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

        public StoreEditViewModel() : base() { }

        public StoreEditViewModel(StoreViewModel model, IMapper mapper) : this()
        {
            mapper.Map(model, this);
        }

    }

}