using SEMS.Core.Common;

namespace SEMS.Core.Inventory;

public sealed class InventoryTransaction : BaseEntity
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string Type { get; set; } = "In";
}

