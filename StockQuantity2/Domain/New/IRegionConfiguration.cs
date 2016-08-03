using System.Collections.Generic;

namespace StockQuantity2.Domain
{
    public interface IRegionConfiguration
    {
        IEnumerable<string> RegionsToMaintain { get; }
    }
}