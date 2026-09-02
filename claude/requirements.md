# Requirements — Laser Engraving Assistant plugin

Status: **draft, gathered 2026-09-02.** This file records *what* the plugin must do and the
constraints it works under. It deliberately does not decide *how* — data schemas, file layout
inside the plugin, and code structure belong to the design stage. Open questions are listed at
the end with the reason each one blocks something.

Requirement IDs are stable; refer to them in design and status documents rather than restating
the text.

## 1. Purpose

A Claude Code plugin that helps a laser engraver owner go from "I have a machine" to "I have a
LightBurn file I can open and run", by:

- recording what equipment is on hand and helping configure and calibrate it;
- building a base of engraving recipes that are *verified on that specific machine*;
- generating LightBurn `.lbrn` files for production jobs and for test cards.

The plugin is distributed as a **GitHub repository**, not as a packaged/installed plugin.

## 2. Audience and scope

- **R-A1** **Primary audience:** the owner of a fiber/MOPA or CO2 machine with **basic
  experience** — someone who can already operate the machine and its software, but struggles
  with *practical application*: which parameters give which result on which material, and how to
  get there without wasting stock.
- **R-A2** **Further audiences**, addressed after v1: complete novices, and diode laser owners.
  They are not v1 targets, but no decision may be taken that makes serving them later
  impossible.
- **R-A3** The plugin targets the fiber/MOPA and CO2 **machine classes**, not two specific
  units. What is specific is *verification*: everything claimed is tested on the repository
  author's machines before it is aimed at anyone else. Where a finding turns out to be a
  property of those particular units rather than of the class, it is recorded as such.
- **R-A4** Depth before breadth: v1 covers the fiber/MOPA and CO2 classes thoroughly rather
  than every laser type superficially.
- **R-A5** Consequence of R-A1: the plugin does **not** teach how to click around LightBurn or
  how to run the machine. It teaches **application** — choosing parameters, why a parameter
  matters, how to test, and how to read the result. The data model is built for real depth; the
  documents and dialogue are pitched at this user, not at an expert.
- **R-A6** The agent leads the conversation, asking one thing at a time. A user stuck on
  practical application does not know in advance which facts about their setup turn out to
  matter.
- **R-A7** The agent must not ask for values this user cannot reasonably know — spot size, M²,
  true optical power. It asks what is on the machine, in the listing, or visible in LightBurn,
  and derives or researches the rest, stating plainly what it assumed.
- **R-A8** The agent explains *why*, not only *how much*, so the user can adapt when the result
  is not what they wanted.
- **R-A9** Where an expert would be offered a choice, this user is given a recommendation plus
  "you can do otherwise if …".

## 3. Language and documents

- **R-D1** Dialogue with the owner is in Russian; **every document, comment and identifier in
  the repository is in English**.
- **R-D2** Repository root contains:
  - `CLAUDE.md` — tells the agent where things are and how to work here;
  - `README.md` — human-facing description and usage;
  - `INSTALL.md` — installation and prerequisites;
  - `STATUS.md` — what works today and what is planned.
- **R-D3** `plugin/` holds the plugin itself. `claude/` holds requirements and design decisions.
  `test/` holds testing work.
- **R-D4** `test/` is **not published to GitHub** — it holds real machine profiles and real
  recipe data during development, which are private. (See OQ-1: this collides with the
  committed unit/golden test project, which must be in the repository.)

## 4. Target environment

- **R-E1** Machine control software is **LightBurn v2** (purchased). EZCAD is not in the loop.
- **R-E2** The exact LightBurn patch version is recorded during onboarding, because it changes
  and it matters. The known-good reference file was written by 2.1.04.
- **R-E3** Toolchain is **.NET SDK** (`net10.0`), chosen deliberately over Python/Node: on both
  Windows and Linux the .NET SDK is the smaller installation barrier when nothing is installed
  yet.
- **R-E4** The agent invokes the generator via `dotnet run`; there is no separate build step for
  the user.
- **R-E5** The owner's environment is WSL over Windows, with LightBurn on the Windows side.
  Generated files must land where Windows LightBurn can open them.
- **R-E6** Canonical units throughout: millimetres, mm/s, kHz, ns, watts and percent as
  applicable. The recipe store holds canonical values; presentation for a given software is a
  separate layer. Coordinate convention (axis direction, origin) is a property recorded per
  machine, not assumed globally.

## 5. Equipment

- **R-M1** The plugin records an inventory of machines on hand. Per machine, at minimum:
  laser type (fiber / MOPA / CO2 / diode), power, laser source vendor if known, engraver vendor
  if known, and the machine's adjustable ranges (frequency range, whether pulse duration is
  adjustable and its available values).
