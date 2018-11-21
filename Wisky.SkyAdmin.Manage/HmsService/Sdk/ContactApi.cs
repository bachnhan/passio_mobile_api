using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class ContactApi
    {
        public IQueryable<Contact> GetActiveContactByBrandId(int brandId)
        {
            var queryContact = this.BaseService.Get(p => p.Active == true && p.BrandId == brandId);
            return queryContact;
        }
    }
}
