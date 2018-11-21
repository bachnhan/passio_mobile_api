using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public partial class AccountViewModel
    {
        public CustomerViewModel Customer { get; set; }
        public int addBalanceValue { get; set; }
    }
}
