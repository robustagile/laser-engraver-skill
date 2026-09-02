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
| `07-fill-line` | **Not a probe — a file LightBurn itself saved** (AppVersion 2.1.04). The single most valuable reference here. | Keep. |
| `08-sublayer-passes` | Does a `SubLayer` carry its own `numPasses`? | **Settled** — yes, independent of its parent's. |
| `09-do-output` | Is `doOutput` the element that marks a layer as not output? | **OPEN** — and it is a safety question: guess wrong and a layer meant as a guide fires the laser. |
| `10-frequency-units` | Is `frequency` the Q-switch rate element, and is the value in Hz or kHz? | **OPEN** — the writer emits this name on a guess. |

Still unprobed, because the element name is not known well enough to guess at: **pulse duration**
for MOPA. That one is settled the other way round — by having LightBurn save a file with the
value set, and reading the name out of what it wrote.

## How to answer an open probe

1. Open the generated `.lbrn` in LightBurn, with a device profile of the right kind selected —
   frequency and pulse duration only appear for a fiber/galvo profile.
2. Look at what the probe's `HowToTell` says to look at, in the UI.
3. Record the answer in the table above, and in `../references/lbrn-format.md`, moving the fact
   from assumed to verified.

**"It opens" is not an answer**, and neither is re-saving the file and diffing it: LightBurn v2
rewrites the file on save whether or not it understood a given element.
