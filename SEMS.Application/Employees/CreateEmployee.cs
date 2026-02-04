using FluentValidation;
using MediatR;
using SEMS.Core.Common;
using SEMS.Core.HR;
using SEMS.Core.DomainEvents;
using SEMS.Core.ValueObjects;
using SEMS.Application.Abstractions;

namespace SEMS.Application.Employees;

[CacheInvalidation("EmployeesList")]
public sealed record CreateEmployee(string FirstName, string LastName, string Email, string Phone, Guid DepartmentId, Guid? RoleId) : IRequest<Guid>;

public sealed class CreateEmployeeValidator : AbstractValidator<CreateEmployee>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.LastName).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Phone).MinimumLength(7);
    }
}

public sealed class CreateEmployeeHandler : IRequestHandler<CreateEmployee, Guid>
{
    private readonly IRepository<Employee> _repo;
    private readonly IUnitOfWork _uow;
    public CreateEmployeeHandler(IRepository<Employee> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }
    public async Task<Guid> Handle(CreateEmployee request, CancellationToken cancellationToken)
    {
        var e = new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = new Email(request.Email),
            Phone = new PhoneNumber(request.Phone),
            DepartmentId = request.DepartmentId,
            RoleId = request.RoleId!.Value
        };
        e.AddDomainEvent(new EmployeeCreated(e.Id));
        await _repo.AddAsync(e, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return e.Id;
    }
}
