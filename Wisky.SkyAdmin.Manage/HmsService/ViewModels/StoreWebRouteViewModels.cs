using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{

    public partial class StoreWebRoutesViewModel
    {
        public StoreWebRouteWithModelsViewModel GlobalRoute { get; set; }
        public IEnumerable<StoreWebRouteWithModelsViewModel> StoreWebRoutes { get; set; }
    }

    public partial class StoreWebRouteWithModelsViewModel
    {
        public StoreWebRouteViewModel StoreWebRoute { get; set; }
        public IEnumerable<StoreWebRouteModelViewModel> Models { get; set; }
    }

}
