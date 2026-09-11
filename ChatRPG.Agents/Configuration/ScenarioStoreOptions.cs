namespace ChatRPG.Agents.Configuration;

public sealed class ScenarioStoreOptions
{
    public const string Section = "ScenarioStore";

    public string ConnectionString { get; init; } = "";
    
    public int Dimensions { get; init; } = 1536;

    // To pick the chunk size, estimate how much information would be required to capture most passages you'd like to ask
    // questions about. Too many characters makes it difficult to capture semantic meaning, and too few characters means you
    // are more likely to split up important points that are related. In general, 200-500 characters is good for stories
    // without complex sequences of actions.
    public int ChunkSize { get; init; } = 500;

    // To pick the chunk overlap you need to estimate the size of the smallest piece of information. It may happen that one
    // chunk ends with `Ron's hair` and the other one starts with `is red`.In this case, an embedding would miss important
    // context, and not be generated properly. With overlap the end of the first chunk will appear in the beginning of the
    // other, eliminating the problem.
    public int ChunkOverlap { get; init; } = 200;
}
