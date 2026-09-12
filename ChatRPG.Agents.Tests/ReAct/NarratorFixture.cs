using ChatRPG.Agents.Configuration;
using ChatRPG.Agents.Llm;
using ChatRPG.Agents.Prompts;
using ChatRPG.Agents.Prompts.Catalogs;
using ChatRPG.Agents.ReAct;
using ChatRPG.Agents.Tools;
using ChatRPG.Agents.Tools.Catalogs;
using ChatRPG.Agents.Tools.Helpers;
using ChatRPG.Agents.Tools.Implementations;
using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;
using ChatRPG.Application.Abstractions;
using ChatRPG.Domain.Combat;
using ChatRPG.Domain.Entities;
using ChatRPG.Domain.Enums;
using LangChain.Providers;
using Moq;
using Environment = ChatRPG.Domain.Entities.Environment;

namespace ChatRPG.Agents.Tests.ReAct;

/// <summary>
/// Builds a narrator whose only real collaborator is the ReAct loop. The tools have to be genuine
/// instances rather than mocks - the loop reads their names and descriptions to render the prompt -
/// but every dependency underneath them is stubbed, so nothing reaches a network.
/// </summary>
internal sealed class NarratorFixture
{
    public const string OpenWorldPrompt =
        "OPEN WORLD\nSummary: {gameSummary}\nAction: {action}\n" +
        "Tools: {tools} [{tool_names}]\nInput: {input}\nHistory: {history}";

    public const string ScenarioPrompt =
        "SCENARIO\nSummary: {gameSummary}\nAction: {action}\nGraph: {graph}\n" +
        "Tools: {tools} [{tool_names}]\nInput: {input}\nHistory: {history}";

    public Mock<IChatModel> Model { get; }

    public Mock<IChatModelFactory> Models { get; } = new();

    public Mock<IInstructionCatalog> Prompts { get; } = new();

    public Mock<IToolFactory> Tools { get; } = new();

    public AgentOptions Options { get; }

    private NarratorFixture(Mock<IChatModel> model, AgentOptions options)
    {
        Model = model;
        Options = options;

        Models.Setup(m => m.CreateChat(It.IsAny<double>(), It.IsAny<string?>(), It.IsAny<bool>()))
            .Returns(model.Object);

        Prompts.Setup(p => p.Get(It.IsAny<InstructionKey>()))
            .Returns((InstructionKey key) => key switch
            {
                InstructionKey.Narrate => OpenWorldPrompt,
                InstructionKey.NarrateWithGraph => ScenarioPrompt,
                // Instructions are bound as plain text, so a marker is enough to see which was chosen.
                _ => $"<instruction:{key}>",
            });

        Tools.Setup(t => t.GetWoundCharacterTool(It.IsAny<Campaign>()))
            .Returns((Campaign c) => new WoundCharacterTool(
                c, CharacterFinder(), Prompts.Object, Parser(), Combat(), Descriptions()));

        Tools.Setup(t => t.GetHealCharacterTool(It.IsAny<Campaign>()))
            .Returns((Campaign c) => new HealCharacterTool(
                c, Descriptions(), Prompts.Object, Parser(), CharacterFinder(), Combat()));

        Tools.Setup(t => t.GetBattleTool(It.IsAny<Campaign>()))
            .Returns((Campaign c) => new BattleTool(
                c, Prompts.Object, Descriptions(), Parser(), new BattleValidator(new CharacterValidator()),
                CharacterFinder(), new SystemRandomSource(), Combat()));

        Tools.Setup(t => t.GetSearchScenarioTool(It.IsAny<Campaign>()))
            .Returns((Campaign c) => new SearchScenarioTool(
                c, "<summary>", Mock.Of<IScenarioDocumentStore>(), Models.Object, Prompts.Object,
                Descriptions(), Parser(), new SearchScenarioValidator()));
    }

    /// <summary>A narrator whose model streams the given outputs token by token.</summary>
    public static NarratorFixture Streaming(AgentOptions? options = null, params string[] outputs)
    {
        return new NarratorFixture(ChatModelStub.Streaming(outputs), options ?? new AgentOptions());
    }

    /// <summary>A narrator whose model answers in one block.</summary>
    public static NarratorFixture Answering(AgentOptions? options = null, params string[] outputs)
    {
        return new NarratorFixture(ChatModelStub.Answering(outputs), options ?? new AgentOptions());
    }

    public ReActNarratorAgent Narrator()
    {
        return new ReActNarratorAgent(Models.Object, Prompts.Object, Tools.Object, Microsoft.Extensions.Options.Options.Create(Options));
    }

    /// <summary>The prompt the model was asked to complete on its first step.</summary>
    public string FirstPrompt() => ChatModelStub.PromptsSentTo(Model)[0];

    public static Campaign Campaign(bool isOpenWorld, NarrativeGraph? graph = null)
    {
        var user = new User { IdentityId = Guid.NewGuid(), Username = "tester" };
        var campaign = new Campaign(user, "A Test Campaign", "It begins in a tavern.", isOpenWorld)
        {
            NarrativeGraph = graph,
        };

        var environment = new Environment(campaign, "The Tavern", "Warm, loud and smelling of ale.");
        campaign.Environments.Add(environment);
        campaign.Characters.Add(
            new Character(campaign, environment, CharacterType.Humanoid, "Aldric", "A weary knight.", true));

        return campaign;
    }

    public static NarrativeGraph Graph()
    {
        var graph = new NarrativeGraph();
        graph.InitializeStartNode();
        return graph;
    }

    private static IToolDescriptionCatalog Descriptions()
    {
        var descriptions = new Mock<IToolDescriptionCatalog>();
        descriptions.Setup(d => d.Get(It.IsAny<ToolDescriptionKey>()))
            .Returns((ToolDescriptionKey key) => $"<describes {key}>");

        return descriptions.Object;
    }

    private static IToolDataTextParser Parser() => Mock.Of<IToolDataTextParser>();

    private static CombatResolver Combat() => new(new SystemRandomSource());

    private CharacterFinder CharacterFinder() =>
        new(Models.Object, Prompts.Object, Microsoft.Extensions.Options.Options.Create(Options), Parser());

    private sealed class SystemRandomSource : IRandomSource
    {
        public double NextDouble() => 0.5;

        public int Next(int min, int max) => min;
    }
}
