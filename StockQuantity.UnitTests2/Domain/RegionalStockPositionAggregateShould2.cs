using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using StockQuantity.UnitTests2.Domain.DomainHelpers;
using StockQuantity2;
using StockQuantity2.Domain;

namespace StockQuantity.UnitTests2.Domain
{
    [TestFixture]
    public class RegionalStockPositionAggregateShould2
    {
        [SetUp]
        public void TestSetup()
        {
            IoCConfig.BuildContainer();
        }

        [TestCase("FC01", 15, 5, 0, "UK,US", StockStatus.InStock)]
        [TestCase("FC01", 15, 5, 1, "UK,US", StockStatus.LowInStock)]
        [TestCase("FC04", 15, 5, 0, "Europe", StockStatus.InStock)]
        [TestCase("FC04", 15, 5, 1, "Europe", StockStatus.LowInStock)]
        [TestCase("FC03", 15, 5, 0, "US", StockStatus.InStock)]
        [TestCase("FC03", 15, 5, 1, "US", StockStatus.LowInStock)]
        public void UpdateRegionalStockPositionsCorrectlyForFirstWarehouseUpdate(string warehouse, int pickable, int reserved, int allocated, string regionsToTest, StockStatus expectedStatus)
        {
            // Arrange
            var target = DomainEntityBuilders.InitialiseEmptyRegionStockInstance();
            var newStockPositionAtWarehouse = new WarehouseStockPosition(warehouse, "arbitrarySku", pickable, reserved, allocated, DateTime.Parse("2016-06-15T08:00:00Z"));

            // Act
            target.ApplyWarehouseAvailableStockUpdate(newStockPositionAtWarehouse);

            // Assert
            var regionalPositions = target.RegionPositions.ToList();
            foreach (var region in regionsToTest.Split(','))
            {
                regionalPositions
                    .First(regionalPosition => regionalPosition.Region == region)
                    .AssertAgainst(pickable - (allocated + reserved), reserved, expectedStatus);
            }
        }

        [TestCase("FC01", "UK", WarehouseStockPositionType.NoStockAtAll, StockStatus.OutOfStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.JustBelowThreshold, StockStatus.LowInStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.AboveThreshold, StockStatus.InStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.NoStockAtAll, StockStatus.OutOfStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.JustBelowThreshold, StockStatus.LowInStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.AboveThreshold, StockStatus.InStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.NoStockAtAll, StockStatus.OutOfStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.JustBelowThreshold, StockStatus.LowInStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.AboveThreshold, StockStatus.InStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.NoStockAtAll, StockStatus.OutOfStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.JustBelowThreshold, StockStatus.LowInStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.AboveThreshold, StockStatus.InStock)]
        public void MoveFromOutOfStock(string warehouseSendingUpdate, string regionToCheck, WarehouseStockPositionType updateToPosition, StockStatus expectedStockStatus)
        {
            // Arrange
            const string anArbitrarySku = "sku124";
            var target = DomainEntityBuilders.InitialiseRegionStockAggregate(WarehouseStockPositionType.OutOfStock, anArbitrarySku);
            var newWarehouseStockPosition = NewWarehouseStockPosition(warehouseSendingUpdate, updateToPosition, anArbitrarySku);

            // Act
            target.ApplyWarehouseAvailableStockUpdate(newWarehouseStockPosition);

            // Assert
            target.RegionPositions
                .First(regionalPosition => regionalPosition.Region == regionToCheck)
                .Status.Should().Be(expectedStockStatus, $"region { regionToCheck } should be { expectedStockStatus }");
        }

