using System;
using Microsoft.ServiceBus.Messaging;
using StockQuantity.Contracts.Events;
using StockQuantity.Data;
using StockQuantity.Domain;

namespace StockQuantity.Worker.Messaging
{
    public class WarehouseAvailableStockChangedV1Handler : IMessageHandler<IWarehouseAvailableStockChangedV1>
    {
        private readonly TopicClient _stockQuantityTopicClient;
        private readonly IStockQuantityAggregateStore _stockQuantityStore;
        
        public WarehouseAvailableStockChangedV1Handler(TopicClient stockQuantityTopicClient, IStockQuantityAggregateStore stockQuantityStore)
        {
            if (stockQuantityTopicClient == null)
            {
                throw new ArgumentNullException();
            }

            if (stockQuantityStore == null)
            {
                throw new ArgumentNullException();
            }

            if (stockQuantityTopicClient == null)
            {
                throw new ArgumentNullException();
            }

            _stockQuantityTopicClient = stockQuantityTopicClient;
            _stockQuantityStore = stockQuantityStore;
        }

        public void OnMessage(IWarehouseAvailableStockChangedV1 message)
        {
            if (message == null)
            {
                throw new ArgumentNullException();
            }

            var stockQuantityAggregate = CreateStockQuantityAggregateBySku(message.Sku, _stockQuantityStore);
            stockQuantityAggregate.ApplyStockChanges(new Domain.WarehouseAvailableStock(message.FulfilmentCentre, message.Sku, message.Pickable, message.Reserved, message.Allocated, message.Version));
            _stockQuantityStore.Persist(stockQuantityAggregate);
            PublishRegionStockChanged(stockQuantityAggregate);
        }

        private StockQuantityAggregate CreateStockQuantityAggregateBySku(string sku, IStockQuantityAggregateStore stockQuantityStore)
        {
            var skuVariantMap = stockQuantityStore.GetSkuVariantMap(sku);
            if (skuVariantMap == null)
            {
                throw new Exception("Sku Variant Map Not Found");
            }

            var stockQuantity = stockQuantityStore.GetStockQuantityByVariantId(skuVariantMap.VariantId);
            return new StockQuantityAggregate(stockQuantity.VariantId, stockQuantity.WarehouseAvailableStocks, stockQuantity.RegionStocks, stockQuantity.Version);
        }

        private void PublishRegionStockChanged(IStockQuantityAggregate stockQuantityAggregate)
        {
            foreach (var regionStock in stockQuantityAggregate.RegionStocks)
            {
                if (regionStock.Status.IsChanged)
                {
                    var regionStockStatusChanged = new RegionStockStatusChangedV1(regionStock.Region, regionStock.VariantId, (Contracts.StockStatus)regionStock.Status.Value, regionStock.Version);

                    _stockQuantityTopicClient.Send(new BrokeredMessage(regionStockStatusChanged));
                }
            }
            
        }
    }
}
