using ChatRPG.Application.Abstractions;
using ChatRPG.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ChatRPG.Infrastructure.Visualization;

public class FileSystemVisualizationStore(IOptions<InfrastructureOptions> options) : IVisualizationStore
{
    private string FullPath(string location) => Path.Combine(options.Value.VisualizationsRootPath, location);
    
    public async Task<byte[]> GetAsync(string location, CancellationToken ct = default)
    {
        return await File.ReadAllBytesAsync(FullPath(location), ct);
    }

    public async Task SaveAsync(string location, byte[] content, CancellationToken ct = default)
    {
        // TODO: Error handling, throw domain errors where relevant
        string fullPath = FullPath(location);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllBytesAsync(fullPath, content, ct);
    }
}