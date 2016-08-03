using System.Collections.Generic;

namespace StockQuantity2.Domain
{
    public class RegionWarehouses
    {
        public RegionWarehouses(string region, IEnumerable<string> warehouses)
        {
            Region = region;
            Warehouses = warehouses;
        }

        public string Region { get; }  

        public IEnumerable<string> Warehouses{ get; }  
    }
}