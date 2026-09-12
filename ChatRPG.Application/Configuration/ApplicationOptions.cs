namespace ChatRPG.Application.Configuration;

public sealed class ApplicationOptions
{
    public const string Section = "Application";

    public bool EnableGraphVisualization { get; init; } = false;
    public bool EnablePortraitGeneration { get; init; } = false;
}
