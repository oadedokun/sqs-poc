namespace StockQuantity2.Domain
{
    public class RegionalStockPosition
    {
        public RegionalStockPosition(string region)
        {
            Region = region;
            Status = CalculateStockStatus(AvailableQuantity);
        }

        public RegionalStockPosition(string region, int available, int reserved) : this(region)
        {
            AvailableQuantity = available;
            ReservedQuantity = reserved;
            Status = CalculateStockStatus(available);
        }

        public string Region { get; }

        public int AvailableQuantity { get; }

        public int ReservedQuantity { get; }

        public StockStatus Status { get; }

        private static StockStatus CalculateStockStatus(int available)
        {
            return available <= 0
                ? StockStatus.OutOfStock
                : available < Constants.LowInStockThreshold
                    ? StockStatus.LowInStock
                    : StockStatus.InStock;
        }
    }
}