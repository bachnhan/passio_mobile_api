using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class PosFileApi
    {
        public PosFileViewModel GetActivePosFileByNameAndStore(string fileName, int storeId)
        {
            var entity = this.BaseService.GetActivePosFileByNameAndStore(fileName, storeId);
            return entity == null ? null : new PosFileViewModel(entity);
        }

        public PosFile GetFileEntityByFileName(string fileName, int storeId)
        {
            return BaseService.GetActivePosFileByNameAndStore(fileName, storeId);

        }

        public int CreateAndReturnId(PosFileViewModel model)
        {
            try
            {
                var entity = model.ToEntity();
                this.BaseService.Create(entity);

                return entity.Id;
            }
            catch (Exception e)
            {
                return -1;
            }
        }

        public bool UpdateAndReturnSuccess(PosFileViewModel model)
        {
            try
            {
                var entity = model.ToEntity();
                this.BaseService.Update(entity);

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public bool UpdatePostFileWithAllConfigs(PosFileViewModel model)
        {
            if (DeleteAllConfigAndReturnSuccess(model.Id))
            {
                try
                {
                    this.Edit(model.Id, model);
                    return true;
                } catch (Exception e)
                {
                    return false;
                }
            }
            else return false;
        }

        public bool DeleteAllConfigAndReturnSuccess(int posFileId)
        {
            try
            {
                var posConfigApi = new PosConfigApi();
                var configList = posConfigApi.GetAllPosConfigByFileId(posFileId);

                foreach(var config in configList)
                {
                    posConfigApi.Delete(config.Id);
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
