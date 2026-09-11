using ChatRPG.Agents.Tools.Parsing;

namespace ChatRPG.Agents.Tools.Validators;

using System;

internal class ToolDataValidatorFactory : IToolDataValidatorFactory
{
    private static readonly Lazy<Dictionary<Type, Func<object>>> ValidatorCreators = new(
        () => new Dictionary<Type, Func<object>> 
        {
            { typeof(ToolData.Character), () => new CharacterValidator() },
            { typeof(ToolData.Environment), () => new EnvironmentValidator() },
            { typeof(ToolData.UpdateGraph), () => new UpdateGraphValidator() },
            { typeof(ToolData.SearchScenario), () => new SearchScenarioValidator() },
            { typeof(ToolData.AddEdge), () => new AddEdgeValidator() },
            { typeof(ToolData.AddNode), () => new AddNodeValidator(new AddEdgeValidator()) },
            { typeof(ToolData.AddEndNode), () => new AddEndNodeValidator() },
            { typeof(ToolData.Battle), () => new BattleValidator(new CharacterValidator()) }
        });

    public IToolDataValidator<T> Create<T>() where T : ToolData
    {
        var key = typeof(T);
        if (!ValidatorCreators.Value.TryGetValue(key, out var factory))
        {
            throw new NotSupportedException($"Validator for {key} is not registered.");
        }
        return (IToolDataValidator<T>) factory.Invoke();
    }
}
