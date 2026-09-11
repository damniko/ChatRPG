using ChatRPG.Domain.Combat;

namespace ChatRPG.Application.Gameplay;

public class SystemRandomSource : IRandomSource
{
    public double NextDouble() => Random.Shared.NextDouble();

    public int Next(int min, int max) => Random.Shared.Next(min, max);
}
