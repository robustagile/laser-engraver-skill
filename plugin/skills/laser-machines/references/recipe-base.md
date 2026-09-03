# Building the recipe base

Workflow 2 (R-W2.1 to R-W2.4) and the recipe lifecycle (R-R14). The plugin ships **no recipes**
(R-R12) - what it ships is the catalogue of *which* recipes a machine of this class needs. A
recipe comes into existence for a concrete machine, is seeded from research or computation, and
becomes real only after a burn.

The lifecycle, and the state each step ends in:

| Step | Needs the machine? | Ends at |
|---|---|---|
| 1. Machine registered (workflow 1) | yes | `calibrated` |
| 2. Research this machine, then this class | no | - |
| 3. Seed as a candidate with full provenance | no | `candidate` |
| 4. Calibrate on test cards | **yes** | `calibrating` |
| 5. Confirmation burn against the criteria | **yes** | `verified` |

Steps 2 and 3 can be done with no machine on the bench, and that is worth doing ahead of time.
Steps 4 and 5 cannot be faked, and until they happen every number is a candidate however good
the source was.

## The coverage view

Entries in `references/catalogue/` whose `applies_to` includes this machine's `type`, minus the
recipe files that exist under `recipes/<machine>/<lens>/`, is the work list (R-R13).

Present it as coverage, ordered by the entry's `priority`, and show each recipe's state and
provenance rather than a bare tick. `priority: 1` means "what a new machine needs in its first
sessions" (R-R19) - the surface marks that show whether the machine is set up correctly at all,
and whatever regulatory work drove the purchase - not what is most interesting.

Two things the view must not hide:

- **A candidate is a gap, not a done item.** A row with `internet` provenance and no burn is
  outstanding work that happens to have a starting point.
- **A material the owner does not have is a permanent gap** (R-R18). Say so on the row. It stays
  at `candidate` indefinitely, and the standing temptation is to let a well-sourced candidate for
  such a material quietly read as finished.

## Researching candidates

In this order, and stop when something specific enough is found (R-W2.1):

1. **This exact machine** - the machine vendor's own tables, the community around that model,
   the parameter set that shipped in its EZCAD installation.
2. **This laser source** - the source model is often shared across many machine brands, and a
   parameter table for it transfers far better than one from a different source of the same
   power.
3. **This laser type and power class** - a 60 W MOPA table is a usable starting point for another
   60 W MOPA. Across a factor of two in power it is a hint about direction, not a number.

What a source is worth depends on what it states about itself. A table that names the machine,
the lens, the material grade and its surface is worth ten posts that say "60 % power, 1000 mm/s"
with no context. A photograph of the result raises the value of a post considerably; a video
where the settings are visible on screen is better than a comment quoting them.

Take **all** the parameters, not the two that are quoted: power, speed, line interval, passes,
and on fiber and MOPA the frequency and pulse duration too (R-R2). A recipe missing the
frequency is not a recipe for a MOPA, and filling that gap silently from a default is how a
candidate becomes untraceable.

**Check the candidate against what the machine can actually do** before seeding it. On a MOPA
that means the `pulse_widths` table rather than the envelope: the pulse duration must be one of
the widths the source actually offers, and the frequency must be within *that width's* maximum,
which is usually well below the figure quoted for the source as a whole. A candidate that fails
either check is - it says something about direction and it must not be recorded
as though it could be typed in. If the machine's ranges are unknown, that gap blocks this step,
and it belongs in the record's `## Unknown` rather than being assumed.

## Seeding a candidate

One file per line of inquiry: `recipes/<machine>/<lens>/<id>.md`. The path carries the
(machine, lens) binding so a recipe cannot be read out of context (R-R4) - `<lens>` is the
lens's `id` from the machine record, and a recipe cannot be filed at all while that lens has no
identity beyond "stock".

**These field names are the schema.** A recipe that spells a key its own way is a recipe nothing
can compare:

```yaml
---
id: stainless-304-brushed-black
catalogue_entry: annealing-stainless-steel   # the entry it covers, by its id
state: candidate                             # candidate -> calibrating -> verified
provenance: internet                         # vendor-default | internet | computed
                                             #   | user-tested | user-verified
source_url: https://…                        # mandatory when provenance is internet (R-R16)
source_date: 2026-09-03
source_written_for: 50 W JPT MOPA, 110 mm lens, 304 stainless

work: annealing                              # engraving | etching | annealing | cutting
goal: black-marking
material_family: stainless-steel             # tokens from references/materials.md
material_grade: "304"                        # quoted: 304 is a name, not a number
material_surface: brushed

power_pct: 60
speed_mm_s: 500
interval_mm: 0.01
passes: 1
frequency_khz: 40
pulse_width_ns: 20                           # only where the machine has the lever
defocus_mm: 0
---
```

Every linear dimension is metric and the unit is part of the name - `speed_mm_s`, never a bare
`speed`, and never inches. A regulation written in inches is converted, and the imperial
original is kept in prose as the source.

`source_written_for` is not decoration (R-R16): the distance between that machine and this one
is what the calibration step reads to know how far the candidate has to travel, and whether it
is worth travelling at all.

The body carries `## Acceptance criteria`, inherited from the catalogue entry and never loosened
(R-R5); `## History`, empty until the first burn; and `## Failure modes`, which starts empty
unless the source said something about them.

Seed it, then say plainly that it is a starting point for a test card and not a setting to use
(R-R15).

## When research finds nothing

The catalogue entry stays open and calibration starts from a **computed** starting point (R-R17,
R-R8) - `provenance: computed`, and it is worth no more and no less than a good guess with the
reasoning attached.

Computing one is reasoning about levers, not looking up numbers. What the plugin ships is the
reasoning; the numbers are the machine's to give:

- **Fluence versus average power.** Percentage power is not transferable between sources at all -
  what carries is energy per pulse and how much of it lands in a spot. A 60 W source at 20 % and
  a 20 W source at 60 % are not the same machine at the same setting.
- **Pulse duration decides the regime** on a MOPA, and it is the lever a plain fiber does not
  have. Short pulses concentrate energy in time: ablation, removal, grey and white marks. Long
  pulses spread it: heating, oxide growth, annealed blacks and colours. Almost every "it came out
  grey instead of black" is this lever in the wrong place.
- **Frequency sets overlap and heat accumulation.** Low frequency gives separated, visible dots
  and a coarse mark; high frequency gives continuous heating. With speed, it decides how many
  pulses land per millimetre.
- **Line interval decides whether a fill is a surface at all.** It must be at or below the spot
  size for coverage; wider and the mark is a set of stripes, whatever the other settings say.
- **Passes are not a multiplier.** A second pass over a thermal mark can remove what the first
  one grew, and on annealing it usually lightens rather than darkens.
- **Defocus lowers peak intensity** without touching power, which is the honest way to move from
  ablation towards heating when the pulse duration will not go further.

Write the reasoning into the recipe's body. A computed candidate whose reasoning is recorded can
be argued with after the burn; one that is just numbers cannot.

## Calibrating a candidate

TODO: the test card, the burn, reading the result, refining. Needs the machine on the bench and
needs the card generator (`laser-lightburn/references/test-cards.md`), neither of which exists
yet. One recipe at a time, one parameter axis at a time.

## Promoting a candidate to verified

TODO: R-R14 step 5. The confirmation burn at the final settings - not the winning cell of the
tuning matrix, which is evidence about the matrix rather than about the setting.
