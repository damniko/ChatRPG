namespace ChatRPG.Application.Abstractions;

public interface IVisualizationStore
{
    Task<byte[]> GetAsync(string location, CancellationToken ct = default);
    Task SaveAsync(string location, byte[] content, CancellationToken ct = default);
}