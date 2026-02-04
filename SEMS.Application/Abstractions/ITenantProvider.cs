namespace SEMS.Application.Abstractions;

public interface ITenantProvider
{
    Guid? TenantId { get; }
}