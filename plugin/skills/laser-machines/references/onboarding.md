# Onboarding a machine

Registration - workflow 1 step 1 (R-W1.1). This file gets a machine record written and the state
to `registered`. Getting LightBurn to talk to it is `lightburn-setup.md`, and proving it marks
true is `calibration.md`.

Registration is a conversation, not a form. Ask in the owner's own terms, research what can be
researched, and **write the record before the session ends even if half of it is unknown** - an
unwritten registration is one that has to be redone.

## What to ask the owner

The minimum for a usable record (R-M1, R-M2, R-M3):

| Ask | Why it matters | Where they will find it |
|---|---|---|
| Type: fiber, MOPA, CO2 or diode | Decides the whole parameter vocabulary and which catalogue entries apply | They know this |
| Power in watts | Power class is what recipe research matches on | Nameplate, or the listing they bought from |
| Laser source vendor and model | The frequency range and pulse-duration table are properties of the *source*, and researchable from its model code | Sticker on the source itself, often inside the cabinet |
| Machine vendor | What the vendor's own documentation and community cover | Nameplate |
| Lens or lenses: focal length each | Field size and spot follow from the lens, and a recipe is bound to it | Engraved on the barrel, usually `F=110mm` or similar |
| Tooling: rotary, air assist, extraction, autofocus, Z axis | Extraction is a safety answer, not a convenience one | They know this |

**Never ask galvo versus gantry** (R-M5). The owner thinks in fiber/MOPA/CO2/diode, and the
drive follows from that: fiber and MOPA are galvo, CO2 is a gantry unless it is a galvo CO2, and
where the distinction matters it is inferred rather than asked.

Two things worth asking that are not in the record's frontmatter: **what they bought it to do**,
because it sets which catalogue entries are priority 1 for them, and **what materials they
actually have on the shelf**, because a recipe for a material they do not have can never be
verified (R-R18) and should be visible as a gap from the start.

## What to research rather than ask

An owner who has just unboxed a machine does not know its frequency range, and asking makes it
their problem. Research it, and record where the answer came from and when (R-M7).

Worth researching, in this order:

1. **The source model code** - a JPT, Raycus or MAX model number gives the frequency range and,
   on a MOPA, the pulse-duration table. This is the highest-value lookup in registration: those
   two facts decide what the machine can be asked to do, and they are published. The source
   vendor's own manual is worth finding; a marketing page will give the envelope and none of the
   detail below.

   On a MOPA, get **the table, not the range**. Such a source has a fixed list of selectable
   pulse widths - a value typed between two of them is rounded to the nearest, so a recipe
   calling for an unavailable width is not a recipe for this machine - and for each width, two
   frequencies: the **cut-off**, above which energy per pulse begins to fall, and the **maximum**
   the source accepts at all. A ceiling quoted for the source as a whole is generally reachable
   only at the shortest widths. Record it as the `pulse_widths` table below, and record
   `frequency_max_khz` as the envelope it is.

   That table is worth this much because it is what a candidate recipe is checked against
   (`recipe-base.md`), and because `cutoff_khz` is the explanation for a machine that stops
   getting darker as frequency rises.
2. **The machine model** - field size per lens, whether the Z axis is motorised, what ships in
   the box.
3. **Wavelength**, if it is not derivable: fiber and MOPA are 1064 nm, CO2 is 10.6 µm, diode is
   typically 450 nm. Derive it, record `wavelength_nm_source: derived`, and only research when
   the machine is something unusual.

Do not research the **spot size** and present it as fact. It depends on the beam diameter
entering the lens and on the beam quality, neither of which is usually on any spec sheet. It can
be estimated:

```
spot_mm  ~=  1.27 * M2 * wavelength_mm * focal_mm / beam_diameter_mm
```

which for 1064 nm through a 110 mm lens lands around 0.02 mm - order of magnitude, not a
measurement. Record it as `spot_mm_source: estimated`. The real spot is established by burning a
single line and measuring it, which belongs to `calibration.md`.

**Every researched fact gets a line in the record's `## Provenance` section**: the fact, the
URL, and the date it was retrieved. A number without that becomes indistinguishable from a
measurement within a month.

## A machine of unknown origin

A machine bought used, or built from parts, or ten years old with no documentation. Registration
then has an extra step, and it comes *before* any recipe thinking (R-W1.3, R-M11):

1. **Identify the controller.** It is a labelled board inside the cabinet - Ruida (`RDC…`),
   Trocen (`AWC…`), TopWisdom, Leetro (`MPC…`), or on a fiber galvo a BJJCZ/JCZ board. The label
   is the answer; the front panel's appearance is a hint at best.
2. **Establish whether LightBurn can drive it at all.** Some controllers it has never supported,
   and the honest outcome of onboarding is sometimes "not with this software". Finding that out
   on day one is worth more than any recipe, and it invalidates everything downstream if it is
   discovered late.
3. **Measure the working field** rather than trusting a listing, and **find focus** by burning
   rather than by the spacer that came with it.

Record the finding either way. "LightBurn cannot drive this controller, established on <date>,
here is what the board says" is a complete and useful machine record.

