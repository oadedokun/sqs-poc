using Newtonsoft.Json;
using NUnit.Framework;
using StockQuantity.UnitTests2.Domain.DomainHelpers;
using StockQuantity2;

namespace StockQuantity.UnitTests2.Domain
{
    [TestFixture]
    public class RegionalStockPositionAggregateShould3
    {
        [SetUp]
        public void TestSetup()
        {
            IoCConfig.BuildContainer();
        }

        [Test]
        [Ignore("not implemented")]
        public void BeSerializableToJsonDocument()
        {
            var target = DomainEntityBuilders.InitialiseRegionStockAggregate(WarehouseStockPositionType.OutOfStock, "sku234");

            var serialisedResult = JsonConvert.SerializeObject(target, Formatting.Indented);
        }
    }
}