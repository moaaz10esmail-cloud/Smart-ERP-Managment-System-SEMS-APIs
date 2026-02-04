using SEMS.Core.Common;

namespace SEMS.Core.HR;

public sealed class Position : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
}

