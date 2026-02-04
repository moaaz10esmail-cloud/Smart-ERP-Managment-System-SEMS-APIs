using SEMS.Core.Common;

namespace SEMS.Core.CRM;

public sealed class CommunicationLog : BaseEntity
{
    public Guid CustomerId { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string Channel { get; set; } = "Email";
    public string Notes { get; set; } = string.Empty;
}

