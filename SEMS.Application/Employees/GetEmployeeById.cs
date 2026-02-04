using MediatR;
using SEMS.Core.Common;
using SEMS.Core.HR;

namespace SEMS.Application.Employees;

public sealed record GetEmployeeById(Guid Id) : IRequest<EmployeeDto?>;

public sealed class GetEmployeeByIdHandler : IRequestHandler<GetEmployeeById, EmployeeDto?>
{
    private readonly IRepository<Employee> _repo;
    public GetEmployeeByIdHandler(IRepository<Employee> repo) => _repo = repo;
    public async Task<EmployeeDto?> Handle(GetEmployeeById request, CancellationToken cancellationToken)
    {
        var e = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (e is null) return null;
        return new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email.Value,
            Phone = e.Phone.Value,
            DepartmentId = e.DepartmentId,
            RoleId = e.RoleId,
            HireDate = e.HireDate
        };
    }
}

