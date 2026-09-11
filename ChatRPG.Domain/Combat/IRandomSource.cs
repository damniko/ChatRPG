namespace ChatRPG.Domain.Combat;

public interface IRandomSource
{
    double NextDouble();
    int Next(int minInclusive, int maxExclusive);
}
