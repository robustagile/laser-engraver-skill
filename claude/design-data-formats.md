# Design — data formats

Second design pass. It decides the **shape of every file the plugin reads and writes**: the
machine record, the recipe, the catalogue, the config, the regulatory cache, and the
specification `laser-machines` hands to `laser-lightburn`.

Depends on `design-composition.md`, which settled what the parts are and where they live.

## 1. Two audiences, two formats

Everything the *user and the agent* work with is **Markdown with YAML-style frontmatter**
(R-R6): frontmatter for the facts, body for the reasoning. Everything passed *from the agent to
the generator* is **JSON**.

The split exists because the two have different failure modes. Prose is where rationale,
negative results and "why we stopped trying that" survive; a burn reading with no explanation
is nearly worthless a month later. But prose is a terrible input to a program, and the generator
must never guess.

**The frontmatter grammar is deliberately small**, so it can be parsed by ~100 lines in the
generator with no NuGet dependency (`System.Text.Json` is in the BCL, a YAML library is not, and
a first build that needs the network is a barrier we chose not to add — R-E3):

- `key: scalar` — string, number, or one of the enumerated words;
- `key: [a, b, c]` — a flat list of scalars;
- one level of list-of-objects, indented two spaces, used only for lenses;
- nothing else. **Anything the parser does not recognise is an error, not a warning** (R-G5).

## 2. Three conventions that apply to every file

**The unit is part of the field name.** `speed_mm_s`, `depth_um`, `frequency_khz`,
`interval_mm`, `pulse_width_ns`. Never a bare `depth` or `speed`.

This is not tidiness. The prior work recorded a depth target of "0.01" and then could not
establish whether it meant 0.01 mm or 0.01 in — a factor of 25 between "already achieved" and
"eight times away" — and the question was never resolved before the machine was retired. A field
name that carries its unit cannot produce that.

**Every linear dimension is metric.** Millimetres for anything a user sets or measures with a
rule, micrometres for depth. Inches appear in exactly one place — the prose of an acceptance
criterion, naming the regulation the metric figure came from, as in "0.0762 mm (0.003 in, the
NFA minimum)". No field is ever in inches, and no field name offers a choice of unit.

**Every mandatory key is always present, and `unknown` is a legal value.** A missing key is a
parse error; an unknown value is a fact about the machine. This matters because a half-finished
onboarding has to be resumable (composition §4), and because the generator can then fail
precisely — "this card needs `spot_mm`, which is `unknown` for lens `f110`" — rather than
finding a hole at some later point.

**A numeric fact may carry its own source.** `<field>_source: measured | estimated | researched
| vendor`, defaulting to `measured` when absent. The prior project had exactly one instance of
this (`SpotSizeConfirmed` on a lens) and it earned its keep — every pulse-overlap calculation
rested on an estimate, and the flag is what kept that visible. Generalised, it is also how R-M7
is satisfied without a separate provenance ledger.

## 3. `config.md` — one per installation

```
---
output_directory: D:/engraving/out
lightburn_version: 2.1.04
default_machine: mopa-60w
---
```

`output_directory` must be reachable by LightBurn (composition §5). `lightburn_version` is
recorded because it changes and it matters (R-E2). The body holds anything the user wants to
note about their setup.

## 4. `machines/<id>.md` — one per machine

```
---
id: mopa-60w
name: JPT MOPA 60 W
state: calibrated
type: mopa
power_w: 60
wavelength_nm: 1064
wavelength_nm_source: researched
source_vendor: JPT
machine_vendor: unknown
machine_model: unknown
controller: unknown            # the board - R-W1.3 turns on identifying it
frequency_min_khz: 1
frequency_max_khz: 4000        # the source's envelope - see below, it is not reachable at
                               # every pulse width
pulse_widths:                  # illustrative values; the real table is the source's own
  - { ns: 2,  cutoff_khz: 1950, max_khz: 4000 }
  - { ns: 13, cutoff_khz: 412,  max_khz: 3000 }
  - { ns: 200, cutoff_khz: 30,  max_khz: 600 }
axis_y: up
origin: bottom-left
lenses:
  - id: f110
    focal_mm: 110
    field_x_mm: 110
    field_y_mm: 110
    spot_mm: 0.03
    spot_mm_source: estimated
tooling:                       # a map rather than a list: "unknown" and "no" are different
  rotary: yes                   # answers, and for extraction the difference is a safety one
  air_assist: yes
  extraction: unknown
  autofocus: no
  z_axis: yes
---
```

**A MOPA's frequency ceiling is a property of the pulse width, not of the machine.** This was
found while registering a real 60 W MOPA: its source manual publishes a fixed list of selectable
pulse widths and, for each one, two frequencies — the **cut-off**, above which the energy per
pulse starts to fall, and the **maximum** the source will accept at all. At the shortest width
the ceiling was the full envelope; at 200 ns it was a fraction of it. A single
`frequency_max_khz` would let a candidate recipe be validated against a frequency this machine
cannot deliver at the pulse width the recipe calls for — which is precisely the check
`recipe-base.md` performs before seeding.

