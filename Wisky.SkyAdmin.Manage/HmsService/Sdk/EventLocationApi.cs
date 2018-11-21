using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;

namespace HmsService.Sdk
{
    public partial class EventLocationApi
    {
        public IEnumerable<EventLocation> GetAll()
        {
            var eventLocations = this.BaseService.GetAll()
                // .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
                .ToList();
            return eventLocations;
        }
        public EventLocation GetEventById(int id)
        {
            var eventLocation = this.BaseService.GetEventById(id);
                // .ProjectTo<ProductViewModel>(this.AutoMapperConfig)
            return eventLocation;
        }
    }
}
