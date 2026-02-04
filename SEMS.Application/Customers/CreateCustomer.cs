using FluentValidation;
using MediatR;
using SEMS.Core.Common;
using SEMS.Core.CRM;
using SEMS.Core.ValueObjects;

namespace SEMS.Application.Customers;

public sealed record CreateCustomer(string Name, string Email, string Phone) : IRequest<Guid>;

public sealed class CreateCustomerValidator : AbstractValidator<CreateCustomer>
{
    public CreateCustomerValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Phone).MinimumLength(7);
    }
}

public sealed class CreateCustomerHandler : IRequestHandler<CreateCustomer, Guid>
{
    private readonly IRepository<Customer> _repo;
    private readonly IUnitOfWork _uow;
    public CreateCustomerHandler(IRepository<Customer> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }
    public async Task<Guid> Handle(CreateCustomer request, CancellationToken cancellationToken)
    {
        var c = new Customer
        {
            Name = request.Name,
            Email = new Email(request.Email),
            Phone = new PhoneNumber(request.Phone)
        };
        await _repo.AddAsync(c, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return c.Id;
    }
}

