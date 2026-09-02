# Installing

> **Not yet possible.** The plugin is still being written, and there is nothing to deploy. The
> prerequisites below are real and can be done now; the deployment step at the end describes
> what it *will* be. See [STATUS.md](STATUS.md).

## Prerequisites

### LightBurn v2

A purchased copy of LightBurn v2. The plugin generates files for LightBurn and nothing else —
EZCAD is not in the loop.

Note the exact version, including the patch number (Help -> About). The `.lbrn` format is
undocumented and changes between versions, so the plugin records which version you have.

### .NET SDK

The `.lbrn` generator is a .NET program. Install the SDK — not just the runtime.

**Windows**

```
winget install Microsoft.DotNet.SDK.10
```

**Linux** — follow the instructions for your distribution at
<https://learn.microsoft.com/dotnet/core/install/linux>, installing the **SDK** package.

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

## Deploying — *not available yet*

The intended shape, so you know what is coming:

```
./install.sh  <target>        # Linux, macOS, WSL
.\install.ps1 <target>        # Windows
```

where `<target>` is either your user-level `.claude` directory, or a project's:

| Target | Effect |
|---|---|
| `~/.claude` | Available in every directory. Your machines and recipes are shared across all your work. |
| `<project>/.claude` | Everything stays inside one working folder. Choose this if you keep your user level deliberately spare. |

The installer copies the skills and commands into the target, and **never writes inside your
data directory** — your machine records, recipes and burn results are yours and are left alone,
so re-running it to update is safe.

If you install into a project, its `.claude` directory is often committed to git. The installer
will print the `.gitignore` line that keeps your own records out of that history.

## Updating — *not available yet*

```
git pull
./install.sh <the same target>
```

## Uninstalling — *not available yet*

Delete the deployed skills and commands from your `.claude`. Your data directory is separate and
survives; delete it too if you want the recipes gone.
