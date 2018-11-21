using HmsService.Sdk;
using SkyWeb.DatVM.Mvc.Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HmsService.Models.Entities.Services
{
    public partial interface IProductItemService
    {
        #region StoreReport
        IQueryable<ProductItem> GetAvailableProductItems();
        IQueryable<ProductItem> GetAvailableProductItemsbyBrand(int brandId);
        IQueryable<ProductItem> GetProductItems();
        #endregion
        #region Inventory
        Task<ProductItem> GetProductItemById(int productItemId);
        #endregion
        #region Provider
        IQueryable<ProductItem> GetProductItemsByCategoryId(int productCategoryId);
        #endregion
        IEnumerable<DateProductItem> GetProductItemByDate(IEnumerable<DateProduct> listProduct);
        IQueryable<ProductItem> GetProductItemByStore(int storeId, int brandId);
    }
    public partial class ProductItemService
    {
        #region StoreReport
        public IQueryable<ProductItem> GetAvailableProductItems()
        {
            return this.Get(c => c.IsAvailable != null && c.IsAvailable == true);
        }
        public IQueryable<ProductItem> GetAvailableProductItemsbyBrand(int brandId)
        {
            var caterMappingService = DependencyUtils.Resolve<IProductItemCategoryService>();
            var caterList = caterMappingService.GetItemCategoryByBrand(brandId).Select(q => q.CateID).ToList();
            return this.Get(c => c.IsAvailable != null && c.IsAvailable == true && caterList.Contains(c.CatID ?? 0));
        }
        public IQueryable<ProductItem> GetProductItems()
        {
            return this.Get().Where(a => a.IsAvailable == true);
        }
        #endregion
        #region Inventory
        public async Task<ProductItem> GetProductItemById(int productItemId)
        {
            return await this.GetAsync(productItemId);
        }
        #endregion
        #region Provider
        public IQueryable<ProductItem> GetProductItemsByCategoryId(int productCategoryId)
        {
            return this.Get(item => item.CatID == productCategoryId && (item.IsAvailable ?? false));
        }
        #endregion

        public IEnumerable<DateProductItem> GetProductItemByDate(IEnumerable<DateProduct> listProduct)
        {

            List<ProductItemCompositionMapping> listComposition = new List<ProductItemCompositionMapping>();

            var compositionApi = new ProductItemCompositionMappingApi();
            foreach (var item in listProduct)
            {
                var itemsInProduct = compositionApi.GetProductItemByProductID(item.ProductId);
                foreach (var item2 in itemsInProduct)
                {
                    item2.Quantity *= item.Quantity;
                    listComposition.Add(item2);
                }
            }
            var productItems = listComposition.GroupBy(p => p.ItemID).Select(sp => new DateProductItem()
            {
                ProductItemID = sp.FirstOrDefault().ItemID,
                ProductItemName = sp.FirstOrDefault().ProductItem.ItemName,
                Quantity = (int)sp.Sum(s => s.Quantity),
                Unit = sp.FirstOrDefault().ProductItem.Unit,
            });

            return productItems;
        }

        public IQueryable<ProductItem> GetProductItemByStore(int storeId, int brandId)
        {
            IQueryable<ProductItem> productItemList = null;
            // Dựa vào curStoreId, nếu id = 0 thì lấy hết ProductItem,
            // còn khác 0 thì chỉ lấy những ProductItem của store có id đó.
            if (storeId == 0)
            {
                productItemList = this.GetAvailableProductItemsbyBrand(brandId);
            }
            else
            {
                    InventoryReceiptApi inventoryReceiptApi = new InventoryReceiptApi();
                    InventoryReceiptItemApi inventoryReceiptItemApi = new InventoryReceiptItemApi();

                    var inventoryReceiptItem = inventoryReceiptItemApi.GetActive();
                    var productItem = this.GetActive();
                    var inventoryReceipt = inventoryReceiptApi.GetInventoryReceiptByStore(storeId);

                    var temp = from iri in inventoryReceiptItem
                               join ir in inventoryReceipt
                                        on iri.ReceiptID equals ir.ReceiptID
                               join pi in productItem
                                        on iri.ItemID equals pi.ItemID
                               select pi;
                    productItemList = temp.AsQueryable();
            }

            productItemList = productItemList.Where(w => w.IsAvailable == true).OrderBy(pi => pi.ItemName);

            return productItemList;
        }
    }
}
