using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public partial class MenuViewModel
    {
        public IEnumerable<MenuViewModel> ChildrenMenus { get; set; }

        public bool isSelected { get; set; }

        public MenuViewModel(Models.Entities.Menu entity, string filter) : base(entity)
        {
            if (entity.Menu1 != null)
            {
                ChildrenMenus = entity.Menu1.Select(q => new MenuViewModel(q, filter))
                    .OrderBy(q => q.DisplayOrder).ToList();
            }
            else
            {
                ChildrenMenus = new List<MenuViewModel>();
            }

            if (entity.FeatureCode == null)
            {
                isSelected = ChildrenMenus.Count(q => q.isSelected) > 0;
            }
            else
            {
                //var check = "";
                if (FeatureCode != null)
                {
                    try
                    {
                        //check = filter[FeatureCode.Value].ToString();

                        Int64 brandFeatureFilter = Int64.Parse(filter);
                        var decimalResult = Convert.ToInt64(brandFeatureFilter & (1 << FeatureCode));
                        var decimalToBase2 = Convert.ToString(decimalResult, 2);
                        isSelected = decimalToBase2.Contains('1');
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
                //isSelected = filter == null ? true : check == "1";
                //isSelected = true;
            }
        }


    }

    public class SelectedMenuItem : SelectListItem
    {
        public SelectedMenuItem() : base() { }
        public SelectedMenuItem(MenuViewModel model) : base()
        {
            MenuFeatureCode = model.FeatureCode;
            Name = model.MenuText;
            Selected = model.isSelected;
            IdMenu = model.Id;
            ChildrenMenus = model.ChildrenMenus
                .Select(q => new SelectedMenuItem(q)).ToList();
        }
        public int? MenuFeatureCode { get; set; }
        public string Name { get; set; }
        public int IdMenu { get; set; }
        public IEnumerable<SelectedMenuItem> ChildrenMenus { get; set; }
    }
}
