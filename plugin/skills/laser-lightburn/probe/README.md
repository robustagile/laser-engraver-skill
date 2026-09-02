# Probe register

One file per format question. A probe is worth writing only if the competing answers make the
file look **obviously different** when it is opened — a probe that renders plausibly either way
proves nothing.

Generate the ones that are still open:

```
dotnet run --project ../tools/LightBurn.Probes -- <output-directory>
```

| Probe | Question | Status |
|---|---|---|
| `01-rect-only` | Does a bare `Rect` load, and are `W`/`H` full dimensions or half-extents? | **Settled** — full dimensions; `XForm` translation places the centre. |
| `02-lines` | Does `Path` with `VertList`/`PrimList` load? | **Settled** — yes, `V x y` vertices and `L a b` primitives. |
| `04-text-group` | Text anchoring and `Group`/`Children` nesting. | **Settled** — text is anchored at the TOP of the line; groups nest. |
| `06-rect-semantics` | Does `XForm` scale multiply `W`/`H`? | **Settled** — yes. |
| `07-fill-line-saved` | **Not a probe — a file LightBurn itself saved** (AppVersion 2.1.04). Settled Fill+Line, bezier handles and what LightBurn writes that this writer does not. | Keep. |
| `08-sublayer-passes` | Does a `SubLayer` carry its own `numPasses`? | **Settled** — yes, independent of its parent's. |
| `10-fiber-layer-settings` | Do `doOutput`, `hide`, `frequency` and `QPulseWidth` survive in the older format version this writer emits? | **OPEN** — every name and unit is verified, but only from files LightBurn wrote in its own format. |
| `11-fiber-frequency-qpulsewidth-saved` | **Not a probe — a file LightBurn itself saved** with a fiber profile, 5 kHz and 150 ns set. | Keep. Settled `frequency` (hertz) and `QPulseWidth` (nanoseconds). |
| `12-fiber-do-output-hide-saved` | **Not a probe — the same file re-saved** with Output and Show unchecked. | Keep. Settled `doOutput` and revealed `hide`. |

A probe numbered `09-do-output` was written and then never needed: asking for a saved file
answered its question first. Nothing was lost, but the order is worth remembering — **ask for a
saved file before writing a probe.**

Two saved files settled four elements and both of their units in the space of a few minutes.
`QPulseWidth` and `hide` were not names anyone would have arrived at by guessing, and the second
file — the same case re-saved with two checkboxes cleared — worked because **LightBurn omits
elements sitting at their default**, so the way to make an element appear is to change it away
from the default and save again.

## How to answer an open probe

1. Open the generated `.lbrn` in LightBurn, with a device profile of the right kind selected —
   frequency and pulse duration only appear for a fiber/galvo profile.
2. Look at what the probe's `HowToTell` says to look at, in the UI.
3. Record the answer in the table above, and in `../references/lbrn-format.md`, moving the fact
   from assumed to verified.

A file LightBurn saved answers a question outright, but only for the format LightBurn itself
writes. Confirming that this writer's older format version carries the same element is a
separate step, and it is what probe `10` is for.

**"It opens" is not an answer**, and neither is re-saving the file and diffing it: LightBurn v2
rewrites the file on save whether or not it understood a given element.
