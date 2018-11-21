using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using HmsService.Models;
using System.Data.Entity.Validation;
using SkyWeb.DatVM.Data;
using HmsService.Models.Entities.Repositories;
using SkyWeb.DatVM.Mvc.Autofac;

namespace HmsService.Models.Entities.Services
{

    public partial interface IProductService
    {
        Task<ProductDetails> GetBySeoNameAsync(string seoName, int brandId);
        IQueryable<Product> GetAvailableByProductCategoryId(int categoryId);

        IQueryable<Product> GetAvailableByProductCategoryAndPattern(int categoryId, int storeId, string pattern);
        IQueryable<Product> GetActiveByStoreId(int storeId);
        IQueryable<Product> GetActiveDisplayByStoreId(int storeId);
        IQueryable<Product> GetActiveWithoutExtraByStoreId(int storeId);
        IQueryable<ProductDetails> GetActiveWithSpecsByStoreId(int storeId);
        IQueryable<ProductDetails> GetAllProductWithoutExtraByStoreId(int storeId);
        IQueryable<Product> GetByStoreId(int storeId);
        IQueryable<Product> GetByProductCategoryId(int categoryId);
        IQueryable<Product> GetAdminByStoreWithFilter(int brandId, string keyword, KeyValuePair<string, bool> orderByProperty);
        IQueryable<Product> GetProductByBrand(int brandId, string keyword);
        IQueryable<Product> GetProductByBrand(int brandId);
        IQueryable<Product> GetAllProductByBrand(int brandId);
        IQueryable<Product> GetAllProductsByBrand(int brandId);
        IQueryable<Product> GetProductExtraByBrandId(int brandId);

        System.Threading.Tasks.Task CreateAsync(Product entity, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs);
        int CreateSync(Product entity, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs, IEnumerable<int> listStoreId);
        System.Threading.Tasks.Task EditAsync(Product entity, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs);
        void EditSync(Product entity, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs);
        void EditSync(Product entity, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs, int[] combos);
        Task<Product> GetActiveByStoreAsync(int id, int storeId);
        Task<ProductDetails> GetActiveDetailsByStoreAsync(int id, int storeId);

        IQueryable<Product> GetLikelyProducts(string seoName, int storeId);
        IQueryable<Product> GetAllProductGeneral(int productId);

        Product GetProductGeneral(int productId);
        ProductDetails GetActiveDetailsByBrand(int id, int brandId);
        Task<ProductDetails> GetBySeoNameAsyncInBrand(string seoName, int brandId);
        IQueryable<Product> GetActiveByBrandId(int brandId);
        IQueryable<Product> GetAdminByBrandWithFilter(int brandId, string keyword, KeyValuePair<string, bool> orderByProperty);
        IQueryable<Product> GetLikelyProductsInBrand(string seoName, int brandId);
        IQueryable<ProductDetails> GetActiveWithSpecsByBrandId(int brandId);
        IQueryable<Product> GetAvailableByProductCategoryAndPatternInBrand(int categoryId, int brandId, string pattern);
        IQueryable<Product> GetByProductCategoryAndPatternInBrand(int categoryId, int brandId, string pattern);
        IEnumerable<Product> GetActiveProductsEntitybyBrandId(int brandId);

        #region Store Report
        IQueryable<Product> GetProducts();
        IQueryable<Product> GetAllProducts();
        IQueryable<ProductForComparisonReport> GetAllStoreActiveProductsForReport(int storeId);
        #endregion

        IQueryable<Product> GetProductOfCategoryBySeoName(string seoName, int storeId);
        IQueryable<Product> GetChildProductGeneral(int productId);
        Product GetProductById(int productId);
        Product GetProductByCode(string code);
        Product GetProductBySeo(string seo);
        IQueryable<Product> GetActiveByBrandIdAndCateId(int storeId, int cateId);
        IQueryable<ProductDetails> GetProductDetailsOfCategoryBySeoName(string seoName, int storeId); // author: DucBM
        IQueryable<Product> GetGeneralProductByBrandId(int brandId);
        void EditSeoName();

