using System.Runtime.CompilerServices;
using System.Text;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Gameplay;

public class GameTurnService(
    INarrator narrator,
    IInputExaminer examiner,
    IGraphNavigator navigator,
    IArchivist archivist,
    IUnitOfWork unitOfWork) : IGameTurnService
{
    public async IAsyncEnumerable<TurnEvent> PlayTurnAsync(
        Campaign campaign,
        PlayerAction action,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        AdherenceVerdict? verdict = null;
        string? graphSummary = null;

        if (!campaign.IsOpenWorld)
        {
            verdict = await examiner.ExamineAsync(campaign, action.Text, ct);
            if (!verdict.IsAllowed)
            {
                yield return new TurnEvent.InputRejected(verdict.Reasoning);
            }
            else
            {
                graphSummary = await navigator.ReviewGraphAsync(campaign, action.Text, verdict, ct);
            }
        }

        yield return new TurnEvent.NarrationStarted();

        var narration = new StringBuilder();

        var request = new NarrationRequest.PlayerTurn(campaign, action, verdict, graphSummary);

        await foreach (string chunk in narrator.NarrateStreamingAsync(request, ct))
        {
            narration.Append(chunk);
            yield return new TurnEvent.NarrationChunk(chunk);
        }
        
        yield return new TurnEvent.NarrationCompleted(narration.ToString());

        if (IsGameOver(campaign))
        {
            string epilogue = await narrator.NarrateAsync(
                new NarrationRequest.Epilogue(campaign, action, narration.ToString()), ct);
            campaign.GameOver = true;
            yield return new TurnEvent.GameEnded(epilogue);
        }

        yield return new TurnEvent.ArchivingStarted();
        await archivist.ApplyNarrativeChangesAsync(campaign, action.Text, narration.ToString(), ct);
        await archivist.AppendMessagesAsync(campaign, action.Text, narration.ToString(), verdict, null, ct);
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
            var verdict = new AdherenceVerdict(true, "The scenario is created directly from the scenario document.");
            graphUpdateSummary = await navigator.ReviewGraphAsync(campaign, openingPrompt, verdict, ct);
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
        await archivist.ApplyNarrativeChangesAsync(campaign, openingPrompt, narration.ToString(), ct);
        await archivist.AppendMessagesAsync(campaign, openingPrompt, narration.ToString(), null, null, ct);
        await unitOfWork.SaveChangesAsync(ct);

        yield return new TurnEvent.CampaignSaved();
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
