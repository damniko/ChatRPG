using System.Text.Json;
using System.Text.Json.Serialization;
using ChatRPG.Domain.Entities.Abstractions;

namespace ChatRPG.Domain.Entities;

public class NarrativeNode : IEntity
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };

    private NarrativeNode() { }

    public NarrativeNode(string name, string content, NarrativeGraph graph)
    {
        Name = name;
        StoryContent = content;
        Graph = graph;
    }

    [JsonIgnore]
    public int Id { get; init; }

    [JsonIgnore]
    public NarrativeGraph Graph { get; private set; } = null!;

    public string Name { get; private set; } = null!;
    public string StoryContent { get; set; } = null!;

    public ICollection<NarrativeEdge> Edges { get; private set; } = [];

    [JsonIgnore]
    public Status NodeStatus { get; set; } = Status.Undiscovered;

    public string NodeStatusCategory => NodeStatus.ToString();

    public enum Status
    {
        Undiscovered,
        Ongoing,
        Completed
    }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, _jsonSerializerOptions);
    }

    public override string ToString() => $"{NodeStatus} Node({Id}) named [{Name}]: {StoryContent}";
}
