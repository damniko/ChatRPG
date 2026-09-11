namespace ChatRPG.Application.Abstractions;

public interface IScenarioDocumentStore
{
    Task IngestAsync(int campaignId, byte[] pdf, CancellationToken ct = default);
    Task<IReadOnlyList<ScenarioExcerpt>> SearchAsync(int campaignId, string query, int take = 20, CancellationToken ct = default);
    Task DeleteAsync(int campaignId, CancellationToken ct = default);
}

public sealed record ScenarioExcerpt(string Content, double Score);