        IQueryable<ProductForComparisonReport> GetAllBrandActiveProductsForReport(int brandId);

    }

    public partial class ProductService
    {
        #region By Store
        public Product GetProductById(int productId)
        {
            return this.Get(productId);
        }
        public Product GetProductByCode(string code)
        {
            return this.FirstOrDefault(q => q.Code == code && q.Active);
        }
        public Product GetProductBySeo(string seo)
        {
            return this.FirstOrDefault(q => q.SeoName == seo && q.Active);
        }
        public async Task<ProductDetails> GetBySeoNameAsync(string seoName, int brandId)
        {
            var product = await this.GetActive(q => q.IsAvailable == true && q.SeoName == seoName && q.ProductCategory.BrandId == brandId)
                .FirstOrDefaultAsync();
            if (product == null)
            {
                return null;
            }
            else
            {
                return new ProductDetails()
                {
                    Product = product,
                    ProductCollections = product.ProductCollectionItemMappings
                        .Where(q => q.Active && q.ProductCollection.Active),
                    ProductImages = product.ProductImages.Where(a => a.Active),
                    ProductSpecifications = product.ProductSpecifications.Where(a => a.Active)
                };
            }
        }

        public IQueryable<Product> GetAvailableByProductCategoryId(int categoryId)
        {
            var products = this
                .GetActive(q => q.CatID == categoryId && q.IsAvailable == true)
                .OrderBy(q => q.Position);
            return products;
        }

        public IQueryable<Product> GetActiveByStoreId(int storeId)
        {
            var productDetailMappingService = DependencyUtils.Resolve<IProductDetailMappingService>();
            var products = productDetailMappingService.GetProductByStore(storeId).Select(q => q.Product);
            return products.Where(q => q.Active == true);
        }
        public IQueryable<Product> GetActiveDisplayByStoreId(int storeId)
        {
            var productDetailMappingService = DependencyUtils.Resolve<IProductDetailMappingService>();
            var products = productDetailMappingService.GetProductByStore(storeId).Select(q => q.Product);
            return products.Where(q => q.Active == true && q.ProductCategory.IsDisplayedWebsite == true && q.ProductCategory.IsExtra == false);
        }
        public IQueryable<Product> GetActiveWithoutExtraByStoreId(int storeId)
        {
            var productDetailMappingService = DependencyUtils.Resolve<IProductDetailMappingService>();
            var products = productDetailMappingService.GetProductByStore(storeId).Select(q => q.Product);
            return products.Where(q => q.Active == true && q.HasExtra == true);
        }

        public IQueryable<Product> GetAvailableProductByStoreId(int storeId)
        {
            return GetActiveByStoreId(storeId).Where(q => q.IsAvailable == true);
        }

        public IQueryable<Product> GetProductExtraByBrandId(int brandId)
        {
            return this.GetActive(q => q.ProductCategory.Active == true
                                      && q.ProductCategory.BrandId == brandId
                                      && q.ProductCategory.IsExtra == true);
        }

        public IQueryable<ProductDetails> GetActiveWithSpecsByStoreId(int storeId)
        {
            var products = this.GetActiveByStoreId(storeId);

            return products.Select(a => new ProductDetails
            {
                Product = a,
                ProductCollections = a.ProductCollectionItemMappings,
                ProductSpecifications = a.ProductSpecifications.Where(b => b.Active),
                ProductImages = a.ProductImages
            });
        }

        public IQueryable<ProductDetails> GetAllProductWithoutExtraByStoreId(int storeId)
        {
            var products = this.GetActiveWithoutExtraByStoreId(storeId);

            return products.Select(a => new ProductDetails
            {
                Product = a,
                ProductCollections = a.ProductCollectionItemMappings,
                ProductSpecifications = a.ProductSpecifications.Where(b => b.Active),
                ProductImages = a.ProductImages
            });
        }

