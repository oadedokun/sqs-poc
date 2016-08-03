﻿using System.Collections.Generic;

namespace StockQuantity2.Domain
{
    public interface IStockQuantityAggregate
    {
        int VariantId { get; }
        IEnumerable<WarehouseAvailableStock> WarehouseAvailableStocks { get; }
        IEnumerable<RegionStock> RegionStocks { get; }
        string Version { get; }
        void ApplyStockChanges(WarehouseAvailableStock warehouseAvailableStock);
    }
}