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
- **Tests** — `plugin/tests/`, 32 passing over the writer and the transform maths.
- **Human-facing docs** — `README.md`, `INSTALL.md`, this file, `CLAUDE.md`.

Deliberately **not** imported from the prior experimental work: the card generators, the lens
and machine constants, and their golden files. All of those encode the retired machine's
numbers and its cards' geometry, and the cards will be redesigned for the MOPA and the CO2. The
Arial advance-width table is machine-independent and can be brought over when cards are written.

**No skill or command file exists yet**, so nothing is installable and neither skill can be
invoked. What exists is the machinery they will call.

## Open problems

- **Two format elements are unverified** (R-G15), and one is a safety matter. **The probes are
  written and waiting to be opened** — `dotnet run --project
  plugin/skills/laser-lightburn/tools/LightBurn.Probes -- <output directory>` generates both.
  - `09-do-output` — is `doOutput` the element marking a layer as non-firing? Guess it wrong and
    a layer meant as a guide fires the laser.
  - `10-frequency-units` — is `frequency` the Q-switch rate element, and is its value in Hz or
    kHz? Two layers carry the two readings of "42 kHz" and a third sets nothing, so LightBurn's
    default is visible for comparison.
- **Pulse duration for MOPA is not modelled at all**, and the element name is not known well
  enough to probe. It has to be settled the other way round: LightBurn saves a file with the
  value set, and the name is read out of what it wrote.
- **OQ-7** — how far geometry generation goes in v1: layer settings only, layers plus geometry,
  or variable data as well.
- **OQ-8** — whether to keep emitting the older `.lbrn` format version or match what LightBurn
  v2 writes natively. The writer currently stamps `AppVersion="1.5.06"`.
- **Neither verification machine is on hand.** The MOPA 60 W and the CO2 are both in transit and
  the arrival order is unknown, so nothing may be planned that depends on which lands first
  (R-M10).

## Where to start the next session

1. **Answer the two open probes.** Open `09-do-output.lbrn` and `10-frequency-units.lbrn` in
   LightBurn with a fiber/galvo device profile selected, read the UI as each probe's `look` line
   describes, and record the answers in the probe register and in `references/lbrn-format.md`.
   Neither needs a laser.
2. **Get a file LightBurn wrote with pulse duration set**, and read the element name out of it.
3. **Then confirm it the other way**: emit that element in the format the plugin writes, and
   check LightBurn shows the value in its UI. Reading a name out of a LightBurn-written file
   proves nothing about the older format version the plugin emits.
