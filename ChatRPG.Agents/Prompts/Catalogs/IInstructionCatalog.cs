namespace ChatRPG.Agents.Prompts.Catalogs;

internal interface IInstructionCatalog
{
    string Get(InstructionKey key);
}
