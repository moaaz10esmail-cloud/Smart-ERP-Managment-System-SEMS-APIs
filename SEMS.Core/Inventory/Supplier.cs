using SEMS.Core.Common;

namespace SEMS.Core.Inventory;

public sealed class Supplier : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
}

