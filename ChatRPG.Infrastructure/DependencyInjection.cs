using ChatRPG.Application.Abstractions;
using ChatRPG.Infrastructure.Email;
using ChatRPG.Infrastructure.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChatRPG.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddDbContext<ChatRpgDbContext>();
        services.AddScoped<ICampaignRepository, CampaignRepository>()
            .AddScoped<IUnitOfWork, UnitOfWork>()
            .AddScoped<IUserDirectory, UserDirectory>();
        return services;
    }
}
