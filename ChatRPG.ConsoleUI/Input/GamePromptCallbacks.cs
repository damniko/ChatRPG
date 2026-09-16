using ChatRPG.ConsoleUI.Configuration;
using ChatRPG.ConsoleUI.Session;
using PrettyPrompt;
using PrettyPrompt.Completion;
using PrettyPrompt.Consoles;
using PrettyPrompt.Documents;
using PrettyPrompt.Highlighting;

namespace ChatRPG.ConsoleUI.Input;

/// <summary>
/// Completion and highlighting for the input bar. The snapshot is swapped in after every turn so
/// completion follows the characters and locations that currently exist in the campaign.
/// </summary>
public sealed class GamePromptCallbacks(PlayerSettings settings) : PromptCallbacks
{
    private static readonly ConsoleFormat CommandFormat = new(AnsiColor.BrightMagenta);
    private static readonly ConsoleFormat SpeechFormat = new(AnsiColor.BrightCyan);
    private static readonly ConsoleFormat NameFormat = new(AnsiColor.BrightYellow);

    public GameSnapshot? Snapshot { get; set; }

    protected override IEnumerable<(KeyPressPattern, KeyPressCallbackAsync)> GetKeyPressCallbacks()
    {
        yield return (new KeyPressPattern(ConsoleKey.F2),
            (_, _, _) => Task.FromResult<KeyPressCallbackResult?>(new KeyPressCallbackResult(string.Empty, SlashCommand.Menu.Name))!);

        yield return (new KeyPressPattern(ConsoleKey.F3),
            (_, _, _) => Task.FromResult<KeyPressCallbackResult?>(new KeyPressCallbackResult(string.Empty, SlashCommand.Config.Name))!);

        yield return (new KeyPressPattern(ConsoleKey.F4),
            (_, _, _) => Task.FromResult<KeyPressCallbackResult?>(new KeyPressCallbackResult(string.Empty, SlashCommand.Party.Name))!);
    }

    protected override Task<bool> ShouldOpenCompletionWindowAsync(
        string text, int caret, KeyPress keyPress, CancellationToken cancellationToken)
    {
        if (!settings.EnableCompletion)
        {
            return Task.FromResult(false);
        }

        // Opening on '/' at the start keeps the menu out of the way while writing prose.
        bool isCommandStart = text.AsSpan(0, caret).TrimStart().Length == 1 && text.TrimStart().StartsWith('/');
        return isCommandStart
            ? Task.FromResult(true)
            : base.ShouldOpenCompletionWindowAsync(text, caret, keyPress, cancellationToken);
    }

    protected override Task<TextSpan> GetSpanToReplaceByCompletionAsync(
        string text, int caret, CancellationToken cancellationToken)
    {
        int start = caret;
        while (start > 0 && !char.IsWhiteSpace(text[start - 1]))
        {
            start--;
        }

        int end = caret;
        while (end < text.Length && !char.IsWhiteSpace(text[end]))
        {
            end++;
        }

        return Task.FromResult(TextSpan.FromBounds(start, end));
    }

    protected override Task<IReadOnlyList<CompletionItem>> GetCompletionItemsAsync(
        string text, int caret, TextSpan spanToBeReplaced, CancellationToken cancellationToken)
    {
        if (!settings.EnableCompletion)
        {
            return Task.FromResult<IReadOnlyList<CompletionItem>>([]);
        }

        string typed = text.Substring(spanToBeReplaced.Start, spanToBeReplaced.Length);

        IReadOnlyList<CompletionItem> items = typed.StartsWith('/')
            ? SlashCommand.All
                .Where(c => c.Name.StartsWith(typed, StringComparison.OrdinalIgnoreCase))
                .Select(c => new CompletionItem(
                    c.Name,
                    new FormattedString(c.Name, CommandFormat),
                    c.Name,
                    (_) => Task.FromResult(new FormattedString($"{c.Usage}\n{c.Summary}"))))
                .ToList()
            : Names()
                .Where(n => n.StartsWith(typed, StringComparison.OrdinalIgnoreCase))
                .Select(n => new CompletionItem(n, new FormattedString(n, NameFormat), n))
                .ToList();

        return Task.FromResult(items);
    }

    protected override Task<IReadOnlyCollection<FormatSpan>> HighlightCallbackAsync(
        string text, CancellationToken cancellationToken)
    {
        var spans = new List<FormatSpan>();

        string trimmed = text.TrimStart();
        if (trimmed.StartsWith('/'))
        {
            int offset = text.Length - trimmed.Length;
            int length = trimmed.IndexOf(' ') is var space && space > 0 ? space : trimmed.Length;
            spans.Add(new FormatSpan(offset, length, CommandFormat));
        }

        // Quoted text reads as speech, so it is coloured the way narration colours dialogue.
        int quote = text.IndexOf('"');
        while (quote >= 0)
        {
            int close = text.IndexOf('"', quote + 1);
            if (close < 0)
            {
                spans.Add(new FormatSpan(quote, text.Length - quote, SpeechFormat));
                break;
            }

            spans.Add(new FormatSpan(quote, close - quote + 1, SpeechFormat));
            quote = text.IndexOf('"', close + 1);
        }

        return Task.FromResult<IReadOnlyCollection<FormatSpan>>(spans);
    }

    private IEnumerable<string> Names()
    {
        if (Snapshot is null)
        {
            yield break;
        }

        foreach (string name in Snapshot.CharacterNames)
        {
            yield return name;
        }

        foreach (string location in Snapshot.LocationNames)
        {
            yield return location;
        }
    }
}
