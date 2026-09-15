using System.Text.Json;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools;
using ChatRPG.Application.Abstractions;

namespace ChatRPG.Agents.ReAct.Agents;

internal sealed class ReActArchivistAgent(
    IChatModelFactory models,
    GameSummaryFormatter summaryFormatter,
    IInstructionCatalog instructions,
    IToolFactory toolFactory) : IArchivist
{
    private const double NarrativeChangesTemperature = 0.7;

    public async Task<ArchiveResult> ApplyNarrativeChangesAsync(ArchiveRequest request, CancellationToken ct = default)
    {
        string characters = JsonSerializer.Serialize(request.Characters.Select(c => new { c.Name, c.Description }));
        string locations = JsonSerializer.Serialize(request.Locations);
        string gameSummary = await summaryFormatter.FormatAsync(request.CampaignId, request.GameSummary, ct);

        var collector = new ChangeCollector();
        var agent = new ReActAgent(models.CreateChat(NarrativeChangesTemperature, nameof(ReActArchivistAgent)), instructions.Get(InstructionKey.Archive))
        {
            Variables =
            {
                ["characters"] = characters,
                ["locations"] = locations,
                ["player_character"] = request.Characters.First(c => c.IsPlayer).Name,
                ["gameSummary"] = gameSummary
            },
            Tools =
            {
                toolFactory.GetUpdateCharacterTool(request.Characters, collector),
                toolFactory.GetUpdateLocationTool(request.Locations, collector)
            }
        };

        await agent.RunAsync(ArchiveInputFormatter.Format(request), ct);

        return new ArchiveResult(
            collector.CharacterChanges, collector.NewCharacters, collector.LocationChanges);
    }
}
