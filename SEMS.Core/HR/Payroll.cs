using SEMS.Core.Common;
using SEMS.Core.ValueObjects;

namespace SEMS.Core.HR;

public sealed class Payroll : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Money Salary { get; set; } = new Money(0, "USD");
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
}

