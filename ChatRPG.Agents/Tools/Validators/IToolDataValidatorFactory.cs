using ChatRPG.Agents.Tools.Parsing;

namespace ChatRPG.Agents.Tools.Validators;

internal interface IToolDataValidatorFactory
{
    IToolDataValidator<T> Create<T>() where T : ToolData;
}
