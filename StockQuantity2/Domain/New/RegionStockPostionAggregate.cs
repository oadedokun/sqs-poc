using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace StockQuantity2.Domain
{
    public class RegionStockPostionAggregate
    {
        private readonly IRegionConfiguration _regionConfiguration;
        private readonly IRegionWarehouseMap _regionWarehouseMap;
        private List<RegionalStockPosition> _regionalStockPositions;
        private Dictionary<string, WarehouseStockPosition> _warehouseStockPositions;


        public RegionStockPostionAggregate()
        {
            _regionConfiguration = IoCConfig.ResolveService<IRegionConfiguration>();
            _regionWarehouseMap = IoCConfig.ResolveService<IRegionWarehouseMap>();

            _warehouseStockPositions = new Dictionary<string, WarehouseStockPosition>();

            CalculateRegionStockPositions();
        }

        public RegionStockPostionAggregate(int variantId): this()
        {
            VariantId = variantId;
        }


        [JsonProperty("id")]
        public string Id => VariantId.ToString();

        [JsonProperty("variantId")]
        public int VariantId { get; }

        [JsonProperty("version")]
        public DateTime Version { get; }

        [JsonProperty("warehouses")]
        public IEnumerable<WarehouseStockPosition> WarehouseStockPositions {
            get { return _warehouseStockPositions.Values; }
            private set
            {
                _warehouseStockPositions = value.ToDictionary(GetKeyForWarehouseStockPosition);
                CalculateRegionStockPositions();
            }
        }

        [JsonProperty("regions")]
        public IEnumerable<RegionalStockPosition> RegionPositions => _regionalStockPositions;

        [JsonIgnore]
        public IEnumerable<RegionWarehouses> MaintainedRegionsAndWarehouses => 
            _regionConfiguration.RegionsToMaintain
                .Select(region => new RegionWarehouses(region, _regionWarehouseMap.GetWarehousesForRegion(region)));

        public void ApplyWarehouseAvailableStockUpdate(WarehouseStockPosition newStockPosition)
        {
            // TODO: ignore newStockPosition if it is older            
            var key = GetKeyForWarehouseStockPosition(newStockPosition);
            _warehouseStockPositions[key] = newStockPosition;
            
            CalculateRegionStockPositions();
        }

        private void CalculateRegionStockPositions()
        {
            _regionalStockPositions = MaintainedRegionsAndWarehouses
                .Select(maintainedRegion =>
                {
                    var applicableWarehouseStockPositions = _warehouseStockPositions.Values
                        .Where(wsp => maintainedRegion.Warehouses.Contains(wsp.Warehouse))
                        .ToList();

                    if (!applicableWarehouseStockPositions.Any())
                    {
                        return new RegionalStockPosition(maintainedRegion.Region, 0, 0);
                    }

                    var warehousesSummed = applicableWarehouseStockPositions
                        .Select(wsp => new { Pickable = wsp.PickableQuantity, Allocated = wsp.AllocatedQuantity, Reserved = wsp.ReservedQuantity })
                        .Aggregate((one, two) => new { Pickable = one.Pickable + two.Pickable, Allocated = one.Allocated + two.Allocated, Reserved = one.Reserved + two.Reserved });

                    return new RegionalStockPosition(
                        maintainedRegion.Region,
                        warehousesSummed.Pickable - (warehousesSummed.Allocated + warehousesSummed.Reserved),
                        warehousesSummed.Reserved);
                })
                .ToList();
        }

        private static string GetKeyForWarehouseStockPosition(WarehouseStockPosition newStockPosition)
        {
            return string.Join("|", newStockPosition.Warehouse, newStockPosition.Sku);
        }
    }
}