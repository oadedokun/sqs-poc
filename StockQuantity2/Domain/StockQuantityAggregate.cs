using System.Collections.Generic;
using Newtonsoft.Json;

namespace StockQuantity.Domain
{
    public class StockQuantityAggregate : IStockQuantityAggregate
    {
        public StockQuantityAggregate()
        {
            
        }

        public StockQuantityAggregate(int variantId, IEnumerable<WarehouseAvailableStock> warehouseAvailableStocks, IEnumerable<RegionStock> regionStocks, string version)
        {
            VariantId = variantId;
            WarehouseAvailableStocks = warehouseAvailableStocks;
            RegionStocks = regionStocks;
            Version = version;
        }
        
        public int VariantId { get; }
        public IEnumerable<WarehouseAvailableStock> WarehouseAvailableStocks { get; }
        public IEnumerable<RegionStock> RegionStocks { get; }
        public string Version { get; }
        public void ApplyStockChanges(WarehouseAvailableStock warehouseAvailableStock)
        {
            foreach (var availableStock in WarehouseAvailableStocks)
            {
                availableStock.ApplyStockChanges(warehouseAvailableStock);
            }

            foreach (var regionStock in RegionStocks)
            {
                regionStock.ApplyStockChanges(warehouseAvailableStock);
            }
        }

    }
}