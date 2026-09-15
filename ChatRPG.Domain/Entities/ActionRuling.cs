namespace ChatRPG.Domain.Entities;

/// <summary>
/// How the examiner ruled on an action the player proposed, before it was narrated.
/// </summary>
public enum ActionPermission
{
    /// <summary>The action succeeds.</summary>
    Allowed,

    /// <summary>The player may try, but something in the world is likely to block or complicate it.</summary>
    Conditional,

    /// <summary>The action is refused and the player is redirected.</summary>
    Disallowed
}

/// <summary>
/// The examiner's ruling on a proposed player action, grounded in the narrative graph, the game
/// summary and the scenario document. Only scenario campaigns are examined; open-world turns have
/// no ruling.
/// </summary>
public sealed record ActionRuling
{
    /// <param name="permission">Whether the action succeeds, is complicated, or is refused.</param>
    /// <param name="reasoning">
    /// The in-world justification for the ruling. Always present, in every permission: it supports the
    /// plausibility of a success, names the obstacle behind a complication, and drives the redirect of a
    /// refusal. Never contains the permission token itself.
    /// </param>
    public ActionRuling(ActionPermission permission, string reasoning)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reasoning);

        Permission = permission;
        Reasoning = reasoning;
    }

    /// <summary>The ruling used for the opening narration, which no examiner produces.</summary>
    public static readonly ActionRuling ScenarioOpening = new(
        ActionPermission.Allowed,
        "The scenario is created directly from the scenario document.");

    public ActionPermission Permission { get; }
    public string Reasoning { get; }

    /// <summary>False only when the action is refused outright.</summary>
    public bool AttemptProceeds => Permission is not ActionPermission.Disallowed;
}
