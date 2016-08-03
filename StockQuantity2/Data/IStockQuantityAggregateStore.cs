using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StockQuantity2.Domain;

namespace StockQuantity2.Data
{
    public interface IStockQuantityAggregateStore : IDisposable
    {
        StockQuantity GetStockQuantityByVariantId(int variantId);
        Task UpdateStockQuantity(StockQuantity stockQuantity);
        Task CreateStockQuantity(StockQuantity stockQuantity);
        Task CreateSkuVariantMap(SkuVariantMap skuVariantMap);
        SkuVariantMap GetSkuVariantMap(string sku);
        Task Persist(IStockQuantityAggregate stockQuantityAggregate);
        IReadOnlyList<SkuVariantMap> GetSkuVariantMap(int batchSize);
    }
}
