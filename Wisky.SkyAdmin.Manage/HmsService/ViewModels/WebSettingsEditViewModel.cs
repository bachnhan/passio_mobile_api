using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HmsService.ViewModels
{

    public class StoreWebSettingsEditViewModel
    {

        public StoreWebSettingPair[] Pairs { get; set; }

    }

    public class StoreWebSettingPair
    {
        public int Id { get; set; }
        public string Value { get; set; }
    }

}