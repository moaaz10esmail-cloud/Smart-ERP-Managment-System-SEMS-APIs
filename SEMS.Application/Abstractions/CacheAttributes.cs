namespace SEMS.Application.Abstractions;

[AttributeUsage(AttributeTargets.Class)]
public class CachedAttribute : Attribute
{
    public int ExpireInMinutes { get; set; } = 5;
    public string KeyPrefix { get; set; } = string.Empty;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class CacheInvalidationAttribute : Attribute
{
    public string KeyPrefix { get; }
    public CacheInvalidationAttribute(string keyPrefix)
    {
        KeyPrefix = keyPrefix;
    }
}