        public IQueryable<Product> GetByStoreId(int storeId)
        {
            var products = this
                .GetActive(q => q.ProductCategory.StoreId == storeId)
                .OrderBy(q => q.Position);
            return products;
        }

        public IQueryable<Product> GetByProductCategoryId(int categoryId)
        {
            var products = this
                .GetActive(q => q.CatID == categoryId)
                .OrderBy(q => q.Position);
            return products;
        }

        public IQueryable<Product> GetAdminByStoreWithFilter(int brandId, string keyword, KeyValuePair<string, bool> orderByProperty)
        {
            var result = this.GetActive(q =>
                q.ProductCategory.BrandId == brandId && q.IsAvailable == true &&
                (keyword == null || q.ProductName.Contains(keyword) || q.ProductCategory.CateName.Contains(keyword)));

            ProductSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case ProductSortableProperty.ProductID:
                        result = result.OrderBy(q => q.ProductID, orderByProperty.Value);
                        break;
                    case ProductSortableProperty.Code:
                        result = result.OrderBy(q => q.Code, orderByProperty.Value);
                        break;
                    case ProductSortableProperty.ProductName:
                        result = result.OrderBy(q => q.ProductName, orderByProperty.Value);
                        break;
                    case ProductSortableProperty.ProductCategory:
                        result = result.OrderBy(q => q.ProductCategory.CateName, orderByProperty.Value);
                        break;
                    case ProductSortableProperty.IsAvailable:
                        result = result.OrderBy(q => q.IsAvailable, orderByProperty.Value);
                        break;
                    case ProductSortableProperty.Position:
                        result = result.OrderBy(q => q.Position, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                //result = result.OrderBy(q => q.Position).ThenBy(q=> q.CreateTime);
                result = result.OrderBy(q => q.Position).ThenByDescending(q => q.CreateTime);
            }

            return result;
        }

        public IQueryable<Product> GetProductByBrand(int brandId, string keyword)
        {
            var result = this.GetActive(q =>
                q.ProductCategory.BrandId == brandId &&
                (keyword == null || q.ProductName.Contains(keyword) || q.ProductCategory.CateName.Contains(keyword)));

            return result;
        }

        public IQueryable<Product> GetProductByBrand(int brandId)
        {
            var result = this.GetActive(q =>
                q.ProductCategory.BrandId == brandId && q.IsAvailable && q.ProductCategory.Active == true);
            return result;
        }

        public IQueryable<Product> GetAllProductByBrand(int brandId)
        {
            var result = this.Get(q =>
                q.ProductCategory.BrandId == brandId);
            return result;
        }

        public IQueryable<Product> GetAllProductsByBrand(int brandId)
        {
            var result = this.Get(q =>
                q.ProductCategory.BrandId == brandId && q.ProductCategory.Active && q.Active);
            return result;
        }

        public IQueryable<Product> GetActiveByBrandIdAndCateId(int brandId, int cateId)
        {
            var products = this
                .GetActive(q => q.ProductCategory.BrandId == brandId && (q.ProductCategory.CateID == cateId || cateId == 0) && q.IsAvailable == true)
                .OrderBy(q => q.Position);
            return products;
        }

        public int CreateSync(Product entity, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs, IEnumerable<int> listStoreId)
        {
            entity.Active = true;
            this.repository.Add(entity);

            // Images
            var productImages = entity.ProductImages.ToArray();
            productImages.UpdateList(images, (imageEntity, data, position, requireAddNew) =>
            {
                imageEntity.ImageUrl = data;

                if (requireAddNew)
                {
                    entity.ProductImages.Add(imageEntity);
                }
            });

            // Specifications
            var productSpecs = entity.ProductSpecifications.ToArray();
            productSpecs.UpdateList(specs, (specEntity, data, position, requireAddNew) =>
            {
                specEntity.Name = data.Key;
                specEntity.Value = data.Value;

                if (requireAddNew)
                {
                    entity.ProductSpecifications.Add(specEntity);
                }
            });

            // Collections
            foreach (var blogPostCollectionId in productCollectionIds)
            {
                var connector = new ProductCollectionItemMapping()
                {
                    ProductCollectionId = blogPostCollectionId,
                    Active = true,
                };

                entity.ProductCollectionItemMappings.Add(connector);
            }

            foreach (var item in listStoreId)
            {
                var mapping = new ProductDetailMapping()
                {
                    DiscountPercent = entity.DiscountPercent,
                    DiscountPrice = entity.DiscountPrice,
                    Price = entity.Price,
                    Active = true,
                    StoreID = item,
                };
                entity.ProductDetailMappings.Add(mapping);
                entity.DiscountPrice = Math.Round(entity.Price * entity.DiscountPercent / 100);
            }

            try
            {
                this.Save();

                return entity.ProductID;
            }
            catch (DbEntityValidationException ex)
            {
                throw new DbEntityValidationException(ex.ToErrorsString(), ex);
            }
        }