## Lenses

A lens is not an attribute of the machine, it is an item in a list, and a recipe is valid for
the **(machine, lens)** pair (R-M2). A 20 W source with a 110 mm lens and a 50 W with a 300 mm
can behave alike, so the pair is the unit of comparison.

Per lens: `id` (`f110`), `focal_mm`, `field_x_mm`, `field_y_mm`, `spot_mm` with its source. Field
size is the lens's, and the owner may have bought a second lens without noticing that every
recipe they have is bound to the first.

## Writing the machine record

`<store>/machines/<id>.md`. `id` is kebab-case and says what the owner calls it -
`omg-mopa-60w`, not `machine-1`.

**These field names are the schema, not a suggestion.** Two machines registered in two sessions
have to come out with the same keys, or nothing downstream can read both. Where a fact is not
established, the value is the literal `unknown` - an omitted key is indistinguishable from a
question nobody asked.

```yaml
---
id: omg-mopa-60w                 # kebab-case, matches the filename
name: OMG Laser 60W MOPA         # what the owner calls it
state: registered                # registered -> configured -> calibrated
registered: 2026-09-03           # the date this record was created

type: mopa                       # fiber | mopa | co2 | diode
power_w: 60
machine_vendor: OMG Laser
machine_model: unknown
wavelength_nm: 1064
wavelength_nm_source: derived    # owner-stated | derived | researched | estimated | measured

source_vendor: JPT               # the laser source, not the machine
source_model: M7
frequency_min_khz: 1             # the source's envelope
frequency_max_khz: 4000
frequency_source: researched
pulse_width_min_ns: 2            # envelope; the table below is what recipes are checked against
pulse_width_max_ns: 500
pulse_width_source: researched
pulse_widths:                    # MOPA only: per selectable width, its own frequency limits
  - { ns: 2,  cutoff_khz: 1950, max_khz: 4000 }
  - { ns: 13, cutoff_khz: 412,  max_khz: 3000 }
                                 # or the literal `unknown` until the source manual is found

controller: unknown              # the board, e.g. BJJCZ/JCZ, Ruida RDC6442 - R-W1.3 turns on it
axis_y: unknown                  # up | down - established in lightburn-setup.md
origin: unknown                  # bottom-left | centre | ... - likewise

lenses:                          # a list: a recipe is valid for a (machine, lens) pair
  - id: stock                    # f110 once the focal length is known
    focal_mm: unknown
    field_x_mm: unknown
    field_y_mm: unknown
    spot_mm: unknown
    spot_mm_source: unknown
    note: the lens the machine shipped with; not yet identified

tooling:                         # a map, not a list: "unknown" and "no" are different answers,
  rotary: unknown                # and for extraction the difference is a safety one
  air_assist: unknown
  extraction: unknown
  autofocus: unknown
  z_axis: unknown
---
```

A `<fact>_source` key says **what kind** of knowledge a value is - `owner-stated`, `derived`,
`researched`, `estimated`, `measured`. `## Provenance` carries the URL and date for the
researched ones. The two are not redundant: the key is what a later session reads to decide
whether to trust a number, the section is how it checks where it came from.

A plain fiber source has no `pulse_widths`, and a CO2 has neither that nor a frequency envelope
worth recording. Omit what the machine does not have; that is not the same as `unknown`.

**Keys, section headings and enum values are always English; the prose is in the language of
the conversation.** The record is the owner's document and they read it, so the explanations
belong in their language - but `## Unknown` has to be findable by a session that has not read
the file, and `state: registered` has to mean the same thing in every record. Two registrations
that disagree about which half is which produce a base nothing can read.

Four body sections, always present, so they can be found without reading the whole file:

- `## Unknown` - what is still not established, **and what each gap blocks**. This is what makes
  a half-finished registration resumable, and `unknown` is a legal frontmatter value for exactly
  this reason: an omitted field is indistinguishable from a forgotten question.
- `## Calibration` - empty at registration; dated entries later (R-M8).
- `## Provenance` - every researched fact, with URL and date.
- `## Notes` - quirks, and what has not been tried.

### `config.md`, written the first time the store is created

One per installation, at `<store>/config.md`, and the field names are as fixed as the record's:

```yaml
---
output_directory: unknown        # where generated .lbrn files go - must be a path LightBurn
                                 # can open, which on WSL means a Windows-visible one
lightburn_version: unknown       # exact, patch included: the format and the UI both change
default_machine: omg-mopa-60w
---
```

`unknown` until asked, and both are worth asking before the first generated file rather than at
the moment one is needed.

Write it, then say plainly what is in it and what is not. The owner should see the gaps, because
the gaps are what the next session picks up.

## Resuming a half-finished registration

Read `## Unknown` **first**, and ask only what is still listed there. Re-interrogating an owner
about facts already in the record is the fastest way to make them stop using the tool.

If something in `## Unknown` blocks the step being asked for, say which fact and why - "the
frequency range is unknown, so a candidate recipe cannot be checked against what this machine
can actually do" - rather than proceeding on an assumption.
