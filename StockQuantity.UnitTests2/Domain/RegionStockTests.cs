using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StockQuantity2.Domain;

namespace StockQuantity.UnitTests.Domain
{
    [TestFixture]
    public class RegionStockTests
    {
        [Test]
        public void Given_Region_Is_OutofStock_When_Applying_Stock_Changes_Due_To_ITL_Notifications_Should_Return_A_Stock_Status_Not_Equal_To_OutofStock()
        {
            // Arrange
            var regionStock = new RegionStockBuilder()
                .OutOfStockAtAllWarehouses()
                .Build();

            var firstWarehouse = regionStock.WarehouseAvailableStocks.First();

            var newWarehouseAvailableStock = new WarehouseAvailableStockBuilder()
                .ForFulfilmentCentre(firstWarehouse.FulfilmentCentre)
                .ForSku(firstWarehouse.Sku)
                .WithPickableAllocatedReservedQtys(firstWarehouse.Pickable + 1, firstWarehouse.Allocated, firstWarehouse.Reserved)
                .Build();
                        
            // Act
            regionStock.ApplyStockChanges(newWarehouseAvailableStock);

            // Assert
            Assert.AreNotEqual(regionStock.Status.Value, StockStatus.OutOfStock);
        }

        [Test]
        public void Given_Region_Is_InStock_When_Applying_Low_In_Stock_StockChange_Should_Return_A_Stock_Status_Is_Equal_To_Low_In_Stock()
        {
            // Arrange
            var lowInStockThreshold = 10;
            var wasVersion = DateTime.Now;

            var regionStock = new RegionStockBuilder()
                .WithLowInStockThreshold(lowInStockThreshold)
                .WithStockInWarehouses(new[]
                {
                    new WarehouseAvailableStockBuilder()
                        .ForFulfilmentCentre("FC01")
                        .WithPickableAllocatedReservedQtys(lowInStockThreshold + 1, 0, 0)
                        .WithVersion(wasVersion)
                })
                .Build();

            var newAvailableStock = new WarehouseAvailableStockBuilder()
                .ForFulfilmentCentre("FC01")
                .WithPickableAllocatedReservedQtys(lowInStockThreshold - 1, 0, 0)
                .WithVersion(wasVersion.AddMinutes(5))
                .Build();

            // Act
            regionStock.ApplyStockChanges(newAvailableStock);

            // Assert
            Assert.AreEqual(regionStock.Status.Value, StockStatus.LowInStock);
        }

        [Test]
        public void Given_Region_Is_InStock_When_Applying_Out_Of_Stock_StockChange_Should_Return_A_Stock_Status_Is_Equal_To_Out_Of_Stock()
        {
            // Arrange
            var wasVersion = DateTime.Now;

            var regionStock = new RegionStockBuilder()
                .WithStockInWarehouses(new[]
                {
                    new WarehouseAvailableStockBuilder()
                        .ForFulfilmentCentre("FC01")
                        .WithPickableAllocatedReservedQtys(3, 0, 0)
                        .WithVersion(wasVersion)
                })
                .Build();

            var newAvailableStock = new WarehouseAvailableStockBuilder()
                .ForFulfilmentCentre("FC01")
                .WithPickableAllocatedReservedQtys(10, 5, 5)
                .WithVersion(wasVersion.AddMinutes(5))
                .Build();

            // Act
            regionStock.ApplyStockChanges(newAvailableStock);

            // Assert
            Assert.AreEqual(regionStock.Status.Value, StockStatus.OutOfStock);
            Assert.AreEqual(regionStock.Quantity, 0);
        }
    }
}