        public async System.Threading.Tasks.Task CreateAsync(Product entity, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs)
        {
            entity.Active = true;
            this.repository.Add(entity);

            // Images
            var productImages = entity.ProductImages.ToArray();
            productImages.UpdateList(images, (imageEntity, data, position, requireAddNew) =>
            {
                imageEntity.ImageUrl = data;

                if (requireAddNew)
                {
                    entity.ProductImages.Add(imageEntity);
                }
            });

            // Specifications
            var productSpecs = entity.ProductSpecifications.ToArray();
            productSpecs.UpdateList(specs, (specEntity, data, position, requireAddNew) =>
            {
                specEntity.Name = data.Key;
                specEntity.Value = data.Value;

                if (requireAddNew)
                {
                    entity.ProductSpecifications.Add(specEntity);
                }
            });

            // Collections
            foreach (var blogPostCollectionId in productCollectionIds)
            {
                var connector = new ProductCollectionItemMapping()
                {
                    ProductCollectionId = blogPostCollectionId,
                    Active = true,
                };

                entity.ProductCollectionItemMappings.Add(connector);
            }

            try
            {
                await this.SaveAsync();
            }
            catch (DbEntityValidationException ex)
            {
                throw new DbEntityValidationException(ex.ToErrorsString(), ex);
            }

        }

        public async System.Threading.Tasks.Task EditAsync(Product entity, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs)
        {
            try
            {
                // Images
                var productImages = entity.ProductImages.ToArray();
                productImages.UpdateList(images, (imageEntity, data, position, requireAddNew) =>
                {
                    imageEntity.ImageUrl = data;

                    if (requireAddNew)
                    {
                        entity.ProductImages.Add(imageEntity);
                    }
                });

                // Specifications
                var productSpecs = entity.ProductSpecifications.ToArray();
                productSpecs.UpdateList(specs, (specEntity, data, position, requireAddNew) =>
                {
                    specEntity.Name = data.Key;
                    specEntity.Value = data.Value;

                    if (requireAddNew)
                    {
                        entity.ProductSpecifications.Add(specEntity);
                    }
                });


                // Collections
                var productCollectionItems = entity.ProductCollectionItemMappings.ToArray();

                foreach (var productCollectionId in productCollectionIds)
                {
                    var connector = productCollectionItems
                        .FirstOrDefault(q => q.ProductCollectionId == productCollectionId);

                    if (connector == null)
                    {
                        connector = new ProductCollectionItemMapping()
                        {
                            ProductCollectionId = productCollectionId,
                        };

                        entity.ProductCollectionItemMappings.Add(connector);
                    }

                    connector.Active = true;
                }

                foreach (var productCollectionItem in productCollectionItems)
                {
                    if (!productCollectionIds.Contains(productCollectionItem.ProductCollectionId))
                    {
                        productCollectionItem.Active = false;
                    }
                }


                // Update and persist
                await this.UpdateAsync(entity);
            }
            catch (DbEntityValidationException ex)
            {
                throw new DbEntityValidationException(ex.ToErrorsString(), ex);
            }

        }

