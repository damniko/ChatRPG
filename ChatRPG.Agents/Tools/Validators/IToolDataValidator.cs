namespace ChatRPG.Agents.Tools.Validators;

internal interface IToolDataValidator<in T>
{
    bool IsValid(T toolData, out IList<string> errors);
}
