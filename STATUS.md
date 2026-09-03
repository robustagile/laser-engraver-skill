# Status

As of **2026-09-03**.

This file says what exists, what is open, and where to start. It is not a log — see the
housekeeping rules in `CLAUDE.md`.

## What exists today

- **Requirements** — `claude/requirements.md`, 91 numbered requirements with stable IDs.
- **Design, first pass** — `claude/design-composition.md`: delivery by clone and copy script,
  the installed layout, two skills (`laser-machines`, `laser-lightburn`), four commands, and the
  data store as the only channel between them.
- **Design, second pass** — `claude/design-data-formats.md`: the shape of the machine record,
  the recipe, the catalogue, the config and the regulatory cache, plus the JSON specification
  the agent hands the generator.
- **The `.lbrn` writer** — `plugin/skills/laser-lightburn/tools/LightBurn.Format/`. Shapes,
  layers and sub-layers, path encoding, transforms, document validation, invariant-culture
  number formatting, atomic writes. It emits `FormatVersion="1"` (OQ-8).
- **The probe generator** — `plugin/skills/laser-lightburn/tools/LightBurn.Probes/`, and the
  register at `plugin/skills/laser-lightburn/probe/README.md`.
- **Format knowledge** — `plugin/skills/laser-lightburn/references/lbrn-format.md`, verified
  facts kept separate from assumed ones, plus the file LightBurn itself saved.
- **Tests** — `plugin/tests/`, 37 passing over the writer and the transform maths.
- **The recipe catalogue** — `plugin/skills/laser-machines/references/catalogue/`, 30 entries
  across seven works and seven materials, each carrying acceptance criteria and how to verify
  them, plus the material vocabulary in `references/materials.md`.
- **The installers** — `install.sh` and `install.ps1`, both exercised: user level and project
  level, first install and re-install, the `.gitignore` advice, the developer symlink mode, and
  the installed generator building where it lands. They copy files and nothing else, replace
  the skills and commands wholesale, never look inside the data store beside the target, and
  stamp a `VERSION`
  beside each `SKILL.md` (R-N7 to R-N9). A real Claude Code session in a project-level install
  finds both skills and all four commands by description alone, and routes correctly: a machine
  question loads `/laser-machine` and then `laser-machines`, a symptom loads `/laser-fix` and
  then `laser-machines`. Both skills state where the data store is and that it is derived rather
  than searched for, which was measured: one listing of the derived path replaced the four to
  six blind shell calls the frame provoked, and nothing reached into the clone (R-N4, observed
  in a copy install - a `--link` install cannot show it).
- **`laser-machines`, workflow 1 complete** — `SKILL.md` (safety, provenance, the mandatory
  test burn, the store, routing by state), `references/onboarding.md` with the machine-record and
  `config.md` schemas inlined, `lightburn-setup.md`, `calibration.md`, and the `/laser-machine`
  command. Exercised on the bench: a real session registers a 60 W MOPA and writes a record that
  matches the schema key for key.
- **The research half of workflow 2** — `references/recipe-base.md`: coverage from the
  catalogue, what to search for and in what order, seeding with mandatory provenance, the
  recipe schema inlined, and how to compute a starting point when research finds nothing. Plus
  the `/laser-recipes` command. A web-enabled run found the source's own manual and the
  per-pulse-width frequency table from it.
- **Skeletons for the rest** — `laser-lightburn/SKILL.md` and its two references, plus the
  troubleshooting, production and regulatory sections of `laser-machines/SKILL.md`: structure
  with a `TODO` per section naming the requirements it has to satisfy.
- **Human-facing docs** — `README.md`, `INSTALL.md`, this file, `CLAUDE.md`.

Deliberately **not** imported from the prior experimental work: the card generators, the lens
and machine constants, and their golden files. All of those encode the retired machine's
numbers and its cards' geometry, and the cards will be redesigned for the MOPA and the CO2. The
Arial advance-width table is machine-independent and can be brought over when cards are written.

**No skill body is written yet.** The plugin installs, and what installs is a frame: neither
skill can actually do anything. What exists is the machinery they will call, the catalogue they
will read, the frame that says which file holds what, and a working way to deploy it.

## Open problems

- **`laser-lightburn` is still a frame**, as are troubleshooting, production and the
  regulatory sections of `laser-machines`. Workflow 1 works; nothing else does.
- **The raster half of OQ-7 does not exist in the writer.** There is no bitmap shape, and the
  `Image` layer type has never been read back in LightBurn's UI, so nothing raster can be
  emitted yet (R-G16). Text is done: live text with the attributes the writer already emits,
  verified in `probe/04-text-group` (R-G18).
- **Depth-map marking rests on three unknowns** (R-G17): which LightBurn image mode modulates
  depth rather than dithers, what bit depth survives into the `.lbrn`, and how grey level maps
  to micrometres — the last being a per-(machine, lens, material) calibration, so it is blocked
  behind a machine as well.
- **Three catalogue entries cannot be generated at all yet.** The `photo-marking-*` entries need
  a bitmap shape and a verified `Image` layer, neither of which exists (R-G16) — the raster half
  of OQ-7 now has concrete dependants rather than being speculative.
- **The catalogue has never been read by a skill**, because no skill exists to read it. Its 30
  entries and their acceptance criteria are authored from domain knowledge and have not been
  through a burn, so every criterion is a proposal until the machines arrive.
- **No test cards exist for either machine.** The prior machine's cards were deliberately not
  imported, and the new ones cannot be designed until there is a machine record to design
  against.
- **Neither verification machine is on hand.** The MOPA 60 W and the CO2 are both in transit and
  the arrival order is unknown, so nothing may be planned that depends on which lands first
  (R-M10). The catalogue reflects this: it covers fiber and MOPA only, and **the CO2 half is
  deliberately unauthored** until the machine is here and its controller is identified —
  whether LightBurn can drive it at all is the question that comes before any recipe (R-W1.3),
  and it can invalidate the whole CO2 branch.

## Where to start the next session

The ordering below is set by what the owner needs **on the day the machine is assembled**, which
is onboarding and calibration — not file generation.

1. **Verify workflow 1 in an interactive session, on the machine that is coming.** Everything
   so far was measured headlessly, and `lightburn-setup.md` carries an explicit warning that its
   UI specifics were written without LightBurn open. The day the MOPA is on the bench, run
   registration for real, correct that file from what the owner sees, and let calibration write
   the first `## Calibration` entry.
2. **Fill in the troubleshooting body** (R-W4.1). It is the most common way in for this
   audience, it needs no machine and no generator, and the bench shows a symptom question
   already routes to it - into a file that is still headings.
3. **Fill in `laser-lightburn/SKILL.md`** — machinery already behind it. It has to establish where
   the data store is, check for the .NET SDK on first use (R-N8), and carry the one rule that
   must never be missed: an element whose name has not been seen read back in LightBurn's UI
   must not be emitted.
4. **Then the first test card**, for the priority-1 catalogue entries on stainless and anodised
   aluminium — the work that starts the moment the machine is calibrated.
