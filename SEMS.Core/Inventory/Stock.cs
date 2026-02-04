using SEMS.Core.Common;

namespace SEMS.Core.Inventory;

public sealed class Stock : BaseEntity
{
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    public int Quantity { get; set; }
    public Guid WarehouseId { get; set; }
    public Warehouse? Warehouse { get; set; }
}

