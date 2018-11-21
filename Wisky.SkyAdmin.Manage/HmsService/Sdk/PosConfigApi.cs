using AutoMapper;
using AutoMapper.QueryableExtensions;
using HmsService.Models;
using HmsService.Models.Entities;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Sdk
{
    public partial class PosConfigApi
    {
        public List<PosConfigViewModel> GetAllPosConfigByFileId(int fileId)
        {
            return this.BaseService.GetAllPosConfigByFileId(fileId)
                .ProjectTo<PosConfigViewModel>(this.AutoMapperConfig)
                .ToList();
        }
    }
}
