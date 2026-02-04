using SEMS.Core.Common;

namespace SEMS.Core.HR;

public sealed class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}

