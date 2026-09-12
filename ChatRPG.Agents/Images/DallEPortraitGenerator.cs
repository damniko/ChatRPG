using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Entities;

namespace ChatRPG.Agents.Images;

public class DallEPortraitGenerator : IPortraitGenerator
{
    public Task<byte[]> GenerateAsync(Character character, string atmosphere, CancellationToken ct = default) => throw new NotImplementedException();
}
