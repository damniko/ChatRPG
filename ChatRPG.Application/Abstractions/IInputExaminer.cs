using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface IInputExaminer
{
    Task<AdherenceVerdict> ExamineAsync(Campaign campaign, string playerInput, CancellationToken ct = default);
}

public sealed record AdherenceVerdict(bool IsAllowed, string Reasoning);
