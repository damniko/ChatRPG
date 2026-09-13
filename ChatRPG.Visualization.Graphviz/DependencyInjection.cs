using ChatRPG.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace ChatRPG.Visualization.Graphviz;

public static class DependencyInjection
{
    public static IServiceCollection AddGraphvizVisualization(this IServiceCollection services)
    {
        services.AddScoped<INarrativeGraphRenderer, GraphvizNarrativeGraphRenderer>();
        return services;
    }
}