using FluentValidation;
using MediatR;
using SEMS.Core.Common;
using SEMS.Core.DomainEvents;
using SEMS.Core.Projects;

namespace SEMS.Application.Projects;

public sealed record CreateProject(string Name, DateTime StartDate) : IRequest<Guid>;

public sealed class CreateProjectValidator : AbstractValidator<CreateProject>
{
    public CreateProjectValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
    }
}

public sealed class CreateProjectHandler : IRequestHandler<CreateProject, Guid>
{
    private readonly IRepository<Project> _repo;
    private readonly IUnitOfWork _uow;
    public CreateProjectHandler(IRepository<Project> repo, IUnitOfWork uow)
    {
        _repo = repo;
        _uow = uow;
    }
    public async Task<Guid> Handle(CreateProject request, CancellationToken cancellationToken)
    {
        var p = new Project { Name = request.Name, StartDate = request.StartDate };
        p.AddDomainEvent(new ProjectCreated(p.Id));
        await _repo.AddAsync(p, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);
        return p.Id;
    }
}

