using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace StockQuantity2.Domain
{
    [Obsolete("has been replaced.")]
    public class RegionStockStatus
    {
        public RegionStockStatus()
        {
        }

        public RegionStockStatus(int lowInStockThreshold)
        {
            LowInStockThreshold = lowInStockThreshold;
            Value = CalculateStatus(0);
            IsChanged = false;
        }

        public RegionStockStatus(int lowInStockThreshold, int quantity)
        {
            LowInStockThreshold = lowInStockThreshold;
            Value = CalculateStatus(quantity);
            IsChanged = false;
        }

        [JsonIgnore]
        public readonly int LowInStockThreshold;

        [JsonConverter(typeof(StringEnumConverter))]
        public StockStatus Value { get; private set; }

        [JsonIgnore]
        public bool IsChanged { get; private set; }

        public void Evaluate(int quantity)
        {
            var newStockStatus = CalculateStatus(quantity);

            IsChanged = IsChanged ||  Value != newStockStatus;

            Value = newStockStatus;
        }

        private StockStatus CalculateStatus(int quantity)
        {
            return quantity > 0 && (quantity < LowInStockThreshold)
                ? StockStatus.LowInStock
                : quantity > 0 && (quantity >= LowInStockThreshold)
                    ? StockStatus.InStock
                    : StockStatus.OutOfStock;
        }
    }
}
