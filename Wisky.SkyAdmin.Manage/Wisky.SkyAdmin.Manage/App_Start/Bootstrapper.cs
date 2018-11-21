using System.Web.Mvc;
using Autofac;
using System.Reflection;
using Autofac.Integration.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using SkyWeb.DatVM.Data;
using Wisky.SkyAdmin.Manage.Models.Identity;
using HmsService.Models.Entities.Services;
using HmsService.Models.Entities.Repositories;
using Wisky.SkyAdmin.Manage.Infrastructure;
using Wiksy.SkyAdmin.Manage.Mappings;

namespace Wisky.SkyAdmin.Manage
{
    public static class Bootstrapper
    {
        public static void Run()
        {
            SetAutofacContainer();
            //Configure AutoMapper
            AutoMapperConfiguration.Configure();

        }

        private static void SetAutofacContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterControllers(Assembly.GetExecutingAssembly());
            builder.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerHttpRequest();
            builder.RegisterType<DatabaseFactory>().As<IDatabaseFactory>().InstancePerHttpRequest();
            builder.RegisterAssemblyTypes(typeof (RoomRepository).Assembly)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces().InstancePerHttpRequest();
            builder.RegisterAssemblyTypes(typeof (RoomService).Assembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces().InstancePerHttpRequest();
            builder.Register(c => new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext())))
               .As<UserManager<ApplicationUser>>().InstancePerHttpRequest();

            //builder.RegisterAssemblyTypes(typeof (DefaultFormsAuthentication).Assembly)
            //    .Where(t => t.Name.EndsWith("Authentication"))
            //    .AsImplementedInterfaces().InstancePerHttpRequest();

            //builder.Register(c => new UserManager<ApplicationUser>(new UserStore<ApplicationUser>( new SocialGoalEntities())))
            //    .As<UserManager<ApplicationUser>>().InstancePerHttpRequest();
            

            builder.RegisterFilterProvider();
            IContainer container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));


            var builder2 = new ContainerBuilder();
            builder2.RegisterType<UnitOfWork>().As<IUnitOfWork>().InstancePerLifetimeScope();
            builder2.RegisterType<DatabaseFactory>().As<IDatabaseFactory>().InstancePerLifetimeScope();
            builder2.RegisterAssemblyTypes(typeof(RoomRepository).Assembly)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder2.RegisterAssemblyTypes(typeof(RoomService).Assembly)
                .Where(t => t.Name.EndsWith("Service"))
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder2.RegisterType<OrderService>().As<IOrderService>().InstancePerLifetimeScope();
        }
    }
}