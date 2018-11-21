using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{

    public partial interface IGroupService
    {
        IQueryable<Group> GetGroup();
        Group GetGroupById(int id);

    }
    public partial class  GroupService
    {
        public IQueryable<Group> GetGroup()
        {
            return this.Get(q => q.IsDisplayed == true);

        }

        public Group GetGroupById(int id)
        {
            return this.FirstOrDefault(q => q.GroupId == id);
        }
    }
}
