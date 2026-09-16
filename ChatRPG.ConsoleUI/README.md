# ChatRPG.ConsoleUI

A console front end for ChatRPG, built on [PrettyPrompt](https://github.com/waf/PrettyPrompt) for
the input bar and [Spectre.Console](https://spectreconsole.net/) for everything else. It is the
composition root for `ChatRPG.Application` + `ChatRPG.Agents` + `ChatRPG.Infrastructure`.

```bash
dotnet run -- --demo   # rendering only: no database, no language model
dotnet run             # the real game
```

## The console ownership rule

PrettyPrompt owns the console only inside `ReadLineAsync`. Every Spectre write — panels, rules,
spinners, menus, prompts — happens strictly between reads, and nothing writes from a background
thread while the input bar is open. Both libraries render in-line, so they compose as long as that
rule holds; breaking it corrupts the screen.

`Session/TurnRenderer.cs` is where this matters most. Spectre's status spinner forbids writes while
its block is open, so the turn's event stream is pumped by hand rather than with `await foreach`:
waits that produce no output happen under a spinner, narration chunks are written outside it.

## What it does not do

There is no pinned sidebar. Spectre has no split-screen layout that survives scrolling, so the
party is drawn as a panel after each turn and on demand via `/party`. In exchange the transcript
lives in normal terminal scrollback and stays selectable and copyable.

Dice rolls and combat detail are not shown inline yet. `BattleTool`, `WoundCharacterTool` and
`HealCharacterTool` resolve outcomes inside the narrator's ReAct loop and format them as prose for
the model only; none of it reaches `TurnEvent`. Until those are plumbed through, health changes are
recovered by diffing the campaign snapshot around each turn.

## Layout

| Path | What lives there |
|---|---|
| `Program.cs` | Host builder, DI, `--demo` switch, crash log |
| `Session/IGameBackend.cs` | Everything the UI needs from the game, with DI scoping hidden behind it |
| `Session/LiveGameBackend.cs` | One DI scope per call; projects EF entities into `GameSnapshot` |
| `Session/GameLoop.cs` | The REPL |
| `Session/TurnRenderer.cs` | `TurnEvent` stream → console |
| `Rendering/` | Theme, party panel, event formatting, the streaming word-wrap writer |
| `Input/` | PrettyPrompt callbacks (completion, highlighting, F-keys) and slash commands |
| `Menus/MainMenu.cs` | Start, continue, delete, settings, quit |
| `Configuration/` | Player settings, their JSON store and the `/config` editor |
| `Demo/DemoGameBackend.cs` | Canned turn used by `--demo` |

## Configuration

`appsettings.json` holds the sections the DI extensions bind and validate on start: `Application`,
`LanguageModel`, `Agents`, `ScenarioStore`, `Infrastructure`, plus
`ConnectionStrings:DefaultConnection`. Keep the OpenAI key in user-secrets:

```bash
dotnet user-secrets set "LanguageModel:ApiKey" "sk-..."
```

Running for real needs Postgres with the `vector` extension, migrated with
`ChatRPG.Infrastructure`'s `Initial` migration.

Player preferences (name, accent colour, wrap width, autocomplete, default action kind) are
separate: they live in `settings.json` under the user profile and are edited with `/config`. A
crash writes `crash.log` next to it.

## Keys

`/help` lists the commands. F2 opens the menu, F3 settings, F4 the party. Shift+Enter inserts a
newline without submitting. Ctrl-C cancels an in-flight turn without killing the process; pressing
it while editing clears the line.
