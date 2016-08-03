using System.Collections.Generic;

namespace StockQuantity2.Domain
{
    public interface IRegionWarehouseMap
    {
        IEnumerable<string> GetWarehousesForRegion(string region);
    }
}