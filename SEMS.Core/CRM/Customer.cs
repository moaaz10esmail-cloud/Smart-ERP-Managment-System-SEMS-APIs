using SEMS.Core.Common;
using SEMS.Core.ValueObjects;
using SEMS.Core.Finance;

namespace SEMS.Core.CRM;

public sealed class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Email Email { get; set; } = new Email("customer@example.com");
    public PhoneNumber Phone { get; set; } = new PhoneNumber("+1000000000");
    public Address? Address { get; set; }
    public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}

