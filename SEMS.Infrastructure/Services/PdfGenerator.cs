namespace SEMS.Infrastructure.Services;

public interface IPdfGenerator
{
    Task<byte[]> GenerateAsync(string html, CancellationToken cancellationToken = default);
}

public class PdfGenerator : IPdfGenerator
{
    public Task<byte[]> GenerateAsync(string html, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(Array.Empty<byte>());
    }
}

