using ChatRPG.Domain.Entities;
using ChatRPG.Domain.Enums;
using ChatRPG.Domain.Exceptions;

namespace ChatRPG.Application.Abstractions;

public interface INarrativeGraphRenderer
{
    /// <summary>
    /// Render <paramref name="graph"/> in the given <paramref name="format"/> and return its byte representation. 
    /// </summary>
    /// <param name="graph">The graph to render.</param>
    /// <param name="format">The format used to encode the render.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>Byte representation of the rendered graph.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="format"/> is invalid.</exception>
    /// <exception cref="MalformedGraphException">If the <paramref name="graph"/> cannot be rendered due to errors in the graph.</exception>
    Task<byte[]> RenderAsync(NarrativeGraph graph, VisualizationFormat format, CancellationToken ct = default);
}