using SEMS.Core.Common;

namespace SEMS.Core.Reports;

public sealed class ReportTemplate : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
}

