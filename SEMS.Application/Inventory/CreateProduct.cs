using FluentValidation;
using MediatR;
using SEMS.Core.Common;
using SEMS.Core.Inventory;

namespace SEMS.Application.Inventory;

public sealed record CreateProduct(string Name, string SKU, decimal Price) : IRequest<Guid>;

public sealed class CreateProductValidator : AbstractValidator<CreateProduct>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.SKU).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}

public sealed class CreateProductHandler : IRequestHandler<CreateProduct, Guid>
{
    private readonly IRepository<Product> _repo;
    private readonly IUnitOfWork _uow;
    public CreateProductHandler(IRepository<Product> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }
    public async Task<Guid> Handle(CreateProduct request, CancellationToken cancellationToken)
    {
        var p = new Product { Name = request.Name, SKU = request.SKU, Price = request.Price };
        await _repo.AddAsync(p, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return p.Id;
    }
}

