using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using SkyWeb.DatVM.Data;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class NotificationApi
    {
        public IQueryable<Notification> GetNotification()
        {
            return this.BaseService.Get(q => q.Active == true).OrderByDescending(q => q.UpdateDate);
        }
        public IQueryable<Notification> GetNotificationByCustomerIdAndGeneral(int customerId)
        {
            return this.BaseService.Get(q => q.Active == true &&( q.CustomerId == customerId || q.CustomerId == null)).OrderByDescending(q => q.UpdateDate);
        }
    }
}
