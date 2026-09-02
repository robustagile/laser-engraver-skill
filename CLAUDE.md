# Laser Engraver Skill — orientation

Start here. This file says what this repository is for, where things live, and which decisions
are already settled, so none of that has to be restated at the start of a session.

## What this is

A Claude Code plugin, distributed as this repository rather than as a packaged install, that
takes the owner of a fiber/MOPA or CO2 laser from *"I have a machine"* to *"I have a LightBurn
file I can open and run"*. It does that in three stages plus one cross-cutting one:

1. **Machine onboarding** — record the equipment, help configure and calibrate LightBurn for it.
2. **Building the recipe base** — research candidate recipes for that machine, then calibrate
   them on it with generated test cards until they meet their acceptance criteria.
3. **Production** — discuss the job, then generate the `.lbrn` file for it.
4. **Troubleshooting by symptom** — "it came out grey", "it burned through". Not a bonus: for
   the target audience this is the most common way in, and it does not fit inside 1–3.

The audience is an owner with **basic experience** — can operate the machine and the software,
but is stuck on practical application. Not experts. Complete novices and diode lasers are a
later audience, not v1.

## Where things are

| Path | State | What it holds |
|---|---|---|
| `claude/requirements.md` | **exists** | What the plugin must do. Numbered requirements with stable IDs, plus the open questions for design. |
| `claude/design-composition.md` | **exists** | First design pass: delivery, installed layout, the two skills and four commands, how they communicate. |
| `claude/design-data-formats.md` | **exists** | Second design pass: the shape of every file — machine record, recipe, catalogue, config, regulatory cache, and the JSON spec handed to the generator. |
| `plugin/` | *not yet* | The plugin itself — `skills/laser-machines/`, `skills/laser-lightburn/`, `commands/`, and the generator. |
| `plugin/tests/` | *not yet* | The committed test suite, including golden files. |
| `README.md`, `INSTALL.md`, `STATUS.md` | *not yet* | Human-facing description, installation, and current state. |

Nothing else exists yet. Requirements are gathered and both design passes are done. What remains
open needs LightBurn rather than a decision: the two unverified format elements (R-G15), how far
geometry generation goes in v1 (OQ-7), and which `.lbrn` format version to emit (OQ-8).

**This repository holds no private data** — no machine records, no recipes, no burn readings.
Real work happens in a separate installation of the plugin; the loop back here is that a problem
found in real work gets reported, fixed here, and re-installed. (R-D4)

## Working agreements

- **Dialogue with the owner is in Russian. Every document, comment and identifier in the
  repository is in English.** (R-D1)
- **Requirement IDs are stable.** Refer to `R-…` and `OQ-…` rather than restating their text,
  and mark a resolved open question as resolved rather than deleting it.
- Design decisions belong in `claude/`, not in commit messages or in this file.

## Decisions already made — do not re-open without a reason

These were settled in conversation and are recorded with their rationale in
`claude/requirements.md`. Re-litigating them wastes a session.

- **The unit is part of every field name** — `speed_mm_s`, `depth_um`, never a bare `depth`. The
  prior work recorded a depth target of "0.01" and could never establish whether it meant
  millimetres or inches. (`design-data-formats.md` §2)
- **Two skills, four commands.** `laser-machines` owns equipment, recipes and calibration;
  `laser-lightburn` owns the `.lbrn` format and the generator, and is a service to the first.
  One command per workflow. (`design-composition.md` §3)
- **The installer never writes in the user's data directory**, and has no prerequisites of its
  own — the .NET check lives in the skill, because a .NET program cannot check for .NET.
  (R-N7, R-N8)
- **No recipes ship with the plugin.** What ships is a catalogue of *which* recipes are needed;
  recipes are researched per machine, seeded with provenance, then calibrated. (R-R12–R-R17)
- **Provenance is load-bearing, not decoration.** At the moment a machine is registered there is
  no verified data at all, so every recipe starts web-sourced or computed and must stay visibly
  distinguishable from a real result. (R-R7)
- **A recipe is valid for a (machine, lens) pair.** Lenses are first class, not an attribute.
  (R-M2)
- **Toolchain is the .NET SDK** — chosen over Python and Node because on both Windows and Linux
  it is the smaller install barrier when nothing is installed yet. (R-E3)
- **LightBurn v2 is the only target software.** EZCAD is not in the loop. (R-E1)
- **The plugin is fully self-contained.** Inherited material is copied in once; nothing is read
  at run time from a path outside the repository. (R-N4, and R-N5 for the user-data exception)
- **Safety warnings are proactive**, and enforced in the plugin's instructions rather than
  relegated to a README note. (R-S1, R-S5)

## Verification discipline

Inherited from prior experimental work, where each of these cost real time. The `.lbrn` format is
undocumented; everything known about it was established by probe files and by reading files
LightBurn itself saved.

- **LightBurn silently normalises what it does not recognise.** An invented layer type was
  loaded, rewritten as a plain one, and its contour dropped — no error, no warning.
- **"It opens" is not verification. Neither is a round-trip diff** — LightBurn v2 rewrites the
  file on save whether or not it understood a given element. Verification is visual or measured
  in the UI.
- **An inferred constant must not be asserted by a test built on the same inference.** Two bugs
  survived precisely because their tests encoded the same wrong assumption.
- **Asking the owner to build a case in LightBurn and save it is the highest-value way to settle
  a format question.** One such file previously settled three at once.

Full detail, including which format facts are verified and which are still assumed, is in
`claude/requirements.md` §10.

## Repository conventions

- **Trunk-based:** commit to `main`, no feature branches. Commit and push only when asked.
- **`git config core.fileMode false` on every clone.** On a `/mnt/*` DrvFs mount every file
  appears executable, and without this git reports the whole tree as mode-changed. Git cannot
  carry this in the tree. (R-N6)
- **Line endings:** the working tree is Windows format (CRLF), text is stored with LF, so an
  EOL-only difference is never a content change. `git status` may still flag such a file because
  it compares against the size cached in the index — **`git diff` is the authoritative check.**
- The repository is edited from Windows and from WSL in **one shared working tree**. Generated
  `.lbrn` files must land where LightBurn on the Windows side can open them.
