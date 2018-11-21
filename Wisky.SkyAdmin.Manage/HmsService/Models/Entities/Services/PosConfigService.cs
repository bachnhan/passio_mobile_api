using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IPosConfigService
    {
        IQueryable<PosConfig> GetAllPosConfigByFileId(int fileId);
    }

    public partial class PosConfigService
    {
        public IQueryable<PosConfig> GetAllPosConfigByFileId(int fileId)
        {
            return this.GetActive().Where(q => q.PosFileId == fileId);
        }
    }
}
