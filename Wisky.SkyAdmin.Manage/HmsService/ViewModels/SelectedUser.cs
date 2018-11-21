using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class SelectedUser
    {
        public string username { get; set; }
        public bool selected { get; set; }
    }
    public class SelectedStore
    {
        public int ID { get; set; }
        public bool selected { get; set; }
    }
}
