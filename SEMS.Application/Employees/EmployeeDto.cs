using SEMS.Core.ValueObjects;

namespace SEMS.Application.Employees;

public sealed class EmployeeDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public Guid DepartmentId { get; init; }
    public Guid RoleId { get; init; }
    public DateTime HireDate { get; init; }
}

