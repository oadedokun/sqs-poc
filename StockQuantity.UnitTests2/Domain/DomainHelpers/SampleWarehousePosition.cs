using System;
using System.Collections.Generic;

namespace StockQuantity.UnitTests2.Domain.DomainHelpers
{
    public struct SampleWarehousePosition
    {
        private static readonly Dictionary<WarehouseStockPositionType, SampleWarehousePosition> positions =
            new Dictionary<WarehouseStockPositionType, SampleWarehousePosition>
            {
                { WarehouseStockPositionType.NoStockAtAll, new SampleWarehousePosition { Pickable = 0, Allocated = 0, Reserved = 0} },
                { WarehouseStockPositionType.OutOfStock, new SampleWarehousePosition { Pickable = 20, Allocated = 10, Reserved = 10} },
                { WarehouseStockPositionType.OneItemAvailable, new SampleWarehousePosition { Pickable = 5, Allocated = 4, Reserved = 0} },
                { WarehouseStockPositionType.JustBelowThreshold, new SampleWarehousePosition { Pickable = 11, Allocated = 1, Reserved = 1} },
                { WarehouseStockPositionType.AtThreshold, new SampleWarehousePosition { Pickable = 12, Allocated = 1, Reserved = 1} },
                { WarehouseStockPositionType.AboveThreshold, new SampleWarehousePosition { Pickable = 20, Allocated = 1, Reserved = 1} },
            };


        public static SampleWarehousePosition FromPositionType(WarehouseStockPositionType positionType)
        {
            if (positions.ContainsKey(positionType)) return positions[positionType];
            throw new ArgumentException($"PositionType of { positionType } not setup");
        }

        public int Pickable { get; set; }
        public int Allocated { get; set; }
        public int Reserved { get; set; }   
    }
}