# Installing

> **The installer works; the plugin does not yet.** Deploying is real and safe to do now, but
> what lands in your `.claude` is still a frame — the skills have their structure and none of
> their content, so neither can actually do anything. Install it to check the plumbing, not to
> engrave. See [STATUS.md](STATUS.md).

## Prerequisites

### LightBurn v2

A purchased copy of LightBurn v2. The plugin generates files for LightBurn and nothing else —
EZCAD is not in the loop.

Note the exact version, including the patch number (Help -> About). The `.lbrn` format is
undocumented and changes between versions, so the plugin records which version you have.

### .NET SDK

The `.lbrn` generator is a .NET program. Install the **SDK**, not the Runtime — the Runtime can
only start an already-built program, and this one is built from source on your machine.

Download it from <https://dotnet.microsoft.com/en-us/download/dotnet/10.0>. That page has the
installers for Windows and macOS and the per-distribution instructions for Linux. Take the
column marked **SDK**.

**Verify:**

```
dotnet --list-sdks
```

At least one SDK must be listed. If the command is not found after installing, open a new
terminal — the installer changes `PATH`.

You do not need to install anything before running the installer itself: it only copies files.
The SDK check happens the first time the plugin actually needs to generate something, and it
will tell you what to do then.

### git

To clone this repository, and to update it later.

## Getting the repository

```
git clone git@github.com:robustagile/laser-engraver-skill.git
cd laser-engraver-skill
```

The clone is needed to install and to update. Nothing reads from it while the plugin runs, so it
can live anywhere — but do not delete it if you want updates.

## Deploying

```
./install.sh  <target>        # Linux, macOS, WSL
.\install.ps1 <target>        # Windows
```

where `<target>` is either your user-level `.claude` directory, or a project's:

| Target | Effect |
|---|---|
| `~/.claude` | Available in every directory. Your machines and recipes are shared across all your work. |
| `<project>/.claude` | Everything stays inside one working folder. Choose this if you keep your user level deliberately spare. |

Your data lives in **`laser-skill-data/` beside the target**, not inside it — `~/laser-skill-data/`
for a user-level install, `<project>/laser-skill-data/` for a project-level one. One directory
holds all of it: machines, recipes, burn results, your settings.

It is outside `.claude/` for a reason that was measured rather than guessed: Claude Code guards
writes anywhere under a `.claude` directory, and no permission rule in your settings lifts that
guard, so a store in there would need an interactive approval every single session.

The installer copies the skills and commands into the target and **never writes inside your data
directory** — your machine records, recipes and burn results are yours and are left alone, so
re-running it to update is safe.

If you install into a project, that store lands in the project's root, which is committed to
git. The installer prints the `.gitignore` line that keeps your own records out of that history
— and says nothing when the line is already there.

It reports what it changed, per skill and per command, and records what it installed in a
`VERSION` file beside each `SKILL.md`, so a re-run can say whether anything moved.

### Developing the plugin itself

```
./install.sh  <target> --link
.\install.ps1 <target> -Link
```

Symlinks the skills to your clone instead of copying them, so an edit takes effect with no
re-install. On Windows this needs developer mode or an elevated prompt. Do not use it for
ordinary use: `git pull` then changes the installed plugin underneath you.

## Updating

```
git pull
./install.sh <the same target>
```

## Uninstalling

Delete the deployed skills and commands from your `.claude`. `laser-skill-data/` beside it is
separate and survives; delete it too if you want the recipes gone.
