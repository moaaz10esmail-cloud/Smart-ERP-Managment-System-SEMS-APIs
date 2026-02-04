namespace SEMS.Application.Abstractions;

public interface IUserContext
{
    IEnumerable<string> Roles { get; }
    string? UserId { get; }
    string? IpAddress { get; }
}
