using ChatRPG.Domain.Entities;

namespace ChatRPG.Application.Abstractions;

public interface IActionExaminer
{
    Task<ActionRuling> ExamineAsync(Campaign campaign, string playerInput, CancellationToken ct = default);
}
