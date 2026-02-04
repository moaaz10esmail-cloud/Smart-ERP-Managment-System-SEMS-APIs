using SEMS.Core.Common;

namespace SEMS.Core.Inventory;

public sealed class Warehouse : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}

