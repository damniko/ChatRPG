using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface IPortraitGenerator
{
    Task<byte[]> GenerateAsync(Character character, string atmosphere, CancellationToken ct = default);
}
