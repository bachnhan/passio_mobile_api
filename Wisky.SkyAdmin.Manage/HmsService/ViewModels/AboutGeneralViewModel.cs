using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.ViewModels
{
    public class AboutGeneralViewModel
    {
        public WebPageViewModel Page { get; set; }
        public ImageCollectionDetailsViewModel Banner { get; set; }
        public BlogPostCollectionWithPostsViewModel OurTeam { get; set; }
    }
}
