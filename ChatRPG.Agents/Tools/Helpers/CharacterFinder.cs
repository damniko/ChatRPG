using System.Text;
using System.Text.Json;
using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Domain.Entities;
using LangChain.Providers;
using Microsoft.Extensions.Options;
using MessageRole = ChatRPG.Domain.Enums.MessageRole;

namespace ChatRPG.Agents.Tools.Helpers;

// TODO: Abstract this (interface, maybe another class/place? could it be a tool?)
internal sealed class CharacterFinder(
    IChatModelFactory models,
    IInstructionCatalog instructions,
    IOptions<AgentOptions> options,
    IToolDataTextParser parser)
{
    private const double Temperature = 0.1;

    public async Task<Character?> FindAsync(Campaign campaign, string input, string instruction, CancellationToken ct = default)
    {
        var model = models.CreateChat(Temperature);
        var query = new StringBuilder(instructions.Get(InstructionKey.FindCharacter));
        
        AppendSummary(campaign, ref query);
        AppendCharacters(campaign, ref query);
        query.AppendLine($"\n\nFind the character using the following content: {input}");
        
        string prompt = new PromptTemplate(query.ToString()).Render(
            new Dictionary<string, string>
            {
                ["instruction"] = instruction
            });

        var response = await model.GenerateAsync(ChatRequest.ToChatRequest(prompt), cancellationToken: ct);
        string answer = response.LastMessageContent;

        if (!parser.TryParse<ToolData.Character>(answer, out var toolData, out string? _))
            return null;

        var charData = toolData!;
        var character = campaign.Characters.FirstOrDefault(c => c.Name == charData.Name && c.Description == charData.Description && c.Type.ToString() == charData.Type);
        
        return character;
    }

    private void AppendSummary(Campaign campaign, ref StringBuilder query)
    {
        query.Append($"\n\nThe story until now: {campaign.GameSummary}");

        if (!options.Value.IncludePreviousMessages || campaign.Messages.Count == 0)
            return;

        query.Append(
            "\n\nUse these previous messages as context. They only serve to give a hint of the current scenario:");
            
        var messages = campaign.Messages
            .Where(m => m.Role != MessageRole.System)
            .OrderBy(m => m.Timestamp)
            .TakeLast(options.Value.PreviousMessagesCount);
        foreach (var message in messages)
        {
            if (message.Role == MessageRole.User)
            {
                query.Append($"\nPlayer: {message.Content}");
                if (message.Verdict != null)
                    query.Append($"\nAdherence verdict: {message.Verdict.Content}");
            }
            else
            {
                query.AppendLine($"\nGM: {message.Content}");
            }
        }
    }

    private static void AppendCharacters(Campaign campaign, ref StringBuilder query)
    {
        query.Append("\n\nHere is the list of all characters present in the story:\n\n");

        var characters = campaign.Characters.Select(c => new
        {
            c.Name,
            c.Description,
            c.Type
        });
        query.AppendLine(JsonSerializer.Serialize(characters));

        query.Append($"\n\nThe player is {campaign.Player.Name}. First-person pronouns refer to them.");
    }
}
