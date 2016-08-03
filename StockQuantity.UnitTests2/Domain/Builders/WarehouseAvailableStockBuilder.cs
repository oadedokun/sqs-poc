using System;
using StockQuantity.Domain;

namespace StockQuantity.UnitTests.Domain
{
    public class WarehouseAvailableStockBuilder : IDomainEntityBuilder<WarehouseAvailableStock> {
        private string _fulfilmentCentre = "FC01";
        private string _sku = "abc";
        private int _pickableQty = 20;
        private int _reservedQty = 5;
        private int _allocatedQty = 2;
        private DateTime _version = DateTime.Parse("2016-06-15T07:00:00");

        public WarehouseAvailableStockBuilder WithPickableQty(int pickableChange)
        {
            _pickableQty += pickableChange;
            return this;
        }

        public WarehouseAvailableStock Build()
        {
            return new WarehouseAvailableStock(_fulfilmentCentre, _sku, _pickableQty, _reservedQty, _allocatedQty, _version);
        }

        public WarehouseAvailableStockBuilder OutOfStock()
        {
            _pickableQty = _reservedQty + _allocatedQty;
            return this;
        }

        public WarehouseAvailableStockBuilder ForFulfilmentCentre(string fulfilmentCentre)
        {
            _fulfilmentCentre = fulfilmentCentre;
            return this;
        }

        public WarehouseAvailableStockBuilder WithPickableAllocatedReservedQtys(int pickable, int allocated, int reserved)
        {
            _pickableQty = pickable;
            _allocatedQty = allocated;
            _reservedQty = reserved;
            return this;
        }

        public WarehouseAvailableStockBuilder WithVersion(DateTime version)
        {
            _version = version;
            return this;
        }
        public WarehouseAvailableStockBuilder ForSku(string sku)
        {
            _sku = sku;
            return this;
        }
    }
}