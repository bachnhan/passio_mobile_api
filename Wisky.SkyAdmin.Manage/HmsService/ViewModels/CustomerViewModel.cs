using HmsService.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace HmsService.ViewModels
{
    public partial class CustomerViewModel
    {
        public IEnumerable<SelectListItem> AvailableAccounts { get; set; }

        public IEnumerable<SelectListItem> AllAccounts { get; set; }

        public AccountViewModel DefaultAccount { get; set; }

        public bool ContainAccount { get; set; }

        public IEnumerable<MembershipCard> MembershipCards { get; set; }

        public string birthDayString { get; set; }
        // Level Up Card
        public int currentCardToLevelUp { get; set; }
        public bool? isAvailable { get; set; }
        public string nextLevel { get; set; }
        public bool isHighest { get; set; }

        // Count cards and accounts
        public int creditCount { get; set; }
        public int giftCount { get; set; }
        public int memberCount { get; set; }
        public int unknownCount { get; set; }
    }
}
