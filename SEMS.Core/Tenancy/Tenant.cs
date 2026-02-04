using SEMS.Core.Common;

namespace SEMS.Core.Tenancy;

public sealed class Tenant : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Identifier { get; set; } = string.Empty; // e.g. "company-a" for subdomain or unique lookup
    public bool IsActive { get; set; } = true;
    public DateTime ValidUntil { get; set; } = DateTime.UtcNow.AddYears(1);
}