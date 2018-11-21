using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class AccountApiViewModel
    {
        public string AccountId { get; set; }
        public string AccountPassword { get; set; }
        public string StaffName { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
        public int StoreId { get; set; }
        public bool IsUsed { get; set; }
    }

    public class LoginAccountApiViewModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
