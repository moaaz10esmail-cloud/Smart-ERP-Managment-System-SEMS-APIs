using SEMS.Core.Common;

namespace SEMS.Core.Inventory;

public sealed class PurchaseOrder : BaseEntity
{
    public Guid SupplierId { get; set; }
    public DateTime OrderedOn { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
}

