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
| `plugin/skills/laser-lightburn/` | **partly** | `tools/` — the `.lbrn` writer and the probe generator. `references/lbrn-format.md` — verified versus assumed. `probe/` — the probe register, including the file LightBurn itself saved. `SKILL.md` is a skeleton: sections with `TODO` bodies. |
| `plugin/skills/laser-machines/` | **partly** | `references/catalogue/` — 30 authored entries with acceptance criteria. `references/materials.md` — the material vocabulary. `SKILL.md` and its five reference files are skeletons: sections with `TODO` bodies. |
| `plugin/commands/` | **skeleton** | The four entry points. Frontmatter and one `TODO` paragraph each. |
| `plugin/tests/` | **exists** | The committed test suite. 37 tests over the writer and the transform maths. |
| `README.md` | **exists** | What the plugin will do, for a human. Deliberately minimal while it is unbuilt. |
| `INSTALL.md` | **exists** | Prerequisites, which are real now; deployment, which is marked as not yet available. |
| `STATUS.md` | **exists** | What exists today, what is open, where to pick up. Read it first in a new session. |

Requirements are gathered, both design passes are done, and the `.lbrn` writer is imported,
under test, and verified against LightBurn for every element it emits (R-G15 settled). What
remains is writing the skills themselves — both `SKILL.md` files and every reference file exist
as skeletons with `TODO` bodies, and there are no install scripts, so nothing is installable
yet. The catalogue is authored (OQ-10 resolved); no design question is open.

**This repository holds no private data** — no machine records, no recipes, no burn readings.
Real work happens in a separate installation of the plugin; the loop back here is that a problem
found in real work gets reported, fixed here, and re-installed. (R-D4)

## Keeping STATUS.md

`STATUS.md` is the first thing to read in a new session and the last thing to update before
leaving one. It has **exactly three sections** and nothing else:

1. **What exists today** — a declaration of the present.
2. **Open problems** — what is blocked, unverified or undecided, and why it matters.
3. **Where to start the next session** — the next few concrete moves, in order.

The rules that keep it useful:

- **It is not a log.** No account of what was done, no dated entries accumulating down the page,
  no "previously we…". That history is in `git log`, where it cannot go stale.
- **Rewrite, never append.** When something becomes true, edit the sentence that said otherwise.
  When a problem is solved, delete it — a resolved problem leaves no trace here.
- **Update it whenever a session changed what exists, what is blocked, or what comes next.** A
  session that only discussed something changed none of those and needs no update.
- **Keep the date at the top honest**, and say plainly what does *not* exist. An orientation
  file that lists the intended tree flatly reads as though it were all there.

## Working agreements

- **Dialogue with the owner is in Russian. Every document, comment and identifier in the
  repository is in English.** (R-D1)
- **Requirement IDs are stable.** Refer to `R-…` and `OQ-…` rather than restating their text,
  and mark a resolved open question as resolved rather than deleting it.
- Design decisions belong in `claude/`, not in commit messages or in this file.

## Decisions already made — do not re-open without a reason

These were settled in conversation and are recorded with their rationale in
`claude/requirements.md`. Re-litigating them wastes a session.

- **The unit is part of every field name, and every linear dimension is metric** —
  `speed_mm_s`, `depth_um`, never a bare `depth` and never inches. The prior work recorded a
  depth target of "0.01" and could never establish whether it meant millimetres or inches. A
  regulation written in inches is converted, and the imperial original is kept in prose as the
  source, not as a unit a field might be in. (R-E6, `design-data-formats.md` §2)
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
- **v1 generates geometry, not just settings** — primitives, text at an exact character height,
  arrays, imported vector art, and raster images including PNG/TIFF depth maps. Variable data
  (`VariableText`) stays out. The writer has none of the raster side yet, and an `Image` layer
  has never been read back in LightBurn's UI. (OQ-7, R-G16–R-G19)
- **Production has two modes, and neither is the fallback of the other:** the plugin generates
  the file from a description, or the user draws it in LightBurn and the plugin supplies the
  layer parameters. Most real work is the second — the artwork is already the user's, and
  parameters are what they are stuck on. (R-W3.4)
- **In the second mode the parameters are delivered as text the user types in by hand** —
  named and united as LightBurn's UI names them, carrying the recipe's provenance and
  acceptance criteria. `.lbrn` is the only file v1 writes; a geometry-free `.lbrn` is rejected,
  and writing `.lbset` material libraries is **v2, deferred not rejected** — so nothing in v1
  may foreclose it, and a recipe must hold everything such an entry would need. (R-W3.5–R-W3.7,
  R-G1)
- **Text is live text, never outlines,** and the attribute set the writer already emits is
  enough for LightBurn. Typeface, weight and fit are left to the person with the file open —
  they are trivial to change there, unlike burn parameters, which is where the effort belongs.
  No font parser, no width tables, no availability checks. (OQ-12, R-G18, R-G19)
- **The writer emits `FormatVersion="1"`,** because that version can be checked by eye. Expect
  LightBurn to rewrite the file in its own current format the moment it saves; that is fine,
  because generated files are write-only and disposable — re-run the generator, never re-read a
  saved file. (OQ-8, R-G3, R-G4)
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
- **Run the tests with `dotnet run --project tests/LightBurn.Format.Tests`, not `dotnet test`.**
  The suite is xUnit v3 with its in-process runner; `dotnet test` reports "Zero tests ran" and
  exits non-zero, which looks like a broken suite and is not one. 37 tests, all passing.