So `frequency_min_khz` and `frequency_max_khz` stay as the source's envelope, and
`pulse_widths` carries the real per-width limits. Two further consequences worth writing down,
both from the same manual:

- The selectable widths are a **fixed list**, and a value typed in between two of them is
  rounded by the source to the nearest one. A recipe carrying an unavailable `pulse_width_ns` is
  not a recipe for this machine.
- `cutoff_khz` is what explains a machine that stops getting darker as frequency rises: above
  it, more pulses each carry less energy. That belongs in a recipe's `## Failure modes` rather
  than being rediscovered per material.

A plain fiber source has no `pulse_widths` at all — its pulse width is whatever it is — and a
CO2 has neither. The key is present only where the machine has the lever.

`state` is the ladder from composition §4: `registered` -> `configured` -> `calibrated`. It
advances on evidence, not on intent: `configured` means LightBurn talks to the machine and a
device profile exists; `calibrated` means a commanded size was measured on material and matched,
and the red-dot frame agreed with the mark.

A `<fact>_source` key says what **kind** of knowledge a value is - `owner-stated`, `derived`,
`researched`, `estimated`, `measured` - and `## Provenance` carries the URL and date for the
researched ones. Not redundant: the key is what a later session reads to decide whether to trust
a number, the section is how it checks where that number came from.

**The schema is repeated inside the skill, in `references/onboarding.md`.** That is deliberate
duplication rather than an oversight: this document is not installed, so a skill that cited it
would be citing something it cannot read. It was found the hard way - a session with the field
list only in here invented `output_dir` for `output_directory`, and two machines registered in
two sessions would not have had the same keys. This document holds the reasoning; the shipped
reference holds the authority.

Fixed body sections, so the agent can find them without reading the whole file:

- `## Unknown` — what is still not established, and what it blocks. This is the section that
  makes a half-finished onboarding resumable, and it is why `unknown` is a legal frontmatter
  value rather than an omission.
- `## Calibration` — dated entries (R-M8). This is where a correction that lives in LightBurn's
  own settings gets written down. The prior machine needed a 0.8 scale correction on one lens,
  without which every job came out 25 % oversize, and nothing in the code compensated for it —
  a fresh LightBurn profile would have silently lost it.
- `## Provenance` — for each researched fact, the URL and the date (R-M7).
- `## Notes` — quirks, and the levers not yet tried.

## 5. `recipes/<machine>/<lens>/<id>.md` — one per line of inquiry

The path encodes the (machine, lens) binding (R-R4), so a recipe cannot be read out of context.

```
---
id: stainless-304-brushed-black
catalogue_entry: black-marking-stainless
state: verified
provenance: user-verified
work: engraving
goal: black-marking
material_family: stainless-steel
material_grade: "304"
material_surface: brushed
power_pct: 60
speed_mm_s: 500
interval_mm: 0.01
passes: 1
frequency_khz: 40
pulse_width_ns: 20
defocus_mm: 0
---
```

When `provenance: internet`, three further keys are mandatory (R-R16): `source_url`,
`source_date`, and `source_written_for` — the machine, power and lens the recipe was written
for. The last is not decoration: the distance between that machine and this one is what says how
much calibration is needed, and whether the candidate is usable at all.

`state` is the recipe ladder: `candidate` -> `calibrating` -> `verified`.

Fixed body sections:

- `## Acceptance criteria` — inherited from the catalogue entry, possibly tightened (R-R5).
- `## History` — the log. Every attempt: date, the settings, the card and **card version**
  (R-G9), what came out, and the verdict.
- `## Failure modes` — what goes wrong on either side of the setting, in the user's own terms
  (R-R9).

### OQ-4, resolved: the log lives inside the recipe

There is no separate `log/` tree, and the layout in composition §2 drops it.

A reading is only meaningful attached to the settings that produced it. The prior project learned
this expensively: its scratch card was edited between burns, so a result read off it "means
nothing unless the settings are written down with it" — and they had to be copied by hand into
the machine file, where they were separated from the card that produced them. One file per line
of inquiry, carrying its own history, removes that gap.

A file may sit in `candidate` forever with a history of nothing but failures and no working
settings. That is a useful record, not an empty one, and the coverage view has something real
to point at.

### What promotes a candidate to verified

**Meeting the acceptance criteria on a confirmation burn at the final settings** — not on the
tuning matrix that produced them.

