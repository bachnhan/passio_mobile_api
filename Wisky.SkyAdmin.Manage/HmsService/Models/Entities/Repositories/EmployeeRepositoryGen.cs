//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace HmsService.Models.Entities.Repositories
{
    using System;
    using System.Collections.Generic;
    
    
    public partial interface IEmployeeRepository : SkyWeb.DatVM.Data.IBaseRepository<Employee>
    {
    }
    
    public partial class EmployeeRepository : SkyWeb.DatVM.Data.BaseRepository<Employee>, IEmployeeRepository
    {
    	public EmployeeRepository(System.Data.Entity.DbContext dbContext) : base(dbContext)
    	{
    	}
    }
}