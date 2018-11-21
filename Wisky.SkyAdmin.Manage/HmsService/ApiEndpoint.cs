using AutoMapper;
using HmsService.Models.Entities;
using HmsService.Models.Entities.Services;
using HmsService.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace HmsService
{
    public static class ApiEndpoint
    {

        public static void Entry(Action<IMapperConfiguration> additionalMapperConfig, params Autofac.Module[] additionalModules)
        {
            Action<IMapperConfiguration> fullMapperConfig = q =>
            {
                ApiEndpoint.ConfigAutoMapper(q);

                if (additionalMapperConfig != null)
                {
                    additionalMapperConfig(q);
                }
            };

            SkyWeb.DatVM.Mvc.Autofac.AutofacInitializer.Initialize(
                Assembly.GetExecutingAssembly(),
                typeof(HmsEntities),
                new MapperConfiguration(fullMapperConfig),
                additionalModules);
        }



        private static void ConfigAutoMapper(IMapperConfiguration config)
        {
            config.CreateMissingTypeMaps = true;

            config.CreateMap<Store, StoreViewModel>();



            config.CreateMap<Order, OrderViewModel>()
                //.ForMember(a => a.CustomerName, opt => opt.MapFrom(a => a.CustomerID.HasValue ? a.Customer.Name : ""))
                .ForMember(a => a.Store, opt => opt.MapFrom(a => a.Store));

            config.CreateMap<InventoryReceipt, InventoryReceiptViewModel>()
                .ForMember(a => a.Provider, opt => opt.MapFrom(a => a.Provider));
            config.CreateMap<InventoryReceiptItem, InventoryReceiptItemViewModel>()
                .ForMember(a => a.ProductItem, opt => opt.MapFrom(a => a.ProductItem));
            //.ForMember(a => a.Store, opt => opt.MapFrom(a => a.Store))
            //.ForMember(a => a.Store1, opt => opt.MapFrom(a => a.Store1));
            config.CreateMap<InventoryCheckingItem, InventoryCheckingItemViewModel>()
                .ForMember(a => a.InventoryChecking, opt => opt.MapFrom(a => a.InventoryChecking));
            config.CreateMap<InventoryChecking, InventoryCheckingViewModel>()
                .ForMember(a => a.InventoryCheckingItems, opt => opt.MapFrom(a => a.InventoryCheckingItems));
            //.ForMember(a => a.Store, opt => opt.MapFrom(a=> a.));

            config.CreateMap <ProductItem, ProductItemViewModel>()
                .ForMember(a => a.CateName, opt => opt.MapFrom(a => a.ProductItemCategory.CateName));

            config.CreateMap<OrderDetail, OrderDetailViewModel>()
                .ForMember(a => a.ProductImage, opt => opt.MapFrom(a => a.Product.PicURL))
                .ForMember(a => a.ProductCode, opt => opt.MapFrom(a => a.Product.Code))
                .ForMember(a => a.ProductName, opt => opt.MapFrom(a => a.Product.ProductName));

            config.CreateMap<Product, ProductViewModel>()
                .ForMember(a => a.CateName, opt => opt.MapFrom(a => a.ProductCategory.CateName))
                .ForMember(a => a.Composition, opt => opt.MapFrom(a => a.ProductItemCompositionMappings));

            config.CreateMap<ProductItemCompositionMapping, ProductItemCompositionMappingViewModel>()
                .ForMember(a => a.ProductItem, opt => opt.MapFrom(a => a.ProductItem));
            config.CreateMap<InventoryDateReport, InventoryDateReportViewModel>()
                .ForMember(a => a.InventoryDateReportItems, opt => opt.MapFrom(a => a.InventoryDateReportItems));
            config.CreateMap<ProductItem, ProductItemViewModel>()
                .ForMember(a => a.ItemCategory, opt => opt.MapFrom(a => a.ProductItemCategory))
                .ForMember(a => a.CateName, opt => opt.MapFrom(q => q.ProductItemCategory.CateName));
            //config.CreateMap<CustomerFeedback, CustomerFeedbackViewModel>();
            config.CreateMap<Product, ProductViewModel>();
            config.CreateMap<ProductImageCollectionItemMapping, ProductImageCollectionItemMappingViewModel>();
            config.CreateMap<ProductImageCollection, ProductImageCollectionDetailsViewModel>();


            config.CreateMap<ProductCategory, ProductCategoryViewModel>();

            
            // Custom config to prevent StackOverflow caused by infinite loop
            config.CreateMap<ProductCategoryTree, ProductCategoryTreeViewModel>()
                .ForMember(q => q.Category, opt => opt.MapFrom(q => q.Category))
                .ForMember(q => q.Subcategories, opt => opt.MapFrom(q => q.Subcategories));
            //config.CreateMap<PromotionDetail, PromotionDetailViewModel>()
            //    .ForMember(q => q.Promotion, opt => opt.MapFrom(q => q.PromotionCode));

            //config.CreateMap<StoreGroup, StoreGroupViewModel>()
            //    .ForMember(a => a.StoresInGroup, opt => opt.MapFrom(q => q.StoreGroupMappings.Select(a => a.Store)));

            config.CreateMap<StoreWebRoute, StoreWebRouteViewModel>();
            config.CreateMap<StoreWebRouteModel, StoreWebRouteModelViewModel>();


            config.CreateMap<TemplateReportProductItemMapping, TemplateReportProductItemMappingEditViewModel>()
                .ForMember(a => a.ItemCategory, opt => opt.MapFrom(q => q.ProductItem.ProductItemCategory.CateName))
                .ForMember(a => a.ItemName, opt => opt.MapFrom(q => q.ProductItem.ItemName));

            //config.CreateMap<TemplateReportProductItemMappingViewModel, TemplateReportProductItemMapping>()
            //    .ForMember(a => a.ProductItem.ItemName, opt => opt.MapFrom(q => q.ItemName))
            //    .ForMember(a => a.ProductItem.ProductItemCategory.CateName, opt => opt.MapFrom(q => q.ItemCategory));

            config.CreateMap<ProductCollectionItemMapping, ProductCollectionItemMappingViewModel>()
                .ForMember(a => a.ProductSpecifications, opt => opt.MapFrom(q => q.Product.ProductSpecifications));
            config.CreateMap<ProductCollection, ProductCollectionViewModel>();

            config.CreateMap<AspNetUser, AspNetUserViewModel>();
            config.CreateMap<AspNetRole, AspNetRoleViewModel>();

            config.CreateMap<AspNetUserDetails, AspNetUserDetailsViewModel>();

            config.AllowNullDestinationValues = false;
        }

    }
}
