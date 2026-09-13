using ChatRPG.Application.Abstractions;
using ChatRPG.Application.Campaigns;
using ChatRPG.Application.Configuration;
using ChatRPG.Application.Gameplay;
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

        services.AddScoped<IGameTurnService, GameTurnService>();
        services.AddScoped<CreateCampaignHandler>();
        services.AddScoped<DeleteCampaignHandler>();
        services.AddScoped<INarrativeGraphVisualizer, NarrativeGraphVisualizer>();

        return services;
    }
}
