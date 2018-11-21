using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public class PromotionEditViewModel : PromotionViewModel
    {
        public PromotionEditViewModel() { }

        public PromotionEditViewModel(IEnumerable<PromotionViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public PromotionEditViewModel(PromotionViewModel original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public IEnumerable<SelectListItem> AvailableStore { get; set; }

        public HttpPostedFileBase imageUpload { get; set; }

        public IQueryable<PromotionEditViewModel> AvailablePromotion { get; set; }

        public IEnumerable<SelectListItem> AvailableGroup { get; set; }

        public int[] storeArray { get; set; }
        public ReceiveValue rv { get; set; }
           
        public string FromDateStr
        {
            get
            {
                return FromDate.ToString("dd/MM/yyyy");
            }
            set
            {
                DateTime result = default(DateTime);
                var rs = DateTime.TryParseExact(value, "dd/MM/yyyy", null,
                    System.Globalization.DateTimeStyles.None, out result);
                if (rs)
                {
                    this.FromDate = result;
                }
            }
        }
        public string ToDateStr
        {
            get
            {
                return ToDate.ToString("dd/MM/yyyy");
            }
            set
            {
                DateTime result = default(DateTime);
                var rs = DateTime.TryParseExact(value, "dd/MM/yyyy", null,
                    System.Globalization.DateTimeStyles.None, out result);
                if (rs)
                {
                    this.ToDate = result;
                }
            }
        }
    }



    public class ReceiveValue
    {

    }
}