- **R-M2** **Lenses are first class.** A machine carries a *list* of lenses; field size, field
  centre and spot size are properties of the lens, not the machine. A recipe is valid for a
  **(machine, lens)** pair, since a 20 W source with a 110 mm lens and a 50 W with a 300 mm can
  behave alike.
- **R-M3** Tooling and fixtures are recorded: rotary, air assist, extraction, autofocus, Z axis.
- **R-M4** Wavelength is recorded when the user knows it or it can be researched; it is not
  required, and must be derivable from laser type where possible.
- **R-M5** Drive type (galvo vs gantry) is **not** asked of the user — the user thinks in terms
  of fiber/MOPA/CO2/diode. Where the distinction matters it is inferred from the laser type.
- **R-M6** No **instrumented** power calibration or drift measurement — the user has neither
  the instruments nor the need for them. It is handled **indirectly** instead: calibrating
  recipes to the machine (R-R14 step 4) absorbs whatever the machine actually does, including
  a source that does not deliver its nominal power. Two consequences follow. A verified
  recipe is valid for the machine *in the state it was in when it was verified*. And on an
  ageing source — a ten-year-old CO2 tube above all — recipes drift out of true, with the
  symptom being a known-good recipe no longer meeting its own acceptance criteria. That is
  the only drift signal available to us, and it is a further reason acceptance criteria are
  recorded rather than settings alone (R-R5).
- **R-M7** Anything the agent researched rather than read off the machine is stored **with its
  source and date**, so a guess is never later mistaken for a specification.
- **R-M8** Calibration outcomes (red-dot offset, scale correction, distortion correction, focus
  findings) are stored in the machine's record, with the date — not treated as a one-off
  procedure that leaves no trace. Rationale: a scale correction living only in LightBurn's
  device settings is silently lost when the profile is recreated, and every job then comes out
  the wrong size.
- **R-M9** Retired machines are removed from the active base. Their *recipe* data does not
  carry to a replacement machine; their *format and methodology* knowledge does.

### Verification hardware

The machines the repository author has available to test against. They define what can be
*verified*, not what the plugin is *for* (R-A3).

| Machine | Role | Availability |
|---|---|---|
| MOPA 60 W | primary verification machine | in transit, arrival order unknown |
| CO2 floor-standing, ~10 years old, unknown origin | second verification machine | in transit, arrival order unknown |
| old galvo fiber, early 2010s, 20 kHz ceiling | **retired**, out of the base | — |

- **R-M10** **Arrival order is unknown** — either machine may land first, and the plan must not
  depend on which does. Machine onboarding (workflow 1) is built to be validated on whichever
  arrives, and the writer extension for frequency and pulse duration (R-G2) must be ready early
  enough that a MOPA arriving first is not blocked waiting for it. CO2 work needs no writer
  extension; MOPA work does — which is why that extension is v1 work rather than later.
- **R-M11** It is an accepted risk that a ten-year-old CO2 controller may not be supported by
  LightBurn at all. Helping the user *determine that* is a valid outcome of workflow 1.

## 6. Recipes

- **R-R1** A recipe records: type of work (engraving / etching / annealing / cutting), goal
  (deep engraving, serialisation, NFA marking, colour marking, medal, business card, …),
  material, and the parameters to set.
- **R-R2** Parameters cover at least: power, speed, line interval, passes, and — where the
  machine has them — frequency and pulse duration.
- **R-R3** Material identification must be specific enough to be reproducible. Family alone
  ("steel") is insufficient; grade and **surface state** (raw / polished / brushed / anodised /
  powder-coated / plated / oxide) frequently matter more than the base material.
- **R-R4** A recipe is bound to the **(machine, lens)** pair it was verified on.
- **R-R5** Each recipe carries **acceptance criteria** — what "good" means for that goal, in
  measurable terms (depth in µm/thou, contrast, legibility, character height), defined *before*
  testing. Without this, "refine the parameters" degenerates into taste.
- **R-R6** Recipes are stored as **Markdown**.
- **R-R7** **Provenance is mandatory and user-visible.** Every recipe states where it came from:
  vendor default, community/web, computed by the agent, user-tested, or user-verified. This is
  load-bearing rather than cosmetic: at the moment a machine is registered there is **no
  verified data at all**, so every recipe begins its life web-sourced or computed and must stay
  visibly distinguishable from a real result for as long as it is one.
- **R-R8** The agent **may** compute or extrapolate a recipe for an unknown combination, but
  the result is presented as a starting point with its provenance shown, and paired with a test
  card rather than offered for use on a workpiece.
