using SEMS.Core.Common;
using SEMS.Core.Enums;

namespace SEMS.Core.HR;

public sealed class Attendance : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow.Date;
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
}

