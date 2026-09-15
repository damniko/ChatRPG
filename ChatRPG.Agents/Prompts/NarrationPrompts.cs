using ChatRPG.Application.Gameplay;

namespace ChatRPG.Agents.Prompts;

internal static class NarrationPrompts
{
    public static InstructionKey SystemPromptFor(NarrationRequest request)
    {
        return request.Campaign.IsOpenWorld ? InstructionKey.Narrate : InstructionKey.NarrateWithGraph;
    }

    public static InstructionKey InstructionFor(NarrationRequest request)
    {
        return request switch
        {
            NarrationRequest.Opening => InstructionKey.Initial,
            NarrationRequest.Epilogue => InstructionKey.GameOver,
            NarrationRequest.PlayerTurn { Action.Kind: PlayerActionKind.Do } turn =>
                turn.Ruling is null ? InstructionKey.Do : InstructionKey.DoWithRuling,
            NarrationRequest.PlayerTurn { Action.Kind: PlayerActionKind.Say } turn =>
                turn.Ruling is null ? InstructionKey.Say : InstructionKey.SayWithRuling,
            _ => throw new ArgumentOutOfRangeException(nameof(request), request, null)
        };
    }
}
