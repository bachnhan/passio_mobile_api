using HmsService.Models.Entities.Services;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class OrderCustomEntityViewModel : BaseEntityViewModel<OrderCustomEntity>
    {
        public OrderViewModel Order { get; set; }
        public IEnumerable<OrderDetailViewModel> OrderDetails { get; set; }
        public CustomerViewModel Customer { get; set; }


        public OrderCustomEntityViewModel() : base() { }
        public OrderCustomEntityViewModel(OrderCustomEntity entity) : base(entity) { }
    }
}
