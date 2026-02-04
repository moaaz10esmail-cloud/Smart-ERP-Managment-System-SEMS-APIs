using System.Text.RegularExpressions;

namespace SEMS.Core.ValueObjects;

public sealed class PhoneNumber
{
    public string Value { get; private set; }
    private PhoneNumber() { Value = string.Empty; }
    public PhoneNumber(string value)
    {
        if (!Regex.IsMatch(value, @"^[+]?[\d\s\-()]{7,}$")) throw new ArgumentException("Invalid phone");
        Value = value;
    }
    public override string ToString() => Value;
}