        public void EditSync(Product entity, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs)
        {
            try
            {
                // Images
                var productImages = entity.ProductImages.ToArray();
                productImages.UpdateList(images, (imageEntity, data, position, requireAddNew) =>
                {
                    imageEntity.ImageUrl = data;

                    if (requireAddNew)
                    {
                        entity.ProductImages.Add(imageEntity);
                    }
                });

                // Specifications
                var productSpecs = entity.ProductSpecifications.ToArray();
                productSpecs.UpdateList(specs, (specEntity, data, position, requireAddNew) =>
                {
                    specEntity.Name = data.Key;
                    specEntity.Value = data.Value;

                    if (requireAddNew)
                    {
                        entity.ProductSpecifications.Add(specEntity);
                    }
                });


                // Collections
                var productCollectionItems = entity.ProductCollectionItemMappings.ToArray();

                foreach (var productCollectionId in productCollectionIds)
                {
                    var connector = productCollectionItems
                        .FirstOrDefault(q => q.ProductCollectionId == productCollectionId);

                    if (connector == null)
                    {
                        connector = new ProductCollectionItemMapping()
                        {
                            ProductCollectionId = productCollectionId,
                        };

                        entity.ProductCollectionItemMappings.Add(connector);
                    }

                    connector.Active = true;
                }

                foreach (var productCollectionItem in productCollectionItems)
                {
                    if (!productCollectionIds.Contains(productCollectionItem.ProductCollectionId))
                    {
                        productCollectionItem.Active = false;
                    }
                }


                // Update and persist
                this.Update(entity);
            }
            catch (DbEntityValidationException ex)
            {
                throw new DbEntityValidationException(ex.ToErrorsString(), ex);
            }

        }

        public void EditSync(Product entity, string[] images, int[] productCollectionIds, KeyValuePair<string, string>[] specs, int[] combos)
        {
            try
            {
                // Images
                var productImages = entity.ProductImages.ToArray();
                productImages.UpdateList(images, (imageEntity, data, position, requireAddNew) =>
                        {
                            imageEntity.ImageUrl = data;

                            if (requireAddNew)
                            {
                                entity.ProductImages.Add(imageEntity);
                            }
                        });

                // Specifications
                var productSpecs = entity.ProductSpecifications.ToArray();
                productSpecs.UpdateList(specs, (specEntity, data, position, requireAddNew) =>
                                {
                                    specEntity.Name = data.Key;
                                    specEntity.Value = data.Value;

                                    if (requireAddNew)
                                    {
                                        entity.ProductSpecifications.Add(specEntity);
                                    }
                                });

                //Combos
                var productCombos = entity.ProductComboDetails.ToArray();
                productCombos.UpdateList(combos, (comboEntity, data, position, requireAddNew) =>
                {
                    comboEntity.Quantity = data;

                    if (requireAddNew)
                    {
                        entity.ProductComboDetails.Add(comboEntity);
                    }
                });
                // Collections
                var productCollectionItems = entity.ProductCollectionItemMappings.ToArray();

                foreach (var productCollectionId in productCollectionIds)
                {
                    var connector = productCollectionItems
                        .FirstOrDefault(q => q.ProductCollectionId == productCollectionId);

                    if (connector == null)
                    {
                        connector = new ProductCollectionItemMapping()
                        {
                            ProductCollectionId = productCollectionId,
                        };

                        entity.ProductCollectionItemMappings.Add(connector);
                    }

                    connector.Active = true;
                }

                foreach (var productCollectionItem in productCollectionItems)
                {
                    if (!productCollectionIds.Contains(productCollectionItem.ProductCollectionId))
                    {
                        productCollectionItem.Active = false;
                    }
                }


                // Update and persist
                this.Update(entity);
            }
            catch (DbEntityValidationException ex)
            {
                throw new DbEntityValidationException(ex.ToErrorsString(), ex);
            }

        }

        public async Task<Product> GetActiveByStoreAsync(int id, int storeId)
        {
            return await this.FirstOrDefaultActiveAsync(q => q.ProductID == id && q.ProductCategory.StoreId == storeId);
        }

