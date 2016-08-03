using System;
using Moq;
using StockQuantity2;
using StockQuantity2.Domain;

namespace StockQuantity.UnitTests2.Domain.DomainHelpers
{
    public static class DomainEntityBuilders
    {
        public static RegionStockPostionAggregate InitialiseEmptyRegionStockInstance()
        {
            var mockRegionConfiguration = new Mock<IRegionConfiguration>();
            mockRegionConfiguration
                .Setup(rc => rc.RegionsToMaintain)
                .Returns(new[] { "UK", "Europe", "US" });

            var mockRegionWarehouseMap = new Mock<IRegionWarehouseMap>();
            mockRegionWarehouseMap.Setup(map => map.GetWarehousesForRegion("UK")).Returns(new[] { "FC01" });
            mockRegionWarehouseMap.Setup(map => map.GetWarehousesForRegion("Europe")).Returns(new[] { "FC04" });
            mockRegionWarehouseMap.Setup(map => map.GetWarehousesForRegion("US")).Returns(new[] { "FC01", "FC03" });

            IoCConfig.Inject(mockRegionWarehouseMap.Object);
            IoCConfig.Inject(mockRegionConfiguration.Object);

            return new RegionStockPostionAggregate();
        }

        public static RegionStockPostionAggregate InitialiseRegionStockAggregate(WarehouseStockPositionType positionTypeAtAllWarehouses, string sku)
        {
            return InitialiseRegionStockAggregate(positionTypeAtAllWarehouses, sku, new DateTime(2016, 01, 01));
        }

        public static RegionStockPostionAggregate InitialiseRegionStockAggregate(WarehouseStockPositionType positionTypeAtAllWarehouses, string sku, DateTime lastWarehouseUpdate)
        {
            var result = InitialiseEmptyRegionStockInstance();

            var position = SampleWarehousePosition.FromPositionType(positionTypeAtAllWarehouses);

            result.ApplyWarehouseAvailableStockUpdate(new WarehouseStockPosition("FC01", sku, position.Pickable, position.Reserved, position.Allocated, lastWarehouseUpdate));
            result.ApplyWarehouseAvailableStockUpdate(new WarehouseStockPosition("FC03", sku, position.Pickable, position.Reserved, position.Allocated, lastWarehouseUpdate));
            result.ApplyWarehouseAvailableStockUpdate(new WarehouseStockPosition("FC04", sku, position.Pickable, position.Reserved, position.Allocated, lastWarehouseUpdate));

            return result;
        }
    }
}