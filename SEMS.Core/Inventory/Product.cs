using SEMS.Core.Common;

namespace SEMS.Core.Inventory;

public sealed class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string SKU { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public ICollection<Stock> Stocks { get; set; } = new List<Stock>();
}

