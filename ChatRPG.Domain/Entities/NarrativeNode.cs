using ChatRPG.Domain.Entities.Abstractions;

namespace ChatRPG.Domain.Entities;

public class NarrativeNode : IEntity
{
    private NarrativeNode() { }

    public NarrativeNode(string name, string content, NarrativeGraph graph)
    {
        Name = name;
        StoryContent = content;
        Graph = graph;
    }

    public int Id { get; init; }

    public NarrativeGraph Graph { get; private set; } = null!;

    public string Name { get; private set; } = null!;
    public string StoryContent { get; set; } = null!;

    public ICollection<NarrativeEdge> Edges { get; private set; } = [];

    public Status NodeStatus { get; set; } = Status.Undiscovered;

    public enum Status
    {
        Undiscovered,
        Ongoing,
        Completed
    }

    public override string ToString() => $"{NodeStatus} Node({Id}) named [{Name}]: {StoryContent}";
}
