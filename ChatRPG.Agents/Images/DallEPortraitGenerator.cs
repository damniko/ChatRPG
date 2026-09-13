using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Images;

internal sealed class DallEPortraitGenerator(
    IChatModelFactory models,
    IInstructionCatalog instructions) : IPortraitGenerator
{
    // TODO: This is WIP
    public async Task<byte[]> GenerateAsync(Character character, string atmosphere, CancellationToken ct = default)
    {
        string prompt = new PromptTemplate(instructions.Get(InstructionKey.PortraitGeneration))
            .Render(new Dictionary<string, string>
            {
                ["characterName"] = character.Name,
                ["characterDescription"] = character.Description,
                ["atmosphereDescription"] = atmosphere
            });
        var model = models.CreateTextToImage();
        var response = await model.GenerateImageAsync(prompt, cancellationToken: ct);
        var image = response.Images[0];

        return image.ToByteArray();
    }
}
