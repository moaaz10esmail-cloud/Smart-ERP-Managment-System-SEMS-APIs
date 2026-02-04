using SEMS.Core.Common;

namespace SEMS.Core.CRM;

public sealed class SalesOrder : BaseEntity
{
    public Guid CustomerId { get; set; }
    public DateTime OrderedOn { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
}

