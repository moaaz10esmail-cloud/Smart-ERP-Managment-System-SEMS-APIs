using SEMS.Core.Common;
using SEMS.Core.ValueObjects;

namespace SEMS.Core.CRM;

public sealed class Contact : BaseEntity
{
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Email Email { get; set; } = new Email("contact@example.com");
    public PhoneNumber Phone { get; set; } = new PhoneNumber("+1000000000");
}

