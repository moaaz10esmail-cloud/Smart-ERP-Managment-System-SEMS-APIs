using FluentValidation;
using MediatR;
using SEMS.Core.Common;
using SEMS.Core.Finance;
using SEMS.Core.Enums;
using SEMS.Core.ValueObjects;
using SEMS.Core.CRM;

namespace SEMS.Application.Invoices;

public sealed record CreateInvoice(Guid CustomerId, Guid BankAccountId, decimal Total, string Currency, DateTime DueDate) : IRequest<Guid>;

public sealed class CreateInvoiceValidator : AbstractValidator<CreateInvoice>
{
    public CreateInvoiceValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.BankAccountId).NotEmpty();
        RuleFor(x => x.Total).GreaterThan(0);
        RuleFor(x => x.Currency).NotEmpty();
    }
}

public sealed class CreateInvoiceHandler : IRequestHandler<CreateInvoice, Guid>
{
    private readonly IRepository<Invoice> _repo;
    private readonly IRepository<Customer> _customers;
    private readonly IRepository<BankAccount> _bankAccounts;
    private readonly IUnitOfWork _uow;
    public CreateInvoiceHandler(IRepository<Invoice> repo, IRepository<Customer> customers, IRepository<BankAccount> bankAccounts, IUnitOfWork uow)
    {
        _repo = repo;
        _customers = customers;
        _bankAccounts = bankAccounts;
        _uow = uow;
    }
    public async Task<Guid> Handle(CreateInvoice request, CancellationToken cancellationToken)
    {
        var customer = await _customers.GetByIdAsync(request.CustomerId, cancellationToken);
        if (customer is null) throw new InvalidOperationException("Customer must exist before creating an invoice.");
        var bank = await _bankAccounts.GetByIdAsync(request.BankAccountId, cancellationToken);
        if (bank is null) throw new InvalidOperationException("Company bank account must exist before creating an invoice.");

        var inv = new Invoice
        {
            CustomerId = request.CustomerId,
            BankAccountId = request.BankAccountId,
            Total = new Money(request.Total, request.Currency),
            Status = InvoiceStatus.Draft,
            DueDate = request.DueDate
        };
        await _repo.AddAsync(inv, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return inv.Id;
    }
}