        [TestCase("FC01", "UK", WarehouseStockPositionType.NoStockAtAll, StockStatus.OutOfStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.OutOfStock, StockStatus.OutOfStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.JustBelowThreshold, StockStatus.LowInStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.AboveThreshold, StockStatus.InStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.NoStockAtAll, StockStatus.LowInStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.OutOfStock, StockStatus.LowInStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.JustBelowThreshold, StockStatus.InStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.AboveThreshold, StockStatus.InStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.NoStockAtAll, StockStatus.LowInStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.OutOfStock, StockStatus.LowInStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.JustBelowThreshold, StockStatus.InStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.AboveThreshold, StockStatus.InStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.NoStockAtAll, StockStatus.OutOfStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.OutOfStock, StockStatus.OutOfStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.JustBelowThreshold, StockStatus.LowInStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.AboveThreshold, StockStatus.InStock)]
        public void MoveFromOneItemAvailable_ie_LowInStock(
            string warehouseSendingUpdate, 
            string regionToCheck, 
            WarehouseStockPositionType updateToPosition, 
            StockStatus expectedStockStatus)
        {
            // Arrange
            const string anArbitrarySku = "sku124";
            var target = DomainEntityBuilders.InitialiseRegionStockAggregate(WarehouseStockPositionType.OneItemAvailable, anArbitrarySku);
            var newWarehouseStockPosition = NewWarehouseStockPosition(warehouseSendingUpdate, updateToPosition, anArbitrarySku);
            
            // Act
            target.ApplyWarehouseAvailableStockUpdate(newWarehouseStockPosition);

            // Assert
            target.RegionPositions
                .First(regionalPosition => regionalPosition.Region == regionToCheck)
                .Status.Should().Be(expectedStockStatus, $"region { regionToCheck } should be { expectedStockStatus }");
        }

        [TestCase("FC01", "UK", WarehouseStockPositionType.NoStockAtAll, StockStatus.OutOfStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.OutOfStock, StockStatus.OutOfStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.JustBelowThreshold, StockStatus.LowInStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC01", "UK", WarehouseStockPositionType.OneItemAvailable, StockStatus.LowInStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.NoStockAtAll, StockStatus.InStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.OutOfStock, StockStatus.InStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.JustBelowThreshold, StockStatus.InStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC01", "US", WarehouseStockPositionType.OneItemAvailable, StockStatus.InStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.NoStockAtAll, StockStatus.InStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.OutOfStock, StockStatus.InStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.JustBelowThreshold, StockStatus.InStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC03", "US", WarehouseStockPositionType.OneItemAvailable, StockStatus.InStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.NoStockAtAll, StockStatus.OutOfStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.OutOfStock, StockStatus.OutOfStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.JustBelowThreshold, StockStatus.LowInStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.AtThreshold, StockStatus.InStock)]
        [TestCase("FC04", "Europe", WarehouseStockPositionType.OneItemAvailable, StockStatus.LowInStock)]

        public void MoveFromInStock(
            string warehouseSendingUpdate,
            string regionToCheck,
            WarehouseStockPositionType updateToPosition,
            StockStatus expectedStockStatus)
        {
            // Arrange
            const string anArbitrarySku = "sku124";
            var target = DomainEntityBuilders.InitialiseRegionStockAggregate(WarehouseStockPositionType.AboveThreshold, anArbitrarySku);
            var newWarehouseStockPosition = NewWarehouseStockPosition(warehouseSendingUpdate, updateToPosition, anArbitrarySku);

            // Act
            target.ApplyWarehouseAvailableStockUpdate(newWarehouseStockPosition);

            // Assert
            target.RegionPositions
                .First(regionalPosition => regionalPosition.Region == regionToCheck)
                .Status.Should().Be(expectedStockStatus, $"region { regionToCheck } should be { expectedStockStatus }");
        }

        private static WarehouseStockPosition NewWarehouseStockPosition(string warehouseSendingUpdate,
            WarehouseStockPositionType updateToPosition, string anArbitrarySku)
        {
            var samplePosi = SampleWarehousePosition.FromPositionType(updateToPosition);
            var newWarehouseStockPosition = new WarehouseStockPosition(
                warehouseSendingUpdate,
                anArbitrarySku,
                samplePosi.Pickable,
                samplePosi.Reserved,
                samplePosi.Allocated,
                DateTime.Today);
            return newWarehouseStockPosition;
        }
    }
}