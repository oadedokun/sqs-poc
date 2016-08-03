using System;
using Newtonsoft.Json;

namespace StockQuantity2.Domain
{
    public class WarehouseStockPosition
    {
        public WarehouseStockPosition(string warehouse, string sku, int pickableQuantity, int reservedQuantity, int allocatedQuantity, DateTime version)
        {
            Warehouse = warehouse;
            Sku = sku;
            PickableQuantity = pickableQuantity;
            ReservedQuantity = reservedQuantity;
            AllocatedQuantity = allocatedQuantity;
            Version = version;
        }

        [JsonProperty("warehouse")]
        public string Warehouse { get; private set; }

        [JsonProperty("sku")]
        public string Sku { get; private set; }

        [JsonProperty("pickable")]
        public int PickableQuantity { get; private set; }

        [JsonProperty("reserved")]
        public int ReservedQuantity { get; private set; }

        [JsonProperty("allocated")]
        public int AllocatedQuantity { get; private set; }

        [JsonProperty("version")]
        public DateTime Version { get; private set; }
    }
}