        public async Task<ProductDetails> GetActiveDetailsByStoreAsync(int id, int storeId)
        {
            var result = await this.GetActive(q => q.ProductID == id && q.ProductCategory.StoreId == storeId)
                .FirstOrDefaultAsync();

            if (result == null)
            {
                return null;
            }
            else
            {
                return new ProductDetails()
                {
                    Product = result,
                    ProductCollections = result.ProductCollectionItemMappings
                        .Where(q => q.Active && q.ProductCollection.Active),
                    ProductImages = result.ProductImages.Where(a => a.Active),
                    ProductSpecifications = result.ProductSpecifications.Where(a => a.Active)
                };
            }
        }

        public ProductDetails GetActiveDetailsByBrand(int id, int brandId)
        {
            var result = this.GetActive(q => q.ProductID == id && q.ProductCategory.BrandId == brandId)
                .FirstOrDefault();

            if (result == null)
            {
                return null;
            }
            else
            {
                return new ProductDetails()
                {
                    Product = result,
                    ProductCollections = result.ProductCollectionItemMappings
                        .Where(q => q.Active && q.ProductCollection.Active),
                    ProductImages = result.ProductImages.Where(a => a.Active),
                    ProductSpecifications = result.ProductSpecifications.Where(a => a.Active)
                };
            }
        }

        public IQueryable<Product> GetLikelyProducts(string seoName, int storeId)
        {
            var product = this.GetActive(q => q.SeoName.Equals(seoName) && q.ProductCategory.StoreId == storeId)
                .FirstOrDefault();

            if (product == null)
            {
                return null;
            }
            else
            {
                var likelyProducts = product.ProductCategory.Products
                    .AsQueryable()
                    .Where(a => a.ProductID != product.ProductID);
                return likelyProducts;
            }
        }

        public IQueryable<Product> GetAvailableByProductCategoryAndPattern(int categoryId, int storeId, string pattern)
        {
            var product = this.GetActive(p => (p.GeneralProductId == null) &&
                       (pattern != null || p.ProductName.Contains(pattern)) &&
                       (categoryId <= 0 || p.CatID == categoryId) &&
                       (p.ProductCategory.StoreId == storeId) &&
                       (p.IsAvailable));
            return product;
        }

        public IQueryable<Product> GetAllProductGeneral(int productId)
        {
            var pro = this.Get(productId);
            var listPro = this.GetActive(a => a.GeneralProductId == pro.GeneralProductId);
            return listPro;
        }

        public Product GetProductGeneral(int productId)
        {
            var pro = this.Get(a => a.GeneralProductId == productId).FirstOrDefault();
            return pro;
        }

        public IQueryable<Product> GetProductOfCategoryBySeoName(string seoName, int storeId)
        {
            var listProduct = new List<Product>();
            var cateService = DependencyUtils.Resolve<IProductCategoryRepository>();
            var product = this.GetActive(q => q.SeoName.Equals(seoName) && q.ProductCategory.StoreId == storeId)
                .FirstOrDefault();
            if (product == null)
            {
                return null;
            }
            else
            {
                var listCate = cateService.Get(a => a.ParentCateId == product.ProductCategory.ParentCateId);
                foreach (var item in listCate)
                {
                    listProduct.AddRange(item.Products);
                }
                return listProduct.AsQueryable();
            }
        }

        public IQueryable<Product> GetChildProductGeneral(int productId)
        {
            var product = this.Get(a => a.GeneralProductId == productId);
            return product;
        }
        #endregion

