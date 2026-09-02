# Laser Engraver Skill

A Claude Code plugin for owners of fiber, MOPA and CO2 laser engravers, working through
LightBurn.

> **Under development. There is nothing to install yet.** The repository currently holds
> requirements and design; see [STATUS.md](STATUS.md) for what exists on any given day.

## What it will do

Take you from *"I have a machine"* to *"I have a LightBurn file I can open and run"*, in three
stages plus one that cuts across them:

1. **Set up the machine** — record what you have, research what you do not know about it, and
   help you configure and calibrate LightBurn for it.
2. **Build a recipe base** — find candidate settings for your machine, then calibrate them on
   it with generated test cards until they meet criteria agreed before the test.
3. **Do the job** — discuss what you want to make, then generate the `.lbrn` file for it.
4. **Work out what went wrong** — "it came out grey", "it burned through". Usually the first
   reason anyone opens a tool like this.

## Who it is for

An owner with **basic experience**: you can already run your machine and its software, but you
are stuck on practical application — which settings give which result on which material.
Complete beginners and diode lasers are a later audience.

## What it will not do

- **Ship you a recipe base.** No settings table ships with this plugin. What ships is a
  catalogue of *which* recipes are worth having; the recipes themselves are researched for your
  machine and then calibrated on it, because a setting that was not verified on your machine is
  a guess wearing a number.
- **Replace a test burn.** Every recipe is a starting point until you have burned a sample.
- **Talk to your machine.** LightBurn does that. This plugin writes the file LightBurn opens.

## Requirements

- **LightBurn v2**, purchased.
- **.NET SDK** — used to generate the `.lbrn` files.

See [INSTALL.md](INSTALL.md).

## Where things are

| Path | What it holds |
|---|---|
| `claude/requirements.md` | What the plugin must do, as numbered requirements. |
| `claude/design-*.md` | The design decisions and why they were taken. |
| `plugin/` | The plugin itself. Not written yet. |
| `STATUS.md` | What exists today, what is open, where to pick up. |

## Licence

Apache License 2.0 — see [LICENSE](LICENSE).
