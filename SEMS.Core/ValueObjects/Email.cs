using System.Text.RegularExpressions;

namespace SEMS.Core.ValueObjects;

public sealed class Email
{
    public string Value { get; private set; }
    private Email() { Value = string.Empty; }
    public Email(string value)
    {
        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) throw new ArgumentException("Invalid email");
        Value = value;
    }
    public override string ToString() => Value;
}
