using SEMS.Core.Common;

namespace SEMS.Core.CRM;

public sealed class Complaint : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string Description { get; set; } = string.Empty;
    public bool Resolved { get; set; }
}

