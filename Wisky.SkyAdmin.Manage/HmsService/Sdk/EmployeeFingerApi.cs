using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class EmployeeFingerApi
    {
        public EmployeeFingerViewModel GetEmployeeFinger(int empId, string enrollNumber, int fingerIndex)
        {
            var entity = this.BaseService.GetEmployeeFinger(empId, enrollNumber, fingerIndex);
            if (entity == null) return null;
            return new EmployeeFingerViewModel(entity);
        }

        public bool Add(EmployeeFingerViewModel model)
        {
            try
            {
                var entity = model.ToEntity();
                this.BaseService.Create(entity);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public bool Update(EmployeeFingerViewModel model)
        {
            try
            {
                var entity = model.ToEntity();
                this.BaseService.Update(entity);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }
    }
}
