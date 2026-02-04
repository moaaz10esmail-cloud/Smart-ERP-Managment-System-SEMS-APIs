using SEMS.Core.Common;

namespace SEMS.Core.HR;

public sealed class Benefit : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

