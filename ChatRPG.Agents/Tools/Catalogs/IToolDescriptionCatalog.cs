namespace ChatRPG.Agents.Tools.Catalogs;

internal interface IToolDescriptionCatalog
{
    string Get(ToolDescriptionKey key);
}
