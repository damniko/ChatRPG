using System.Text;

namespace ChatRPG.ConsoleUI.Rendering;

/// <summary>
/// Word-wraps streamed narration as it arrives. Chunks are raw model text, so nothing here goes
/// through Spectre markup parsing — it is written to the console verbatim, one word at a time.
/// </summary>
internal sealed class NarrationWriter(int width, int indent = 2)
{
    private readonly StringBuilder _word = new();
    private readonly TextWriter _out = Console.Out;
    private int _column;
    private bool _lineStarted;
    private int _blankLinesPending;

    public bool HasWritten { get; private set; }

    private int Limit => Math.Max(20, width - indent);

    public void Write(string chunk)
    {
        foreach (char c in chunk)
        {
            if (c == '\r')
            {
                continue;
            }

            if (c == '\n')
            {
                FlushWord();
                EndLine();
                _blankLinesPending++;
                continue;
            }

            if (char.IsWhiteSpace(c))
            {
                FlushWord();
                continue;
            }

            _word.Append(c);
        }
    }

    public void Flush()
    {
        FlushWord();
        EndLine();
        _blankLinesPending = 0;
    }

    private void FlushWord()
    {
        if (_word.Length == 0)
        {
            return;
        }

        string word = _word.ToString();
        _word.Clear();

        // Blank lines the model emitted between paragraphs are only honoured once we know more
        // text follows, so a trailing newline never leaves a dangling gap above the prompt.
        while (_blankLinesPending > 1)
        {
            _out.WriteLine();
            _blankLinesPending--;
        }
        _blankLinesPending = 0;

        int separator = _lineStarted ? 1 : 0;
        if (_lineStarted && _column + separator + word.Length > Limit)
        {
            EndLine();
            separator = 0;
        }

        if (!_lineStarted)
        {
            _out.Write(new string(' ', indent));
            _lineStarted = true;
            _column = 0;
        }
        else if (separator == 1)
        {
            _out.Write(' ');
            _column++;
        }

        // A single word wider than the wrap limit is broken rather than allowed to overflow.
        while (word.Length > Limit)
        {
            int take = Limit - _column;
            if (take <= 0)
            {
                EndLine();
                _out.Write(new string(' ', indent));
                _lineStarted = true;
                _column = 0;
                take = Limit;
            }

            _out.Write(word[..take]);
            word = word[take..];
            EndLine();
            _out.Write(new string(' ', indent));
            _lineStarted = true;
            _column = 0;
        }

        _out.Write(word);
        _column += word.Length;
        HasWritten = true;
    }

    private void EndLine()
    {
        if (!_lineStarted)
        {
            return;
        }

        _out.WriteLine();
        _lineStarted = false;
        _column = 0;
    }
}
