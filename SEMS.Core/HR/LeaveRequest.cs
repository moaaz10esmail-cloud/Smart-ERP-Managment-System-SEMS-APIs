using SEMS.Core.Common;
using SEMS.Core.Enums;

namespace SEMS.Core.HR;

public sealed class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string Reason { get; set; } = string.Empty;
}

