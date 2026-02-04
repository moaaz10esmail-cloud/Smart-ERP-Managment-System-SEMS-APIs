using SEMS.Core.Common;
using SEMS.Core.ValueObjects;

namespace SEMS.Core.HR;

public sealed class Employee : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Email Email { get; set; } = new Email("noreply@example.com");
    public PhoneNumber Phone { get; set; } = new PhoneNumber("+1000000000");
    public Guid DepartmentId { get; set; }
    public Department? Department { get; set; }
    public Guid RoleId { get; set; }
    public Role? Role { get; set; }
    public Address? Address { get; set; }
    public DateTime HireDate { get; set; } = DateTime.UtcNow;
    
    public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}

