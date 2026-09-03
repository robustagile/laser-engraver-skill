# Design — composition

First design pass. It decides **what parts exist, how they are delivered, how they talk to each
other, and where shared data lives**. It deliberately does not decide the *format* of that data
— that is the next pass, and it depends on the answers here.

Requirements referenced by ID from `requirements.md`. Where this document settles an open
question, it says so; where it needs a requirement reworded, it says that too.

## 1. Delivery

**No marketplace.** The user clones the repository and follows `INSTALL.md`, which deploys the
skill and commands into their own `.claude`. The clone is needed only to install and to update;
nothing at run time reads from it.

**The whole payload is deployed**, generator included. The alternative — leaving the generator
in the clone and pointing the installed skill at it — would make the clone a permanent
dependency whose path has to be recorded and kept valid. A .NET project sits next to `SKILL.md`
exactly as script files would.

**The installer is a pair of small scripts** — `install.sh` and `install.ps1`. A .NET installer
was considered and rejected on bootstrap order: the SDK is exactly what may not be present yet,
so a program that needs it cannot be the thing that checks for it. The scripts do one dumb job,
copying files, and need nothing installed to run. They must be **idempotent** and must report
what they changed.

**The .NET check belongs to the skill, not the installer.** On first use the skill verifies the
SDK is present and, if it is not, gives the installation guidance for the user's platform. That
puts the check where it can hold a conversation about it, and keeps the installer free of any
prerequisite of its own.

**Copies, not symlinks.** Symlinks would make `git pull` take effect immediately, but on Windows
they need developer mode or elevation. A copy plus a `VERSION` stamp lets the skill notice that
the installed copy is behind the clone and say so. A symlink mode may exist behind a flag for
developing the plugin itself.

**Both scopes are supported**, and the install target is an argument to the script:

- **user level** (`~/.claude`) — machines and recipes are shared across every directory, and the
  workflows run from anywhere;
- **project level** (`<project>/.claude`) — everything stays inside one working folder, which is
  what someone keeping their user level deliberately spare will want.

The data store is a sibling of the install (section 2), so the scope chosen also decides where
the machine records live. A project-level install with a shared record base is still possible:
`config.md` can point the store elsewhere, which is the escape hatch for exactly that case.

**One hazard specific to project level:** a project's `.claude` is often committed. The user's
machine records, recipes and burn results must not be — so a project-level install prints the
`.gitignore` line to add, and the skill checks for it before it writes the first record.

**No namespace.** Without a marketplace the skill and commands land in the same `~/.claude` as
everything else the user has, so every name carries a `laser-` prefix.

## 2. Installed layout

`<target>` below is `~/.claude` for a user-level install, or `<project>/.claude` for a
project-level one. The shape is identical either way.

```
<target>/
  skills/laser-machines/          <- DISPOSABLE: replaced wholesale on install
    SKILL.md                        the domain: router + safety + provenance
    references/
      onboarding.md
      lightburn-setup.md
      recipe-base.md
      calibration.md
      troubleshooting.md
      catalogue/                    which recipes are needed (R-R12)
    VERSION
  skills/laser-lightburn/         <- DISPOSABLE: replaced wholesale on install
    SKILL.md                        the mechanics: what may and may not be emitted
    references/
      lbrn-format.md                verified vs assumed (R-G12)
      test-cards.md
      jobs.md
    tools/                          the .lbrn generator (.NET), built on first use
    probe/                          format probes + the file LightBurn itself saved (R-G13)
    VERSION
  commands/laser-*.md             <- DISPOSABLE: replaced wholesale on install

  laser-engraver/                 <- THE USER'S: never touched by the installer
    config.md                       output directory, preferences, LightBurn version
    machines/<machine>.md           one file per machine (R-M1, R-M8, R-R10)
    recipes/<machine>/<lens>/       one file per line of inquiry, carrying its own
                                    history of burns (R-R4, R-R9; OQ-4 in formats §5)
    regulatory/                     cached findings, with retrieval dates (R-C2)
```

The data store is named after neither skill, because both read it.

**The invariant, and the reason the layout is split this way:** everything under `skills/` and
`commands/` is disposable and is replaced wholesale by an install;
everything under `laser-engraver/` belongs to the user and is never written by the installer.
Two directories, one rule each. Had the user's data lived inside the skill directory, an install
that replaces that directory would destroy a recipe base that took weeks of burns to build.

This resolves **OQ-2** (where personal data live) and **OQ-3** (how the plugin is packaged).

## 3. Composition: two skills, four commands

The split is by **subject matter**, and it follows the one real seam in the material:

| Skill | Owns | Nature of its knowledge |
|---|---|---|
| `laser-machines` | Equipment, recipes, the catalogue, calibration method, troubleshooting | Physical, per-machine, only verifiable by burning something |
| `laser-lightburn` | The `.lbrn` format, the generator, test-card and job construction | Software, machine-independent, verifiable with LightBurn alone |

That seam is real rather than tidy-looking. Format knowledge carried over intact when the old
machine was retired while every burn result went with it (R-M9), and the two open format
questions can be settled with no laser present at all (R-G15). Keeping them apart also means a
session about material behaviour does not carry the XML lore, and a session about the format
does not carry the recipe base.

**The contract between them** is narrow and one-directional: `laser-lightburn` is a service.
Given a machine record and a card or job specification, it produces a `.lbrn`. It holds no
opinion about materials, and it runs no dialogue about what to engrave. `laser-machines` decides
*what* to burn and *why*; `laser-lightburn` decides *how the file has to be written* so that
LightBurn does what was intended.

Both read the same data store, so `laser-lightburn` takes the machine and lens facts it needs —
field size, spot, frequency ceiling — from the record rather than being handed them (R-G10).
Because the installer controls the layout, either skill can reference the other by a known
relative path; the generator lives under `laser-lightburn/tools/` and nothing duplicates it.

