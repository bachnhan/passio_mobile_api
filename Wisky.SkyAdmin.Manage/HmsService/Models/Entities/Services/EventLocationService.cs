using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IEventLocationService
    {
        IQueryable<EventLocation> GetAll();
        EventLocation GetEventById(int id);

    }

    public partial class EventLocationService
    {
        public IQueryable<EventLocation> GetAll()
        {
            IEventLocationService eventLocationService = DependencyUtils.Resolve<IEventLocationService>();
            var eventLocations = this.Get();
            return eventLocations;
        }
        public EventLocation GetEventById(int id) {
            IEventLocationService eventLocationService = DependencyUtils.Resolve<IEventLocationService>();
            var eventLocation = this.Get(e=>e.LocationId==id).FirstOrDefault();
            return eventLocation;
        }

    }
}