- **R-R9** Recipes record **known failure modes** ("above 40 % it burns through", "below
  30 kHz the dots are visible") and negative results, not only winning settings. Negative
  results are what stop a user from walking in circles.
- **R-R10** A burn result is a fact about a machine, a lens and a material — it is stored with
  the machine, not with the code or the card that produced it.
- **R-R11** Whether an explicit experiment log is kept separately from promoted recipes is a
  design decision (OQ-4), but keeping the readings is not optional.

### Recipe catalogue and lifecycle

- **R-R12** The plugin ships **no recipes**. What it ships is a codified **catalogue of which
  recipes are needed** — the standard set of work type x goal x material combinations worth
  having for a machine class. Recipes themselves come into existence only once a concrete
  machine is known.
- **R-R13** The catalogue is the coverage map. Catalogue minus what exists for this machine is
  the outstanding work list — this is what makes a gap *visible* rather than merely absent, and
  it is how workflow 2 knows what to do next.
- **R-R14** Every recipe follows this lifecycle, in order:
  1. **Machine registered** (workflow 1).
  2. **Research** — look for recipes for this exact machine; failing that, for this laser type
     and power class.
  3. **Seed** — whatever is found is recorded as a *candidate* marked `source: internet`, with
     the URL, the date, and **what it was written for**: which machine, power, lens and
     material.
  4. **Calibrate** — the candidate goes to test cards and is tuned to this machine.
  5. **Verified** — it becomes a recipe for this (machine, lens) pair.
- **R-R15** A candidate found on the internet is never presented as ready to use. It is a
  starting point for step 4, and the plugin says so plainly.
- **R-R16** "What it was written for" carries as much weight as the source URL: the distance
  between the machine a recipe was written for and the machine in hand is what determines how
  much calibration it needs — and whether it is usable at all.
- **R-R17** Where research finds nothing, the catalogue entry stays open and calibration starts
  from a computed starting point (R-R8) with a test card, provenance recorded as computed.

## 7. Workflows

### Workflow 1 — Machine onboarding and setup

- **R-W1.1** Step 1, registration: the user says what they have and what they know about it.
  Missing facts are researched on the internet where possible; where not, the plugin proceeds
  with what is known and says so.
- **R-W1.2** Step 2, setup: help configure LightBurn for this machine, and calibrate so that
  the red-dot frame matches the real mark in both position and size.
- **R-W1.3** For a machine of unknown origin, onboarding includes identifying the controller,
  establishing whether LightBurn can talk to it, measuring the working field, and setting focus.

### Workflow 2 — Building the recipe base

- **R-W2.1** Step 1: find recipes for this machine — research the internet for this exact
  machine, or failing that for this laser type and power class, and seed what is found as
  candidates with full provenance (R-R14). Nothing ships pre-filled (R-R12). This step can be
  returned to at any time, as the catalogue grows or better sources appear.
- **R-W2.2** Step 2: run test burns, read the results, and refine the parameters — one specific
  recipe at a time (e.g. NFA engraving, slate engraving, business-card etching).
- **R-W2.3** Test cards are generated by the plugin rather than delegated to LightBurn's
  built-in material test, because that tool varies only power and speed, while the parameters
  that matter on fiber/MOPA are **frequency, pulse duration, line interval, passes and
  defocus**.
- **R-W2.4** Every test card cell is labelled so a result can be reported unambiguously.
- **R-W2.5** Test burns are **mandatory before production**, not optional. The plugin must
  resist the instinct of taking a number from the internet straight to a workpiece.

### Workflow 3 — Production

- **R-W3.1** The user states what they want to make and what they have; the agent discusses how
  to do it and what is needed, then generates a LightBurn file the user can open and run.
- **R-W3.2** The generated file must be usable as-is: correct layer settings **and** geometry
  placed correctly on the field.
- **R-W3.3** Positioning matters — origin, where the workpiece sits, framing. A file that opens
  but marks in the wrong place is a failure.

### Cross-cutting workflow — Troubleshooting by symptom

- **R-W4.1** For this audience the most common entry point is a symptom, not a parameter
  question: "it
  came out grey instead of black", "it burned through", "nothing is visible", "the focus
  drifts". This is a first-class workflow, not a bonus, and it does not fit inside workflows
  1–3.

## 8. Safety

- **R-S1** Safety warnings are **proactive**, not on request. Knowing how to operate a machine
  is not the same as knowing which materials release hydrogen chloride, or that eyewear is
  wavelength-specific.
- **R-S2** The plugin must warn about materials that must not be lasered (PVC/vinyl, PTFE,
  polycarbonate, ABS, galvanised metal and high-zinc brass, CFRP) and about toxic dusts
  (chrome, cadmium, beryllium).
- **R-S3** Eye protection must be matched to the actual wavelength — 1064 nm, 10.6 µm and
  450 nm are not interchangeable.
- **R-S4** A standing disclaimer applies to every recipe: parameters are a starting point, and a
  sample test is always required.
- **R-S5** Safety is enforced in the plugin's instructions, not relegated to a note in the
  README.

## 9. Regulatory awareness

- **R-C1** The plugin must be *aware* that regulatory requirements exist for some jobs — NFA
  marking, MIL-STD-130 UID, medical UDI, DataMatrix quality grading — and must be able to
  **find** the current requirements.
- **R-C2** Regulatory text is **not** shipped transcribed into the plugin body — a baked-in
  copy goes stale silently. The plugin knows such requirements exist and how to look them up.
  However, a requirement researched **once** for work the user does repeatedly **may be
  cached**, so it is not re-derived from scratch every job. A cached requirement is usable
  only if it records **where it came from and when**: the date is what lets the plugin say
  "this was established N months ago, re-check whether it still holds" rather than quietly
  presenting old rules as current. The cache lives with the user's data, never in the shipped
  plugin — the same rule as R-M7 for researched machine facts.
- **R-C3** Where a job has a regulatory acceptance criterion (minimum character height, minimum
  depth), that criterion is expressed as a recipe acceptance criterion (R-R5) and as a check at
  generation time, with the verification method stated.

## 10. LightBurn file generation

- **R-G1** The plugin generates `.lbrn` files. Generation of other LightBurn artefacts
  (`.lbset` material libraries, device profiles) is a design-stage question, not a v1 commitment.
- **R-G2** The writer must be extended to emit **frequency** and **pulse duration**; without
  them the MOPA's two most important levers cannot be expressed. This is v1 work, not later.
- **R-G3** Files are generated **write-only**: the plugin produces them and LightBurn consumes
  them. Round-tripping is not a design goal — LightBurn v2 reads the older format version fine
  but rewrites it substantially on save.
- **R-G4** Generated output is disposable. The generator is the source of truth; an output
  directory is never an input.
- **R-G5** Every generated file is validated before it is written. Shapes on nonexistent
  layers, primitives referencing missing vertices and beziers missing handles must fail the
  write. A file that opens with silently missing geometry is worse than one that fails to write.
- **R-G6** All numeric output uses invariant culture. A comma decimal separator produces a file
  LightBurn silently mis-parses.
- **R-G7** Writes are atomic (temp file plus move), so an interrupted write cannot leave a
  half-written `.lbrn` where a real job should be.
- **R-G8** LightBurn's limit of **30 colour layers (C00–C29)** is a hard constraint on card
  design and must be enforced with a clear error at generation time.
- **R-G9** Test cards are **versioned**, and a recorded burn result references the card version
  it was burned from. Rationale: the previous project's rule "a burned card is a record, never
  change its geometry" worked because the card was private. In a distributed plugin, an update
  that changes a card's geometry would silently invalidate results users already recorded.
- **R-G10** The generator reads machine and lens facts from the machine record. It must not
  carry hard-coded constants for a particular machine.
- **R-G11** Requirements that are the same everywhere (e.g. NFA minimum character height and
  depth) are kept separate from facts about a particular machine (proven interval, frequency
  ceiling).

### Inherited format knowledge

Prior experimental work established the `.lbrn` format by hand-written probe files and by
reading a file LightBurn itself saved. That knowledge is machine independent and carries over
in full, unlike the burn results. The path it came from
(`/mnt/d/engraving/fiber/CLAUDE/`) is recorded for provenance only — it is imported once and
never read at run time (R-N4).

- **R-G12** The verified/assumed format knowledge is carried into the plugin as reference
  documentation, keeping the distinction between what was verified against a real render and
  what is still assumed.
- **R-G13** The LightBurn-saved reference file must be kept. Asking the user to build a case in
  LightBurn and save it is the highest-value way to settle a format question; one such file
  previously settled three questions at once.
- **R-G14** Three verification rules are inherited and must be stated in the plugin's own
  `CLAUDE.md`:
  1. **LightBurn silently normalises what it does not recognise.** An invented layer type was
     loaded, rewritten as a plain one and its contour dropped, with no error or warning.
  2. **"It opens" is not verification, and neither is a round-trip diff** — v2 rewrites the file
     on save regardless of whether it understood a given element. Verification is visual or
     measured in the UI.
  3. **An inferred constant must not be asserted by a test built on the same inference.** Two
     bugs previously survived precisely because their tests encoded the same wrong assumption.
- **R-G15** Two inherited format questions block work and must be settled early. Both can be
  answered with LightBurn alone, no laser required:
  - `doOutput` — the element that marks a layer as non-firing. If the name is wrong, a layer
    meant as a guide **actually fires the laser**. That makes it a safety blocker, not a
    nicety.
  - `frequency` (and, for MOPA, pulse duration) — element names unknown; the writer currently
    emits neither.
  Settling them takes **two steps**, not one: read the element names out of a file LightBurn
  wrote, then generate a file in the format the plugin emits carrying those elements and confirm
  LightBurn shows the values in its UI. Step one alone proves nothing about the older format
  version the plugin writes.

## 11. Non-functional

- **R-N1** Test stack: xUnit v3 with AwesomeAssertions, plus golden files pinning generated
  output. Golden tests are the only defence against silent format regressions and are not
  optional.
- **R-N2** `INSTALL.md` covers installing the .NET SDK per platform, verifying it, and a smoke
  test proving the installation is usable.
- **R-N3** Prerequisites stated up front, not as footnotes: LightBurn v2 (purchased) and the
  .NET SDK.
- **R-N4** The plugin is **fully self-contained**. Everything inherited from the prior
  experimental work is **copied into this repository once** — the format reference
  documentation, the file LightBurn itself saved, the probe files, the writer and card code,
  and the golden files. Nothing at run time reads from `/mnt/d/engraving/…` or any other path
  outside the repository. That directory is a one-time **source**, not a dependency: once the
  import is done, the plugin must behave identically if it disappears.
- **R-N5** Self-containment is about the plugin's own assets and knowledge. It does not apply
  to the **user's** data — machine records, recipes and generated output legitimately live
  outside the shipped plugin (R-D4, OQ-2). The distinction: shipped content must never depend
  on an external path, user content must never be baked into the shipped plugin.

## 12. Out of scope for v1

- Machine types other than fiber/MOPA and CO2 (R-A4). Diode lasers are a *later* audience
  (R-A2), not a permanent exclusion.
- **Instrumented** power measurement and drift tracking (R-M6). The machine's real behaviour
  is absorbed by recipe calibration instead, and drift shows up as a known-good recipe missing
  its acceptance criteria.
- Photo/raster engraving with in-plugin dithering — image preparation stays in LightBurn
  (revisit after v1).
- Variable data (serial number merge, DataMatrix/QR generation) — wanted, but after the
  geometry layer works.
- Rotary jobs.
- Prebuilt self-contained binaries instead of the .NET SDK — worth doing once the format
  stabilises, not now.

## Open questions for the design stage

- **OQ-1** `test/` is private and excluded from GitHub (R-D4), but the unit and golden test
  project must be committed. Two different things are currently competing for that name; the
  private working area and the committed test suite need distinct homes and unambiguous names.
- **OQ-2** Where do the owner's personal data live once the plugin is installed for real work?
  During development they live in `test/`. The previous project's one-Markdown-file-per-machine
  pattern is the leading candidate and worked well, but placement relative to an installed
  plugin is undecided.
- **OQ-3** How LightBurn generation is packaged inside the plugin — the mapping of the three
  workflows onto skills, slash commands and agents.
- **OQ-4** Whether an experiment log is kept separately from promoted recipes, and what promotes
  a candidate into a recipe (a single good burn, or a repeat).
- **OQ-5** ~~Where the starter recipe set comes from.~~ **Resolved:** no recipes ship at all.
  The plugin ships a catalogue of *which* recipes are needed, and recipes are researched and
  then calibrated per machine — see R-R12 to R-R17.
- **OQ-6** Primary channel for reporting test results: cell label in text, a photo of the card,
  or measurements. Photo reading is possible but unreliable for contrast and depth; text is
  unambiguous but manual.
- **OQ-7** How far geometry generation goes in v1: layer settings only, layers plus geometry
  (text with exact character height, primitives, SVG import, arrays), or also variable data.
- **OQ-8** Whether to keep emitting the older format version (proven to render correctly) or
  move to what LightBurn v2 writes natively.
- **OQ-9** How generated files reach Windows LightBurn from WSL, and where they are placed.
- **OQ-10** What the recipe catalogue contains for v1 — which work types, goals and materials
  are declared "needed" — and whether the catalogue is one global list, or differs per machine
  class (a CO2 and a MOPA do not want the same set).
- **OQ-11** The re-check policy for cached regulatory findings (R-C2): after how long, or on
  what trigger, does a cached requirement have to be looked up again before it may be relied on
  for a compliance job.
