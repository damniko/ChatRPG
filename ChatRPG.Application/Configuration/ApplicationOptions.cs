using ChatRPG.Domain.Enums;

namespace ChatRPG.Application.Configuration;

public sealed class ApplicationOptions
{
    public const string Section = "Application";

    public bool EnableGraphVisualization { get; init; } = false;
    public VisualizationFormat GraphVisualizationFormat { get; init; } = VisualizationFormat.Pdf;
    public bool EnablePortraitGeneration { get; init; } = false;
}
