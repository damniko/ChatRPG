using ChatRPG.Agents.Tools.Parsing;
using ChatRPG.Agents.Tools.Validators;

namespace ChatRPG.Agents.Tests.Tools.Validators;

public class ToolDataValidatorFactoryTests
{
    [Fact]
    public void Create_ResolvesAValidatorForEveryToolDataTheToolFactoryAsksFor()
    {
        var factory = new ToolDataValidatorFactory();

        Assert.IsType<BattleValidator>(factory.Create<ToolData.Battle>());
        Assert.IsType<CharacterValidator>(factory.Create<ToolData.Character>());
        Assert.IsType<EnvironmentValidator>(factory.Create<ToolData.Environment>());
        Assert.IsType<SearchScenarioValidator>(factory.Create<ToolData.SearchScenario>());
        Assert.IsType<UpdateGraphValidator>(factory.Create<ToolData.UpdateGraph>());
        Assert.IsType<AddNodeValidator>(factory.Create<ToolData.AddNode>());
        Assert.IsType<AddEdgeValidator>(factory.Create<ToolData.AddEdge>());
        Assert.IsType<AddEndNodeValidator>(factory.Create<ToolData.AddEndNode>());
    }

    [Fact]
    public void Create_UnregisteredToolData_NamesTheTypeItCouldNotResolve()
    {
        var factory = new ToolDataValidatorFactory();

        var exception = Assert.Throws<NotSupportedException>(factory.Create<ToolData.EdgeConditions>);

        Assert.Contains(nameof(ToolData.EdgeConditions), exception.Message);
    }
}
