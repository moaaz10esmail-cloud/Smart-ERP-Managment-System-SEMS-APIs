using SEMS.Core.Common;

namespace SEMS.Core.CRM;

public sealed class Opportunity : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string Title { get; set; } = string.Empty;
    public decimal EstimatedValue { get; set; }
}

