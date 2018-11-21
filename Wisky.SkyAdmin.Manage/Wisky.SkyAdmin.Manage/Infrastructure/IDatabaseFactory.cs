using HmsService.Models.Entities;
using System;

namespace Wisky.SkyAdmin.Manage.Infrastructure
{
    public interface IDatabaseFactory : IDisposable
    {
        HmsEntities Get();
    }
}
