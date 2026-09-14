using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Campaigns;
using ChatRPG.Application.Configuration;
using ChatRPG.Application.Gameplay;
using ChatRPG.Application.Usage;
using ChatRPG.Application.Visualization;
using ChatRPG.Domain.Combat;
using Microsoft.Extensions.DependencyInjection;

namespace ChatRPG.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddOptions<ApplicationOptions>()
            .BindConfiguration(ApplicationOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<IRandomSource, SystemRandomSource>();
        services.AddSingleton<CombatResolver>();

        // Registered once as the concrete type and forwarded, so all three handles resolve to the
        // same accumulator. Scoped here means per Blazor circuit, which is what a session is.
        services.AddScoped<LlmUsageAccumulator>();
        services.AddScoped<ILlmUsageSink>(sp => sp.GetRequiredService<LlmUsageAccumulator>());
        services.AddScoped<ILlmUsageTracker>(sp => sp.GetRequiredService<LlmUsageAccumulator>());
        services.AddScoped<ILlmUsageReadout>(sp => sp.GetRequiredService<LlmUsageAccumulator>());

        services.AddScoped<IGameTurnService, GameTurnService>();
        services.AddScoped<CreateCampaignHandler>();
        services.AddScoped<DeleteCampaignHandler>();
        services.AddScoped<INarrativeGraphVisualizer, NarrativeGraphVisualizer>();

        return services;
    }
}
