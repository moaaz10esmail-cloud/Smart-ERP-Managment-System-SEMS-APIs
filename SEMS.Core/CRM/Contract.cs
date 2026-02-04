using SEMS.Core.Common;

namespace SEMS.Core.CRM;

public sealed class Contract : BaseEntity
{
    public Guid CustomerId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

