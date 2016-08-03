namespace StockQuantity2.Domain
{
    public class RegionalStockPosition
    {
        public RegionalStockPosition() { }

        public RegionalStockPosition(string region, int available, int reserved)
        {
            Region = region;
            AvailableQuantity = available;
            ReservedQuantity = reserved;
            Status = CalculateStockStatus(available);
        }

        public string Region { get; private set; }

        public int AvailableQuantity { get; private set; }

        public int ReservedQuantity { get; private set; }

        public StockStatus Status { get; private set; }

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