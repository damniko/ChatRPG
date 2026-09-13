using System.ComponentModel.DataAnnotations;

namespace ChatRPG.Infrastructure.Configuration;

public class InfrastructureOptions
{
    public const string Section = "Infrastructure";

    [Required] public string VisualizationsRootPath { get; set; } = "/";
}