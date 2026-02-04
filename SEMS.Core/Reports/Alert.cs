using SEMS.Core.Common;

namespace SEMS.Core.Reports;

public sealed class Alert : BaseEntity
{
    public string Severity { get; set; } = "Info";
    public string Message { get; set; } = string.Empty;
    public DateTime TriggeredOn { get; set; } = DateTime.UtcNow;
}

