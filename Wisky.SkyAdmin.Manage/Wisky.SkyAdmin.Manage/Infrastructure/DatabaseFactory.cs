using System;
using HmsService.Models.Entities;

namespace Wisky.SkyAdmin.Manage.Infrastructure
{
public class DatabaseFactory : Disposable, IDatabaseFactory
{
    private HmsEntities dataContext;
    public HmsEntities Get()
    {
        var entity = new HmsEntities();
        //entity.Configuration.LazyLoadingEnabled = false;
        return dataContext ?? (dataContext = entity);
    }
    protected override void DisposeCore()
    {
        if (dataContext != null)
            dataContext.Dispose();
    }
}
}
