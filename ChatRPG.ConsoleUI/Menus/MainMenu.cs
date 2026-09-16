using ChatRPG.ConsoleUI.Rendering;
using ChatRPG.ConsoleUI.Session;
using Spectre.Console;

namespace ChatRPG.ConsoleUI.Menus;

public abstract record MenuResult
{
    public sealed record Play(int CampaignId, string? OpeningPrompt) : MenuResult;
    public sealed record EditSettings : MenuResult;
    public sealed record Resume : MenuResult;
    public sealed record Quit : MenuResult;
}

public sealed class MainMenu(IGameBackend backend)
{
    public async Task<MenuResult> ShowAsync(bool hasActiveCampaign, CancellationToken ct)
    {
        var choices = new List<string>();
        if (hasActiveCampaign)
        {
            choices.Add("resume");
        }
        choices.AddRange(["continue a campaign", "new campaign", "delete a campaign", "settings", "quit"]);

        string choice = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title($"[{Theme.Accent.Foreground.ToMarkup()}]ChatRPG[/]")
            .HighlightStyle(Theme.Accent)
            .AddChoices(choices));

        switch (choice)
        {
            case "resume":
                return new MenuResult.Resume();

            case "settings":
                return new MenuResult.EditSettings();

            case "quit":
                return new MenuResult.Quit();

            case "continue a campaign":
            {
                CampaignSummary? picked = await PickCampaignAsync("continue which campaign?", ct);
                return picked is null ? new MenuResult.Resume() : new MenuResult.Play(picked.Id, null);
            }

            case "delete a campaign":
            {
                CampaignSummary? picked = await PickCampaignAsync("delete which campaign?", ct);
                if (picked is not null && AnsiConsole.Confirm($"delete [red]{Markup.Escape(picked.Title)}[/]?", false))
                {
                    await backend.DeleteCampaignAsync(picked.Id, ct);
                    EventRenderer.Info($"deleted {picked.Title}");
                }
                return new MenuResult.Resume();
            }

            default:
                return await NewCampaignAsync(ct);
        }
    }

    private async Task<CampaignSummary?> PickCampaignAsync(string title, CancellationToken ct)
    {
        IReadOnlyList<CampaignSummary> campaigns = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Theme.Accent)
            .StartAsync("[grey54]loading campaigns…[/]", _ => backend.ListCampaignsAsync(ct));

        if (campaigns.Count == 0)
        {
            EventRenderer.Info("no campaigns yet");
            return null;
        }

        var prompt = new SelectionPrompt<CampaignSummary?>()
            .Title($"[grey54]{title}[/]")
            .HighlightStyle(Theme.Accent)
            .UseConverter(c => c is null
                ? "← back"
                : $"{c.Title} — {c.PlayerName}{(c.GameOver ? " (ended)" : "")}  [grey54]{c.StartedOn:yyyy-MM-dd}[/]");

        foreach (CampaignSummary campaign in campaigns)
        {
            prompt.AddChoice(campaign);
        }
        prompt.AddChoice(null);

        return AnsiConsole.Prompt(prompt);
    }

    private async Task<MenuResult> NewCampaignAsync(CancellationToken ct)
    {
        string title = AnsiConsole.Ask<string>("campaign [grey54]title[/]");
        string characterName = AnsiConsole.Ask<string>("your [grey54]character's name[/]");
        string characterDescription = AnsiConsole.Ask("a short [grey54]description[/]", "an unremarkable adventurer");

        // CreateCampaignHandler treats isOpenWorld as the scenario-document path: it ingests the
        // document, scribes the narrative graph and generates the starting scenario from it.
        bool fromDocument = AnsiConsole.Confirm("build the campaign from a scenario document?", false);

        byte[]? document = null;
        string startScenario = string.Empty;

        if (fromDocument)
        {
            string path = AnsiConsole.Prompt(new TextPrompt<string>("path to the scenario [grey54]PDF[/]")
                .Validate(p => File.Exists(p)
                    ? ValidationResult.Success()
                    : ValidationResult.Error("no file there")));
            document = await File.ReadAllBytesAsync(path, ct);
        }
        else
        {
            startScenario = AnsiConsole.Ask<string>("how does it [grey54]begin[/]?");
        }

        int campaignId = await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Theme.Accent)
            .StartAsync("[grey54]building the world…[/]", _ => backend.CreateCampaignAsync(
                new NewCampaignRequest(title, startScenario, fromDocument, characterName, characterDescription, document),
                ct));

        GameSnapshot snapshot = await backend.GetSnapshotAsync(campaignId, ct);
        string opening = fromDocument
            ? $"{snapshot.PlayerName} arrives where the story begins."
            : startScenario;

        return new MenuResult.Play(campaignId, opening);
    }
}
