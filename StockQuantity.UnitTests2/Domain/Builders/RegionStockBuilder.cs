using System.Linq;
using StockQuantity2.Domain;

namespace StockQuantity.UnitTests.Domain
{
    public class RegionStockBuilder : IDomainEntityBuilder<RegionStock>
    {
        private string _region = "UK";
        private int _variantId = 9876;
        private WarehouseAvailableStockBuilder[] _warehouseAvailableStockBuilders = { new WarehouseAvailableStockBuilder() };
        private int _lowInStockThreshold = 10;


        public RegionStock Build()
        {
            return new RegionStock(
                _region,
                _variantId,
                _warehouseAvailableStockBuilders.Select(builder => builder.Build()), _lowInStockThreshold);
        }

        public RegionStockBuilder WithLowInStockThreshold(int lowInStockThreshold)
        {
            _lowInStockThreshold = lowInStockThreshold;
            return this;
        }

        public RegionStockBuilder OutOfStockAtAllWarehouses()
        {
            foreach (var builder in _warehouseAvailableStockBuilders)
            {
                builder.OutOfStock();
            }

            return this;
        }

        public RegionStockBuilder WithStockInWarehouses(WarehouseAvailableStockBuilder[] warehouseBuilders)
        {
            _warehouseAvailableStockBuilders = warehouseBuilders;
            return this;
        }
    }
}