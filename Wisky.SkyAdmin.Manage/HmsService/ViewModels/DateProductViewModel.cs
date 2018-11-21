using HmsService.Models.Entities;
using SkyWeb.DatVM.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    #region SystemReport
    public partial class DateProductViewModel: BaseEntityViewModel<DateProduct>
    {
        public ProductViewModel Product { get; set; }
    }
    #endregion
}
