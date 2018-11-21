using Autofac;
using HmsService.Models;
using SkyWeb.DatVM.Data;

namespace SmileCardUpdateBarCode.Configuration
{
    public static class Bootstrapper
    {
        public static void Run()
        {
            SetAutofacContainer();
        }

        private static void SetAutofacContainer()
        {
            var builder2 = new ContainerBuilder();
            builder2.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
            builder2.RegisterType<DatabaseFactory>().As<IDatabaseFactory>().InstancePerLifetimeScope();
            builder2.RegisterAssemblyTypes(typeof(RoomRepository).Assembly)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder2.RegisterAssemblyTypes(typeof(RoomService).Assembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder2.RegisterType<RentService>().As<IRentService>().InstancePerLifetimeScope();
            HmsDependencyResolver.Container = builder2.Build();
        }
    }
}