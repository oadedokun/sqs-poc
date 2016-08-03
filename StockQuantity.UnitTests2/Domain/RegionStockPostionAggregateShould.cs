using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using StockQuantity.UnitTests2.Domain.DomainHelpers;
using StockQuantity2;
using StockStatus = StockQuantity2.Domain.StockStatus;

namespace StockQuantity.UnitTests2.Domain
{
    [TestFixture]
    public class RegionStockPostionAggregateShould
    {
        [SetUp]
        public void TestSetup()
        {
            IoCConfig.BuildContainer();
        }

        [Test]
        public void KnowWhichRegionsItMaintains()
        {
            var target = DomainEntityBuilders.InitialiseEmptyRegionStockInstance();

            var listOfRegions = target.MaintainedRegionsAndWarehouses;

            listOfRegions
                .Select(maintainedRegion => maintainedRegion.Region)
                .Should().Contain(new[] { "UK", "Europe", "US" });
        }

        [Test]
        public void KnowWhichFulfilmentCentresAreIncludedInItsRegions()
        {
            var target = DomainEntityBuilders.InitialiseEmptyRegionStockInstance();

            var maintainedRegions = target.MaintainedRegionsAndWarehouses.ToList();

            maintainedRegions.First(r => r.Region == "UK").Warehouses.Should().BeEquivalentTo("FC01");
            maintainedRegions.First(r => r.Region == "Europe").Warehouses.Should().BeEquivalentTo("FC04");
            maintainedRegions.First(r => r.Region == "US").Warehouses.Should().BeEquivalentTo("FC01", "FC03");
        }

        [Test]
        public void HaveNoStockWhenThereAreNoWarehouseStocksForTheRegion()
        {
            var target = DomainEntityBuilders.InitialiseEmptyRegionStockInstance();

            var regionalPositions = target.RegionPositions.ToList();

            regionalPositions.First(regionalPosition => regionalPosition.Region == "UK").AssertAgainst(0, 0, StockStatus.OutOfStock);
            regionalPositions.First(regionalPosition => regionalPosition.Region == "Europe").AssertAgainst(0, 0, StockStatus.OutOfStock);
            regionalPositions.First(regionalPosition => regionalPosition.Region == "US").AssertAgainst(0, 0, StockStatus.OutOfStock);
        }

        
    }
}
