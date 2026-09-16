using System.Runtime.CompilerServices;
using System.Text;
using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Mapping;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Gameplay;

public class GameTurnService(
    INarrator narrator,
    IActionExaminer examiner,
    IGraphNavigator navigator,
    IArchivist archivist,
    ISummarizer summarizer,
    IUnitOfWork unitOfWork) : IGameTurnService
{
    public async IAsyncEnumerable<TurnEvent> PlayTurnAsync(
        Campaign campaign,
        PlayerAction action,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        ActionRuling? ruling = null;
        string? graphSummary = null;
        string? epilogue = null;

        if (!campaign.IsOpenWorld)
        {
            ruling = await examiner.ExamineAsync(campaign, action.Text, ct);
            yield return new TurnEvent.RulingIssued(ruling);
            if (!ruling.AttemptProceeds)
            {
                yield return new TurnEvent.InputRejected(ruling.Reasoning);
            }
            else
            {
                graphSummary = await navigator.ReviewGraphAsync(campaign, action.Text, ruling, ct);
            }
        }

        yield return new TurnEvent.NarrationStarted();

        var narration = new StringBuilder();

        var request = new NarrationRequest.PlayerTurn(campaign, action, ruling, graphSummary);

        await foreach (string chunk in narrator.NarrateStreamingAsync(request, ct))
        {
            narration.Append(chunk);
            yield return new TurnEvent.NarrationChunk(chunk);
        }
        
        yield return new TurnEvent.NarrationCompleted(narration.ToString());

        if (IsGameOver(campaign))
        {
            epilogue = await narrator.NarrateAsync(
                new NarrationRequest.Epilogue(campaign, action, narration.ToString()), ct);
            campaign.GameOver = true;
            yield return new TurnEvent.GameEnded(epilogue);
        }

        yield return new TurnEvent.ArchivingStarted();
        await archivist.ApplyNarrativeChangesAsync(ArchiveTurn(campaign, action.Text, narration.ToString()), ct);

        campaign.GameSummary = await summarizer.SummarizeAsync(
            new SummaryRequest(campaign.GameSummary, action.Text, narration.ToString(), ruling), ct);

        campaign.Messages.Add(new PlayerMessage(campaign, action.Text, ruling));
        campaign.Messages.Add(new NarrationMessage(campaign, narration.ToString()));
        if (epilogue is not null)
        {
            campaign.Messages.Add(new NarrationMessage(campaign, epilogue));
        }

        await unitOfWork.SaveChangesAsync(ct);

        yield return new TurnEvent.CampaignSaved();
    }

    public async IAsyncEnumerable<TurnEvent> StartCampaignAsync(
        Campaign campaign,
        string openingPrompt,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        string? graphUpdateSummary = null;
        if (!campaign.IsOpenWorld)
        {
            graphUpdateSummary = await navigator.ReviewGraphAsync(
                campaign, openingPrompt, ActionRuling.ScenarioOpening, ct);
        }
        yield return new TurnEvent.NarrationStarted();

        var narration = new StringBuilder();

        var request = new NarrationRequest.Opening(campaign, openingPrompt, graphUpdateSummary);

        await foreach (string chunk in narrator.NarrateStreamingAsync(request, ct))
        {
            narration.Append(chunk);
            yield return new TurnEvent.NarrationChunk(chunk);
        }
        
        yield return new TurnEvent.NarrationCompleted(narration.ToString());
        
        yield return new TurnEvent.ArchivingStarted();
        await archivist.ApplyNarrativeChangesAsync(ArchiveOpening(campaign, narration.ToString()), ct);

        campaign.GameSummary = await summarizer.SummarizeAsync(
            new SummaryRequest(campaign.GameSummary, openingPrompt, narration.ToString(), null), ct);

        campaign.Messages.Add(new NarrationMessage(campaign, narration.ToString()));

        await unitOfWork.SaveChangesAsync(ct);

        yield return new TurnEvent.CampaignSaved();
    }

    
    // TODO: The returned ArchiveResult is not applied back to the campaign yet, so character and
    // location changes the archivist collects are still dropped.
    private static ArchiveRequest.Turn ArchiveTurn(Campaign campaign, string playerInput, string narration)
    {
        return new ArchiveRequest.Turn(
            campaign.Id, campaign.GameSummary, Views(campaign), LocationNames(campaign), playerInput, narration);
    }

    private static ArchiveRequest.Opening ArchiveOpening(Campaign campaign, string narration)
    {
        return new ArchiveRequest.Opening(
            campaign.Id, campaign.GameSummary, Views(campaign), LocationNames(campaign), narration);
    }

    private static IReadOnlyList<CharacterView> Views(Campaign campaign)
    {
        return campaign.Characters.Select(c => c.ToView()).ToList();
    }

    private static IReadOnlyList<string> LocationNames(Campaign campaign)
    {
        return campaign.Locations.Select(l => l.Name).ToList();
    }

    private static bool IsGameOver(Campaign campaign)
    {
        if (campaign.IsOpenWorld)
        {
            return campaign.Player.CurrentHealth <= 0;
        }

        return campaign.Player.CurrentHealth <= 0 ||
            campaign.NarrativeGraph!.GetEndNode()?.NodeStatus is not NarrativeNode.Status.Undiscovered;
    }
}
