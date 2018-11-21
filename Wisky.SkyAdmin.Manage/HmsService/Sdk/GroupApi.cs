using AutoMapper.QueryableExtensions;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class GroupApi
    {
        public IEnumerable<GroupViewModel> GetGroupActive()
        {
            var groups = (this.BaseService.GetGroup()
                .Where(q => q.IsDisplayed == true))
                .ProjectTo<GroupViewModel>(this.AutoMapperConfig)
                .ToList();
            return groups;
        }
        public async System.Threading.Tasks.Task CreateGroup(GroupViewModel model)
        {
            var entity = new Group();
            model.IsDisplayed = true;
            entity = model.ToEntity();
            await this.BaseService.CreateAsync(entity);
        }
        public async System.Threading.Tasks.Task UpdateGroupAsync(GroupViewModel model)
        {
            var entity = await this.BaseService.GetAsync(model.GroupId);
            //entity.Id = model.Id;
            entity.Name = model.Name;
            entity.Description = model.Description;            
            await this.BaseService.UpdateAsync(entity);
        }
        public async System.Threading.Tasks.Task DeleteGroupAsync(GroupViewModel model)
        {
            model.IsDisplayed = false;
            await this.EditAsync(model.GroupId, model);
        }
        public GroupViewModel GetGroupById(int id)
        {
            var model = GetGroupActive().FirstOrDefault(c => c.GroupId == id);
            return model;
        }
    }
}
