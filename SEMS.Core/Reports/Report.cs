using SEMS.Core.Common;

namespace SEMS.Core.Reports;

public sealed class Report : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}

