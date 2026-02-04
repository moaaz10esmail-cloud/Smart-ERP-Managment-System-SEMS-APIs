using MediatR;
using SEMS.Core.Common;
using SEMS.Core.HR;
using SEMS.Application.Abstractions;

namespace SEMS.Application.Employees;

[Cached(ExpireInMinutes = 10, KeyPrefix = "EmployeesList")]
public sealed record GetEmployees(PagedQuery Query) : IRequest<PagedResult<EmployeeDto>>;

public sealed class GetEmployeesHandler : IRequestHandler<GetEmployees, PagedResult<EmployeeDto>>
{
    private readonly IRepository<Employee> _repo;

    public GetEmployeesHandler(IRepository<Employee> repo)
    {
        _repo = repo;
    }

    public async Task<PagedResult<EmployeeDto>> Handle(GetEmployees request, CancellationToken cancellationToken)
    {
        var result = await _repo.ListPagedAsync(request.Query, null, cancellationToken);

        return result.Map(e => new EmployeeDto
        {
            Id = e.Id,
            FirstName = e.FirstName,
            LastName = e.LastName,
            Email = e.Email.Value,
            Phone = e.Phone.Value,
            DepartmentId = e.DepartmentId,
            RoleId = e.RoleId,
            HireDate = e.HireDate
        });
    }
}
