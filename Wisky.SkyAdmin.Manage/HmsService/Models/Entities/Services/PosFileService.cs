using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IPosFileService
    {
        PosFile GetActivePosFileByNameAndStore(string fileName, int storeId);
    }

    public partial class PosFileService
    {
        public PosFile GetActivePosFileByNameAndStore(string fileName, int storeId)
        {
            return GetActive().FirstOrDefault(q => q.FileName.Equals(fileName) && storeId == q.StoreId);
        }
    }
}
