using SEMS.Core.Common;

namespace SEMS.Core.Projects;

public sealed class TimeLog : BaseEntity
{
    public Guid ProjectId { get; set; }
    public Guid EmployeeId { get; set; }
    public DateTime LoggedOn { get; set; } = DateTime.UtcNow;
    public double Hours { get; set; }
}

