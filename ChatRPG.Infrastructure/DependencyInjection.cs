using ChatRPG.Application.Abstractions;
using ChatRPG.Infrastructure.Configuration;
using ChatRPG.Infrastructure.Email;
using ChatRPG.Infrastructure.Persistence;
using ChatRPG.Infrastructure.Visualization;
using Microsoft.Extensions.DependencyInjection;

namespace ChatRPG.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddOptions<InfrastructureOptions>()
            .BindConfiguration(InfrastructureOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddDbContext<ChatRpgDbContext>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserDirectory, UserDirectory>();
        services.AddScoped<IVisualizationStore, FileSystemVisualizationStore>();
        return services;
    }
}
