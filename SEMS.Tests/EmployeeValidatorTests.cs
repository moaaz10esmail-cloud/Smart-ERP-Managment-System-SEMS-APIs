using SEMS.Core.ValueObjects;
using Xunit;

namespace SEMS.Tests;

public class EmailValueObjectTests
{
    [Fact]
    public void Invalid_email_throws_exception()
    {
        Assert.Throws<ArgumentException>(() => new Email("invalid"));
    }
}
