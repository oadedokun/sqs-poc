namespace StockQuantity.UnitTests.Domain
{
    public interface IDomainEntityBuilder<out TEntityType> where TEntityType  : class
    {
        TEntityType Build();
    }
}