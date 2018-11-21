using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class TransactionEditViewModel : TransactionViewModel
    {
        public TransactionEditViewModel() { }
        public TransactionEditViewModel(IEnumerable<TransactionViewModel> original, IMapper mapper)
        {
            mapper.Map(original, this);
        }
        public TransactionEditViewModel(TransactionViewModel original, IMapper mapper)
        {
            mapper.Map(original, this);
        }

        public AccountViewModel Account { get; set; }

        public IQueryable<AccountViewModel> ActiveAccounts { get; set; }
    }
}
