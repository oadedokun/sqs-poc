using System;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Internal;
using FluentAssertions;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using NUnit.Framework;
using StockQuantity.UnitTests2.Domain.DomainHelpers;
using StockQuantity2;
using StockQuantity2.Data;
using StockQuantity2.Domain;

namespace StockQuantity.UnitTests2.Data
{
    [TestFixture]
    public class StockQuantityDocumentDbTests
    {
        private RegionStockPostionAggregateRepository _regionStockPostionDocumentDb;

        private string _dbName;

        private string _dbStockQuantityCollectionName;

        private string _dbSkuVariantMapCollectionName;

        private string _accountEndPoint;

        private string _accountKey;

        private ConnectionPolicy _connectionPolicy;

        [SetUp]
        public void SetUp()
        {
            IoCConfig.BuildContainer();

            _dbName = ConfigurationManager.AppSettings["Microsoft.DocumentDB.StockQuantity.DBName"];
            _dbStockQuantityCollectionName = ConfigurationManager.AppSettings["Microsoft.DocumentDB.StockQuantity.DBCollectionName"];
            _dbSkuVariantMapCollectionName = ConfigurationManager.AppSettings["Microsoft.DocumentDB.SkuVariantMap.DBCollectionName"];

            _accountEndPoint = ConfigurationManager.AppSettings["Microsoft.DocumentDB.StockQuantity.EUN.AccountEndpoint"];
            _accountKey = ConfigurationManager.AppSettings["Microsoft.DocumentDB.StockQuantity.EUN.AccountKey"];

            _connectionPolicy = new ConnectionPolicy();
            _connectionPolicy.PreferredLocations.Add("North Europe"); // first preference
            _connectionPolicy.PreferredLocations.Add("West Europe"); // second preference 
            _regionStockPostionDocumentDb = new RegionStockPostionAggregateRepository(_dbName, _dbStockQuantityCollectionName, _dbSkuVariantMapCollectionName, _accountEndPoint, _accountKey, _connectionPolicy);
        }

        [Test]
        public async Task ShouldCreateStockQuantityDocument()
        {
            var regionStockPostionAggregate = DomainEntityBuilders.InitialiseRegionStockAggregate(WarehouseStockPositionType.AboveThreshold, "someSku");

            try
            {
                await _regionStockPostionDocumentDb.CreateRegionStockPositionAggregate(regionStockPostionAggregate);
            }
            catch (DocumentClientException de)
            {
                Exception baseException = de.GetBaseException();
                Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
            }
            catch (Exception e)
            {
                Exception baseException = e.GetBaseException();
                Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
            }
        }

        [Test]
        public void ShouldGetStockQuantity()
        {
            var expected = DomainEntityBuilders.InitialiseRegionStockAggregate(WarehouseStockPositionType.AboveThreshold, "someSku");
            RegionStockPostionAggregate actual = null;
            
            actual = _regionStockPostionDocumentDb.GetRegionStockPostionAggregateByVariantId(100377);

            actual.Should().NotBeNull();
            actual.VariantId.Should().Be(expected.VariantId);
            actual.Id.Should().Be(expected.Id);
        }

        [Test]
        public void ShouldReplaceStockQuantity()
        {
            var pickable = new Random().Next(1, 200);
            var warehouseAvailableStock = new WarehouseAvailableStock("FC02", "ABC", pickable, 0, 0, DateTime.UtcNow);
            var result1 = _regionStockPostionDocumentDb.GetStockQuantityByVariantId(123);

            var list = result1.WarehouseAvailableStocks.ToList();
            list.Add(warehouseAvailableStock);


            result1.WarehouseAvailableStocks = list.AsEnumerable();

            result1.WarehouseAvailableStocks
                .ForEach(x => x.ApplyStockChanges(warehouseAvailableStock));
            result1.RegionStocks
                .ForEach(x => x.ApplyStockChanges(warehouseAvailableStock));

            _regionStockPostionDocumentDb.UpdateStockQuantity(result1).Wait();

            var result2 = _regionStockPostionDocumentDb.GetStockQuantityByVariantId(123);
                                      
            Assert.AreEqual(result1.WarehouseAvailableStocks.FirstOrDefault().Pickable, result2.WarehouseAvailableStocks.FirstOrDefault().Pickable);
        }

        [Test]
        public void ReplaceStockQuantity_ConcurrentWrites_ShouldThrowException()
        {
            var warehouseAvailableStock = new WarehouseAvailableStock("FC01", "ABC", 25, 0, 0, DateTime.UtcNow);
            var result1 = _regionStockPostionDocumentDb.GetStockQuantityByVariantId(123);
            var result2 = _regionStockPostionDocumentDb.GetStockQuantityByVariantId(123);

            result1.WarehouseAvailableStocks
                .ForEach(x => x.ApplyStockChanges(warehouseAvailableStock));

            result2.RegionStocks
                .ForEach(x => x.ApplyStockChanges(warehouseAvailableStock));

            _regionStockPostionDocumentDb.UpdateStockQuantity(result1).Wait();
            Assert.Throws<AggregateException>(() => _regionStockPostionDocumentDb.UpdateStockQuantity(result2).Wait());
        }

        [Test]
        public void WhenCreatingStockQuantityAndDuplicateExistsThenShouldThrowException()
        {
            var warehouseAvailableStock = new WarehouseAvailableStock("FC01", "ABC", 20, 0, 0, DateTime.UtcNow);
            var stockQuantity = new StockQuantity2.Data.StockQuantity(123, new[] { warehouseAvailableStock }, new[] { new RegionStock("US", 0, new[] { warehouseAvailableStock }, 10) });
            Assert.Throws<DocumentClientException>(() => _regionStockPostionDocumentDb.CreateStockQuantity(stockQuantity).Wait());
        }

        [Test]
        public void ShouldGetSkuVariantMap()
        {
            var sku = "974608667";
            var skuVariantMap = _regionStockPostionDocumentDb.GetSkuVariantMap("974608667");

            Assert.IsNotNull(skuVariantMap);
            Assert.AreEqual(sku, skuVariantMap.SKU);
        }

        [Test]
        public void ShouldGetBatchedSkuVariantMap()
        {
            var skuVariantMaps = _regionStockPostionDocumentDb.GetSkuVariantMap(500);

            Assert.IsNotNull(skuVariantMaps);
            Assert.AreEqual(500, skuVariantMaps.Count);
        }

        [Test]
        public void ShouldCreateSkuVariantMapDocument()
        {
            var variantId = new Random(200);
            var sku = new Random(300);

            for (var i = 0; i <= 50000; i++)
            {
                var vId = variantId.Next();
                var sId = sku.Next();
                var skuVariantMap = new SkuVariantMap
                {
                    VariantId = vId,
                    SKU = sId.ToString()
                };

                try
                {
                    _regionStockPostionDocumentDb.CreateSkuVariantMap(skuVariantMap).Wait();
                }
                catch (DocumentClientException de)
                {
                    Exception baseException = de.GetBaseException();
                    Console.WriteLine("{0} error occurred: {1}, Message: {2}", de.StatusCode, de.Message, baseException.Message);
                }
                catch (Exception e)
                {
                    Exception baseException = e.GetBaseException();
                    Console.WriteLine("Error: {0}, Message: {1}", e.Message, baseException.Message);
                }
            }
        }

        [TearDown]
        public void TearDown()
        {
            _regionStockPostionDocumentDb?.Dispose();
        }
    }
}
