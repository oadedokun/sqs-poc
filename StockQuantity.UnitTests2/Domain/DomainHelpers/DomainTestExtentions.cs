using FluentAssertions;
using StockQuantity2.Domain;

namespace StockQuantity.UnitTests2.Domain.DomainHelpers
{
    public static class DomainTestExtentions
    {
        public static void AssertAgainst(
            this RegionalStockPosition regionalStockPosition, 
            int availableQty,
            int reservedQty, 
            StockStatus stockStatus)
        {
            regionalStockPosition.Should().NotBeNull();
            regionalStockPosition.AvailableQuantity.Should().Be(availableQty);
            regionalStockPosition.ReservedQuantity.Should().Be(reservedQty);
            regionalStockPosition.Status.Should().Be(stockStatus);

        }
    }
}