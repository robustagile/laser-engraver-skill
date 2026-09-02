# Status

As of **2026-09-02**.

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
  number formatting, atomic writes.
- **The probe generator** — `plugin/skills/laser-lightburn/tools/LightBurn.Probes/`, and the
  register at `plugin/skills/laser-lightburn/probe/README.md`.
- **Format knowledge** — `plugin/skills/laser-lightburn/references/lbrn-format.md`, verified
  facts kept separate from assumed ones, plus the file LightBurn itself saved.
- **Tests** — `plugin/tests/`, 37 passing over the writer and the transform maths.
- **Human-facing docs** — `README.md`, `INSTALL.md`, this file, `CLAUDE.md`.

Deliberately **not** imported from the prior experimental work: the card generators, the lens
and machine constants, and their golden files. All of those encode the retired machine's
numbers and its cards' geometry, and the cards will be redesigned for the MOPA and the CO2. The
Arial advance-width table is machine-independent and can be brought over when cards are written.

**No skill or command file exists yet**, so nothing is installable and neither skill can be
invoked. What exists is the machinery they will call.

## Open problems

- **No skill or command file exists**, so nothing is installable and neither skill can be
  invoked. What exists is the machinery they will call.
- **No install scripts** (R-N7 to R-N9), so the plugin cannot be deployed into a `.claude` and
  tried for real.
- **OQ-7** — how far geometry generation goes in v1: layer settings only, layers plus geometry,
  or variable data as well.
- **OQ-8** — whether to keep emitting the older `.lbrn` format version or match what LightBurn
  v2 writes natively. Less pressing than it looked: `FormatVersion="1"` carries every element
  needed so far, including the fiber and MOPA ones. What LightBurn 2.1.04 writes and this writer
  does not — `Thumbnail`, `VariableText`, `UIPrefs`, `Notes`, `DeviceName` — has caused no
  trouble in anything opened so far.
- **No test cards exist for either machine.** The prior machine's cards were deliberately not
  imported, and the new ones cannot be designed until there is a machine record to design
  against.
- **Neither verification machine is on hand.** The MOPA 60 W and the CO2 are both in transit and
  the arrival order is unknown, so nothing may be planned that depends on which lands first
  (R-M10).

## Where to start the next session

1. **Write `laser-lightburn/SKILL.md`** — the skill with machinery already behind it. It has to
   establish where the data store is, check for the .NET SDK on first use (R-N8), and carry the
   one rule that must never be missed: an element whose name has not been seen read back in
   LightBurn's UI must not be emitted.
2. **Write the install scripts** (R-N7 to R-N9) so the plugin can be deployed and tried.
3. **Then `laser-machines/SKILL.md`** and the machine record format, which is what the first
   real onboarding will exercise.
