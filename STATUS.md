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
- **Human-facing docs** — `README.md`, `INSTALL.md`, this file, `CLAUDE.md`.

Deliberately **not** imported from the prior experimental work: the card generators, the lens
and machine constants, and their golden files. All of those encode the retired machine's
numbers and its cards' geometry, and the cards will be redesigned for the MOPA and the CO2. The
Arial advance-width table is machine-independent and can be brought over when cards are written.

**No skill or command file exists yet**, so nothing is installable and neither skill can be
invoked. What exists is the machinery they will call and the catalogue they will read.

## Open problems

- **No skill or command file exists**, so nothing is installable and neither skill can be
  invoked. What exists is the machinery they will call.
- **No install scripts** (R-N7 to R-N9), so the plugin cannot be deployed into a `.claude` and
  tried for real.
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

**The install scripts cannot come first.** A skill directory without a `SKILL.md` is not a skill:
deploying what exists today would copy `references/`, `tools/` and `probe/` into a `.claude` and
produce nothing invocable. There is something to install once there is something to invoke.

1. **Write `laser-machines/SKILL.md`** and the machine record it writes: registration, then
   LightBurn setup and calibration (R-W1.1 to R-W1.3). This is the first thing a newly assembled
   machine actually needs, and the first calibration can be done with geometry drawn in
   LightBurn by hand — mode 2 — so it does not wait on the generator. One command with it,
   `/laser-machine`, so the workflow has an entry point.
2. **Write the install scripts** (R-N7 to R-N9), which now have a real payload, and deploy for
   the first time. The install is only verified when the installed skill can be invoked and
   writes a machine record in the data store beside it.
3. **Write `laser-lightburn/SKILL.md`** — machinery already behind it. It has to establish where
   the data store is, check for the .NET SDK on first use (R-N8), and carry the one rule that
   must never be missed: an element whose name has not been seen read back in LightBurn's UI
   must not be emitted.
4. **Then the first test card**, for the priority-1 catalogue entries on stainless and anodised
   aluminium — the work that starts the moment the machine is calibrated.