The reason is in the prior data. Two cells of a twenty-cell card were reported as the optimum
for annealed metal, and the honest note added afterwards says "optimum means best of the twenty,
not the best the machine can do", with the true peak somewhere between them and unresolvable at
that resolution. The winning cell of a matrix is evidence about the matrix. One burn at the
chosen settings, judged against criteria written before the test, is evidence about the setting.

### OQ-6, resolved: how results are reported

**Text, naming the labelled cell**, is the primary channel — every card cell is self-describing
(R-W2.4), so "R3C5" or "`[500mm-50%]`" is unambiguous and needs no interpretation.

A **photo** is accepted for gross triage only ("everything above row 4 is charred"), never as
the basis for a `verified` state: contrast and depth do not survive a phone camera. A
**measurement** is required wherever the acceptance criterion is dimensional — depth for NFA
marking is measured, not judged.

## 6. The catalogue — in the skill, not the data store

`skills/laser-machines/references/catalogue/<entry>.md`, one file per entry:

```
---
id: annealing-stainless-steel
work: annealing
goal: black-marking
material_family: stainless-steel
material_surface: anodised   # optional; present only when the surface changes the goal
applies_to: [fiber, mopa]
priority: 1
regulatory: false
---
```

Body: `## Acceptance criteria` and `## How to verify`.

**The catalogue is where acceptance criteria are authored**, which is the whole reason it can
ship while recipes cannot (R-R12). Criteria have to exist *before* the first test, or "refine the
parameters" has nothing to converge on (R-R5) — and they are the one part of a recipe that is a
property of the *goal* rather than of the machine. A recipe inherits its criteria from its
catalogue entry and may tighten them, never loosen them.

Coverage (R-R13) is then computable: catalogue entries whose `applies_to` includes this machine's
type, minus the recipe files present for it, is the work list.

`material_surface` is optional and appears only where the surface changes the goal rather than
the settings — anodised aluminium, where the laser interacts with the coating and not the metal.
It was added after the entries were authored: without it, "etch the anodising white" and "cut
through it to depth" collapse into one entry, and they are different jobs.

`applies_to` is **mandatory on every entry and never means "all"** (OQ-10). The classes do not
want the same catalogue: colour marking on stainless is a MOPA goal, reachable through pulse
duration and frequency, and it is not a thing a CO2 owner should ever be offered. One directory,
one list per class computed from it.

## 7. Material vocabulary

`skills/laser-machines/references/materials.md` holds the permitted tokens for
`material_family`, `material_surface`, and the families' known grades. Recipes must use tokens;
anything free-form goes in the body.

A controlled vocabulary is what makes two recipes comparable at all (R-R3). `anodised` versus
`raw` aluminium is a larger difference than aluminium versus brass, and a base that spells the
first one three ways cannot tell the user that.

## 8. Regulatory cache

`<data>/regulatory/<topic>.md`:

```
---
topic: nfa-marking
source_url: https://www.ecfr.gov/...
retrieved: 2026-09-02
---
```

**Policy, resolving OQ-11:** a cached finding older than **12 months** must be looked up again
before it is relied on for a compliance job, and the retrieval date is stated every time the
requirement is used. Under 12 months it may be used as-is, with its date shown. This keeps R-C2's
bargain — cache to avoid re-deriving, but never present an old rule as a current one.

## 9. The spec — JSON, agent to generator

Two things flow into the generator, and they are deliberately different in kind:

- **Facts** — the generator reads the machine record itself, directly (R-G10). Not transcribed
  by the agent, because a transcription is a place for a value to change silently.
- **Intent** — the agent writes a JSON spec: what card or job, which lens, which axes and cells,
  what geometry, where the output goes.

```json
{
  "kind": "test-card",
  "card": "power-speed-matrix",
  "machineRecord": "~/laser-skill-data/machines/mopa-60w.md",
  "lens": "f110",
  "output": "D:/engraving/out/mopa-60w-stainless-matrix.lbrn",
  "axes": [
    { "field": "power_pct", "values": [40, 50, 60, 70, 80] },
    { "field": "speed_mm_s", "values": [300, 500, 750, 1000] }
  ],
  "fixed": { "interval_mm": 0.01, "passes": 1, "frequency_khz": 40 }
}
```

JSON rather than more Markdown because this file is machine-to-machine, transient, and
`System.Text.Json` costs nothing. The generator validates the spec against the machine record it
read — a speed above the machine's ceiling, or a card that does not fit the lens field, is a
failure to write, not a file that opens and marks wrongly (R-G5, R-G8).

## 10. What this pass does not decide

- Nothing raised by this pass is still open.

Resolved elsewhere since this pass was written: **OQ-7** (layers, geometry, fonts and raster —
see R-G16 to R-G19), **OQ-8** (emit `FormatVersion="1"`), **OQ-12** (live text, not outlines)
and **R-G15** (every emitted element verified in LightBurn's UI).
- The card catalogue itself: which cards exist, what each varies, and their versions.