        #region By Brand
        public async Task<ProductDetails> GetBySeoNameAsyncInBrand(string seoName, int brandId)
        {
            var product = await this.GetActive(q => q.IsAvailable == true && q.SeoName == seoName && q.ProductCategory.BrandId == brandId)
                .FirstOrDefaultAsync();
            if (product == null)
            {
                return null;
            }
            else
            {
                return new ProductDetails()
                {
                    Product = product,
                    ProductCollections = product.ProductCollectionItemMappings
                        .Where(q => q.Active && q.ProductCollection.Active),
                    ProductImages = product.ProductImages.Where(a => a.Active),
                    ProductSpecifications = product.ProductSpecifications.Where(a => a.Active)
                };
            }
        }
        public IQueryable<Product> GetActiveByBrandId(int brandId)
        {
            var products = this
                .GetActive(q => q.ProductCategory.BrandId == brandId && q.IsAvailable == true)
                .OrderBy(q => q.Position);
            return products;
        }
        public IQueryable<Product> GetAdminByBrandWithFilter(int brandId, string keyword, KeyValuePair<string, bool> orderByProperty)
        {
            var result = this.GetActive(q =>
                q.ProductCategory.BrandId == brandId && q.IsAvailable == true &&
                (keyword == null || q.ProductName.Contains(keyword) || q.ProductCategory.CateName.Contains(keyword)));

            ProductSortableProperty name;
            if (orderByProperty.Key != null && Enum.TryParse(orderByProperty.Key, out name))
            {
                switch (name)
                {
                    case ProductSortableProperty.ProductID:
                        result = result.OrderBy(q => q.ProductID, orderByProperty.Value);
                        break;
                    case ProductSortableProperty.Code:
                        result = result.OrderBy(q => q.Code, orderByProperty.Value);
                        break;
                    case ProductSortableProperty.ProductName:
                        result = result.OrderBy(q => q.ProductName, orderByProperty.Value);
                        break;
                    case ProductSortableProperty.ProductCategory:
                        result = result.OrderBy(q => q.ProductCategory.CateName, orderByProperty.Value);
                        break;
                    case ProductSortableProperty.IsAvailable:
                        result = result.OrderBy(q => q.IsAvailable, orderByProperty.Value);
                        break;
                    case ProductSortableProperty.Position:
                        result = result.OrderBy(q => q.Position, orderByProperty.Value);
                        break;
                }
            }
            else
            {
                result = result.OrderBy(q => q.Position).ThenBy(q => q.CreateTime);
            }
            return result;
        }

        // author: DucBM
        public IQueryable<ProductDetails> GetProductDetailsOfCategoryBySeoName(string seoName, int storeId)
        {
            var products = this.GetProductOfCategoryBySeoName(seoName, storeId);
            return products.Select(a => new ProductDetails
            {
                Product = a,
                ProductCollections = a.ProductCollectionItemMappings,
                ProductSpecifications = a.ProductSpecifications.Where(b => b.Active),
                ProductImages = a.ProductImages
            });
        }

        public void EditSeoName()
        {
            var product = this.Get(a => a.IsAvailable == true).ToList();
            foreach (var item in product)
            {
                item.SeoName = StringConvert.ConvertShortName(item.ProductName);
                this.Update(item);
            };

            this.Save();
        }


        public IQueryable<Product> GetLikelyProductsInBrand(string seoName, int brandId)
        {
            var product = this.GetActive(q => q.SeoName.Equals(seoName) && q.ProductCategory.BrandId == brandId)
                .FirstOrDefault();

            if (product == null)
            {
                return null;
            }
            else
            {
                var likelyProducts = product.ProductCategory.Products
                    .AsQueryable()
                    .Where(a => a.ProductID != product.ProductID);
                return likelyProducts;
            }
        }
        public IQueryable<ProductDetails> GetActiveWithSpecsByBrandId(int brandId)
        {
            var products = this.GetActiveByBrandId(brandId);

            return products.Select(a => new ProductDetails
            {
                Product = a,
                ProductCollections = a.ProductCollectionItemMappings,
                ProductSpecifications = a.ProductSpecifications.Where(b => b.Active),
                ProductImages = a.ProductImages
            });
        }
        public IQueryable<Product> GetAvailableByProductCategoryAndPatternInBrand(int categoryId, int brandId, string pattern)
        {
            IProductCategoryService categoryService = DependencyUtils.Resolve<IProductCategoryService>();
            List<int> categoriesList = new List<int>();
            GetAllSubcategories(categoryId, categoriesList, brandId);
            var product = this.GetActive(p => (p.GeneralProductId == null) &&
                       (pattern != null || p.ProductName.Contains(pattern)) &&
                       (categoryId <= 0 || categoriesList.Contains(p.CatID)) &&
                       (p.ProductCategory.BrandId == brandId));
            return product;
        }

