using SEMS.Core.Common;

namespace SEMS.Core.Reports;

public sealed class Notification : BaseEntity
{
    public string Type { get; set; } = "Email";
    public string Message { get; set; } = string.Empty;
    public Guid? EmployeeId { get; set; }
}