A single combined skill was the first proposal and is rejected: it would put two kinds of
knowledge with two entirely different verification methods behind one description, and every
session would load both.

**Four commands, one per workflow** (R-W1 to R-W4):

| Command | Workflow | Skills it draws on |
|---|---|---|
| `/laser-machine` | 1 | `laser-machines` |
| `/laser-recipes` | 2 | `laser-machines`, plus `laser-lightburn` for the test cards |
| `/laser-job` | 3 | both — the domain chooses the recipe, the mechanics write the file |
| `/laser-fix` | 4 | `laser-machines` |

A command is an entry point, not a stage gate: each one branches on the state it finds rather
than assuming the user arrived in the right order. `/laser-job` on a machine with no verified
recipe for the material says so and offers to go and calibrate one; `/laser-recipes` on an
uncalibrated machine points back at `/laser-machine` first.

**Rules that have to be in a `SKILL.md` rather than a reference file**, because a rule in a
reference file is only read when something goes looking for it:

- in `laser-machines` — safety (R-S1, R-S5) and provenance (R-R7, R-R15);
- in `laser-lightburn` — that an element whose name is not verified must not be emitted at all.
  The `doOutput` case (R-G15) is why: guess it wrong and a layer meant as a guide fires the
  laser.

**Subagents:** a candidate, not a commitment for v1. Web research for recipes and for regulatory
requirements (R-R14 step 2, R-C1) produces a lot of noisy context for a small result, which is
what a subagent is for. Deferred until the main flow works.

## 4. How the parts communicate

**The data store is the only channel.** No workflow may assume that anything is still in the
conversation: onboarding happens one day, calibration over the following weeks, a job months
later. Each command locates the store, reads what it needs, runs its dialogue, and writes back.

**Everything is resumable, so state has to be explicit.** A half-finished onboarding must be
picked up next session, which means a machine record has to say what is still *unknown*, not
merely what is known. The same applies to a recipe that has been seeded but not yet calibrated.

**Two state ladders carry the whole flow:**

- A **machine** is `registered` -> `configured` -> `calibrated`. Commands branch on it.
- A **recipe** is `candidate` (researched or computed, R-R14 step 3) -> `calibrating` ->
  `verified` (R-R14 step 5). Provenance is a property of the recipe throughout, and the ladder is
  what makes a candidate visibly not a result.

**The catalogue is the coverage view** (R-R13): the catalogue in the skill, minus the recipes in
the user's store for this machine, is the work list. This is why the catalogue ships and the
recipes do not — it is the one thing that can be authored ahead of time.

## 5. Generated output

Records and deliverables are different things and go to different places.

- **Records** — machines, recipes, log — live under `~/.claude/laser-engraver/`. Only the agent
  reads them; they need no visibility from anywhere else.
- **Generated `.lbrn` files** must be opened by LightBurn, which on this setup runs on Windows
  while the agent runs in WSL and `~` is inside the distribution. So the output directory is a
  **setting in `config.md`**, established during onboarding and pointing at a Windows-visible
  path.

This resolves **OQ-9**. The generator writes only where that setting says, and output stays
disposable (R-G4).

## 6. Updating

`git pull` in the clone, then re-run the install script against the same target. It replaces the
skill and the commands, leaves `laser-engraver/` alone, and updates `VERSION`. `SKILL.md` compares its
`VERSION` against the clone when the clone is reachable, so a stale installation is visible
rather than silent.

## 7. OQ-1, and what this repository holds

**Resolved without renaming anything: the private working area does not exist here.** The owner
installs the plugin separately and does real work in that installation, so this repository holds
tests in the pure sense — `plugin/tests/` — and `.gitignore` has no private area to exclude.

The loop between the two is plain: he hits a problem in real work, comes back here, it gets
fixed, he re-installs and carries on. Nothing is migrated in either direction.

## 8. Requirement amendments this pass needs

- **R-N4** says the plugin reads nothing at run time from outside *the repository*. With the
  payload deployed into `~/.claude`, "the repository" is the wrong noun: self-containment now
  means the *installed set* carries everything it needs. Same intent, stale wording.
- **New requirement needed:** the installer must never write inside the user's data directory,
  and must be idempotent. This is the invariant in section 2 and it is load-bearing enough to be
  a requirement rather than a design note.
- **New requirement needed:** the installer itself must have no prerequisites, and the .NET SDK
  check with its installation guidance belongs to the skill on first use — a consequence of
  bootstrap order, not a preference.
- **New requirement needed:** both install scopes are supported, and a project-level install
  must keep the user's records out of the project's git history.
- **R-D3/R-D4** need updating: there is no private `test/` working area in this repository at
  all (section 7). The committed test suite lives at `plugin/tests/`, and `.gitignore` no longer
  needs to exclude a private area.
- **R-D4** is reduced accordingly: this repository holds no private data, and the loop with the
  owner's installation is report-fix-reinstall rather than any kind of migration.

## 9. What came next

The formats are settled in `design-data-formats.md`: the machine record, the recipe, the
catalogue, the config, the regulatory cache, and the JSON specification this document called
for in section 3. That pass also resolved OQ-4, OQ-6 and OQ-11.

No design question is open after both passes. **OQ-10** is resolved — the catalogue differs by
machine class and its v1 content is authored. **OQ-12** is resolved (live
text; LightBurn makes the glyphs), and **OQ-7** is resolved — v1 generates layers, geometry, fonts and raster,
including depth maps, which adds R-G16 to R-G19. **OQ-8** is resolved (`FormatVersion="1"`), and
**R-G15** is closed: every element the writer emits has been read back in LightBurn's UI.
