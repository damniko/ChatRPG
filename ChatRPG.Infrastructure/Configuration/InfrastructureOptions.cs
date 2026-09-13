namespace ChatRPG.Infrastructure.Configuration;

public class InfrastructureOptions
{
    public const string Section = "Infrastructure";

    public required string VisualizationsRootPath { get; set; }
}