        public IQueryable<Product> GetByProductCategoryAndPatternInBrand(int categoryId, int brandId, string pattern)
        {
            IProductCategoryService categoryService = DependencyUtils.Resolve<IProductCategoryService>();
            List<int> categoriesList = new List<int>();
            var product = this.GetActive(p => (p.ProductType != (int)ProductTypeEnum.General) &&
                       (pattern == null || p.ProductName.Contains(pattern)) &&
                       (categoryId <= 0 || p.CatID == categoryId) &&
                       (p.ProductCategory.BrandId == brandId));
            return product;
        }
        public IEnumerable<Product> GetActiveProductsEntitybyBrandId(int brandId)
        {
            return this.GetActive(p => (p.ProductCategory.BrandId == brandId) && (p.IsAvailable == true));
        }

        private void GetAllSubcategories(int parentId, List<int> result, int brandId)
        {
            if (!result.Contains(parentId))
            {
                result.Add(parentId);
            }
            IProductCategoryService categoryService = DependencyUtils.Resolve<IProductCategoryService>();
            IQueryable<ProductCategory> category = categoryService.GetActive(p => p.Active == true && p.BrandId == brandId
            && p.ParentCateId == parentId);
            var temp = category.ToList<ProductCategory>();
            if (temp == null || temp.Count == 0)
            {
                return;
            }

            foreach (var item in temp)
            {
                GetAllSubcategories(item.CateID, result, brandId);
            }
        }

        public IQueryable<Product> GetGeneralProductByBrandId(int brandId)
        {
            var result = this.Get(q => q.IsAvailable && q.ProductType == (int)ProductTypeEnum.General && q.ProductCategory.BrandId == brandId);
            return result;
        }

        #endregion


        #region Store Report
        public IQueryable<Product> GetProducts()
        {
            return this.Get(p => p.IsAvailable).OrderBy(c => c.ProductName);
        }
        public IQueryable<Product> GetAllProducts()
        {
            return this.Get(q => q.Active == true);
        }
        public IQueryable<ProductForComparisonReport> GetAllStoreActiveProductsForReport(int storeId)
        {
            return GetActiveByStoreId(storeId).Select(q => new ProductForComparisonReport
            {
                ProductID = q.ProductID,
                ProductName = q.ProductName,
                CateID = q.CatID,
                CateName = q.ProductCategory.CateName
            });
        }
        #endregion

        public IQueryable<ProductForComparisonReport> GetAllBrandActiveProductsForReport(int brandId)
        {
            return GetActiveByBrandId(brandId).Select(q => new ProductForComparisonReport
            {
                ProductID = q.ProductID,
                ProductName = q.ProductName,
                CateID = q.CatID,
                CateName = q.ProductCategory.CateName
            });
        }
    }

    public class ProductDetails : IEntity
    {
        public Product Product { get; set; }
        public IEnumerable<ProductCollectionItemMapping> ProductCollections { get; set; }
        public IEnumerable<ProductImage> ProductImages { get; set; }
        public IEnumerable<ProductSpecification> ProductSpecifications { get; set; }
    }

    //public class ProductSpec : IEntity
    //{
    //    public Product Product { get; set; }
    //    public IEnumerable<ProductSpecification> ProductSpecifications { get; set; }
    //}
    public class ProductName
    {
        public string Name { get; set; }
    }

    public class ProductForComparisonReport
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public int CateID { get; set; }
        public string CateName { get; set; }
    }

    public enum ProductSortableProperty
    {
        ProductID,
        Code,
        ProductName,
        ProductCategory,
        IsAvailable,
        Position,
    }

}
