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

- **The fiber layer elements are verified by name and unit, but only in LightBurn's own format.**
  `doOutput` (0 means it does not fire), `hide`, `frequency` (hertz — a UI showing 5 kHz wrote
  5000) and `QPulseWidth` (nanoseconds) all came from two files LightBurn saved, kept as
  `probe/11-*-saved.lbrn` and `probe/12-*-saved.lbrn`. That says nothing about the older format
  version this writer emits. Probe `10-fiber-layer-settings` is generated and **waiting to be
  opened**; it writes the same 5 kHz and 150 ns so the only difference is the format version.

  ```
  dotnet run --project plugin/skills/laser-lightburn/tools/LightBurn.Probes -- <output directory>
  ```

- **Element order is untested.** LightBurn writes `frequency` and `QPulseWidth` before `priority`
  and `doOutput`; this writer puts them after. It already tolerates other differences, so order
  is probably free — but it is the first hypothesis to check if the probe comes back showing
  defaults.
- **OQ-7** — how far geometry generation goes in v1: layer settings only, layers plus geometry,
  or variable data as well.
- **OQ-8** — whether to keep emitting the older `.lbrn` format version or match what LightBurn
  v2 writes natively. The writer currently stamps `AppVersion="1.5.06"`, and LightBurn 2.1.04
  also writes `Thumbnail`, `VariableText`, `UIPrefs` and `Notes`, none of which this writer
  emits.
- **Neither verification machine is on hand.** The MOPA 60 W and the CO2 are both in transit and
  the arrival order is unknown, so nothing may be planned that depends on which lands first
  (R-M10).

## Where to start the next session

1. **Open `10-fiber-layer-settings.lbrn`** in LightBurn with a fiber/galvo profile and check the
   four layers as the probe's `look` line describes. That closes R-G15 outright.
2. **Then start on the skills themselves** — neither `SKILL.md` exists yet, and until one does
   nothing is installable. `laser-lightburn` is the one with machinery behind it already.
3. **Write the install scripts** (R-N7 to R-N9), so the plugin can be deployed into a `.claude`
   and tried for real.
