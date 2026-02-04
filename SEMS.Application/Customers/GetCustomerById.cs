using MediatR;
using SEMS.Core.Common;
using SEMS.Core.CRM;

namespace SEMS.Application.Customers;

public sealed record GetCustomerById(Guid Id) : IRequest<CustomerDto?>;

public sealed class GetCustomerByIdHandler : IRequestHandler<GetCustomerById, CustomerDto?>
{
    private readonly IRepository<Customer> _repo;
    public GetCustomerByIdHandler(IRepository<Customer> repo) => _repo = repo;
    public async Task<CustomerDto?> Handle(GetCustomerById request, CancellationToken cancellationToken)
    {
        var c = await _repo.GetByIdAsync(request.Id, cancellationToken);
        if (c is null) return null;
        return new CustomerDto
        {
            Id = c.Id,
            Name = c.Name,
            Email = c.Email.Value,
            Phone = c.Phone.Value
        };
    }
}

