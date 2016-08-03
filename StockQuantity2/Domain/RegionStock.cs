using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace StockQuantity.Domain
{
    public class RegionStock
    {
        public RegionStock()
        {
            
        }

        public RegionStock(string region, int variantId, IEnumerable<WarehouseAvailableStock> warehouseAvailableStocks, int lowInStockThreshold)
        {
            Region = region;
            VariantId = variantId;
            _warehouseAvailableStocks = warehouseAvailableStocks?.ToList() ?? _warehouseAvailableStocks;
            Status = new RegionStockStatus(lowInStockThreshold);
        }

        private readonly List<WarehouseAvailableStock> _warehouseAvailableStocks = new List<WarehouseAvailableStock>();

        [JsonProperty("regionId")]
        public string Region { get; }

        [JsonProperty("variantId")]
        public int VariantId { get; }

        [JsonProperty("quantity")]
        public int Quantity {
            get
            {
                return WarehouseAvailableStocks.Sum(x => x.Pickable - (x.Allocated + x.Reserved));
            }
        }

        [JsonProperty("status")]
        public RegionStockStatus Status { get; }

        [JsonProperty("version")]
        public DateTime Version { get; private set; }
        
        [JsonIgnore]
        public IReadOnlyList<WarehouseAvailableStock> WarehouseAvailableStocks => _warehouseAvailableStocks;

        public void ApplyStockChanges(WarehouseAvailableStock warehouseAvailableStock)
        {
            if (warehouseAvailableStock == null)
            {
                throw new ArgumentNullException();
            }

            if (_warehouseAvailableStocks != null && _warehouseAvailableStocks.Any())
            {
                var index =
                    _warehouseAvailableStocks.FindIndex(
                        (x) =>
                            (x.Sku == warehouseAvailableStock.Sku &&
                             x.FulfilmentCentre == warehouseAvailableStock.FulfilmentCentre));
                if (index > -1)
                {
                    _warehouseAvailableStocks.RemoveAt(index);
                    _warehouseAvailableStocks.Insert(index, warehouseAvailableStock);
                }
                else
                {
                    _warehouseAvailableStocks.Add(warehouseAvailableStock);
                }
            }
            
            Status.Evaluate(Quantity);

            // TODO:  The version needs to be tied to the RegionStock DB when it gets persisted, not the WAS version
            Version = warehouseAvailableStock.Version;
        }
    }
}
