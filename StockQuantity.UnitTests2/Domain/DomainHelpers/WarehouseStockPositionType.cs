namespace StockQuantity.UnitTests2.Domain.DomainHelpers
{
    public enum WarehouseStockPositionType
    {
        NoStockAtAll,
        OutOfStock,
        OneItemAvailable,
        JustBelowThreshold,
        AtThreshold,
        AboveThreshold
    }
}