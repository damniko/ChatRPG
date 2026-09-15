using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Images;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.ReAct;
using ChatRPG.Agents.ReAct.Agents;
using ChatRPG.Agents.Scenarios;
using ChatRPG.Agents.Summarization;
using ChatRPG.Agents.Tools;
using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Helpers;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using ChatRPG.Application.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ChatRPG.Agents;

public static class DependencyInjection
{
    public static IServiceCollection AddAgents(this IServiceCollection services)
    {
        services.AddOptions<LanguageModelOptions>()
            .BindConfiguration(LanguageModelOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<AgentOptions>()
            .BindConfiguration(AgentOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<ScenarioStoreOptions>()
            .BindConfiguration(ScenarioStoreOptions.Section)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton<IChatModelFactory, OpenAiChatModelFactory>();
        services.AddSingleton<IInstructionCatalog, EmbeddedInstructionCatalog>();
        services.AddSingleton<IToolDescriptionCatalog, EmbeddedToolDescriptionCatalog>();
        services.AddSingleton<IToolDataTextParser, ToolDataTextParser>();
        services.AddSingleton<IToolDataValidatorFactory, ToolDataValidatorFactory>();
        services.AddSingleton<EdgeConsistencyValidator>();

        services.AddScoped<CharacterFinder>();
        services.AddScoped<IToolFactory, ToolFactory>();

        services.AddScoped<INarrator, ReActNarratorAgent>();
        services.AddScoped<IActionExaminer, ReActExaminerAgent>();
        services.AddScoped<IGraphNavigator, ReActNavigatorAgent>();
        services.AddScoped<IArchivist, ReActArchivistAgent>();
        services.AddScoped<INarrativeGraphScribe, ReActScribeAgent>();
        services.AddScoped<IPortraitGenerator, DallEPortraitGenerator>();
        services.AddScoped<IStartingScenarioGenerator, RagStartingScenarioGenerator>();
        services.AddScoped<IScenarioDocumentStore, PgVectorScenarioDocumentStore>();

        services.AddScoped<LlmSummarizer>();
        services.AddScoped<TranscriptSummarizer>();
        services.AddScoped<ISummarizer>(sp =>
            sp.GetRequiredService<IOptions<AgentOptions>>().Value.SummarizeArchivistMessages
                ? sp.GetRequiredService<LlmSummarizer>()
                : sp.GetRequiredService<TranscriptSummarizer>());


        return services;
    }
}
