using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
using ChatRPG.Application.Abstractions;
using LangChain.Databases.Postgres;
using LangChain.DocumentLoaders;
using LangChain.Extensions;
using LangChain.Splitters.Text;
using Microsoft.Extensions.Options;

namespace ChatRPG.Agents.Scenarios;

internal class PgVectorScenarioDocumentStore(IOptions<ScenarioStoreOptions> options, IChatModelFactory models) : IScenarioDocumentStore
{
    private readonly PostgresVectorDatabase _db = new(options.Value.ConnectionString);
    
    public async Task IngestAsync(int campaignId, byte[] pdf, CancellationToken ct = default)
    {
        await _db.AddDocumentsFromAsync<PdfPigPdfLoader>(
            models.CreateEmbedding(),
            dimensions: options.Value.Dimensions,
            dataSource: DataSource.FromBytes(pdf),
            collectionName: CollectionFor(campaignId),
            textSplitter: new RecursiveCharacterTextSplitter(
                chunkSize: options.Value.ChunkSize,
                chunkOverlap: options.Value.ChunkOverlap),
            cancellationToken: ct);
    }

    public async Task<IReadOnlyList<ScenarioExcerpt>> SearchAsync(
        int campaignId,
        string query,
        int take = 20,
        CancellationToken ct = default)
    {
        var collection = await _db.GetCollectionAsync(CollectionFor(campaignId), ct);
        var documents = await collection.GetSimilarDocuments(models.CreateEmbedding(), query, amount: take, cancellationToken: ct);
        
        // TODO: Score
        return [.. documents.Select(d => new ScenarioExcerpt(d.PageContent, 1.0))];
    }

    public async Task DeleteAsync(int campaignId, CancellationToken ct = default)
    {
        await _db.DeleteCollectionAsync(CollectionFor(campaignId), ct);
    }
    
    private static string CollectionFor(int campaignId) => $"scenario-{campaignId}";
}
