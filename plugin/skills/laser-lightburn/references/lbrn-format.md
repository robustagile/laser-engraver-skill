# The `.lbrn` format — what is verified, and what is assumed

The format is undocumented. Everything here was established either by writing probe files by
hand and having someone report what rendered, or by reading a file LightBurn itself saved. Both
kinds live in `../probe/`.

Keep the distinction between the two lists below. Treating an assumption as a fact is how the
expensive mistakes happened.

## The rule that costs the most to relearn

**LightBurn silently normalises what it does not recognise.** A layer written with an invented
`type="Scan+Cut"` was loaded, rewritten as a plain `Scan`, and its contour dropped — no error,
no warning.

Two consequences:

- **"It opens successfully" is not verification.** It means only that LightBurn did not reject
  the file. Dimensions, positions and anchors have to be *measured* or seen.
- **A round-trip diff is not verification either.** LightBurn v2 rewrites the file on save
  whether or not it understood a given element, so a difference proves nothing and a match
  proves nothing.

And one that applies to the code rather than the file: **an inferred constant must not be
asserted by a test built on the same inference.** That is not independent evidence. Both the
text-anchor and the glyph-height mistakes survived precisely because their tests encoded the
same wrong assumption.

## Verified against real LightBurn renders

- Root `LightBurnProject` attributes; `CutSetting` with `<name Value="…"/>` children.
- `Rect` — `W`/`H` are **full** dimensions, not half-extents. `XForm` translation places the
  **centre**. `XForm` scale multiplies `W`/`H`.
- `Ellipse` (`Rx`/`Ry`), and `Group`/`Children` nesting.
- `Path` — `VertList` / `PrimList`, with `V x y` vertices and `L a b` line primitives.
- **Bezier handles are absolute coordinates**, not offsets from the vertex. LightBurn writes
  e.g. `vx="13.790039" c0x="13.790039" c0y="-3.9153123"` — full coordinates.
- **Fill+Line is a `Scan` layer with a nested `<SubLayer type="Cut" index="1">`** carrying its
  own `maxPower` and `speed`. **There is no `Scan+Cut` layer type** — writing one makes
  LightBurn silently drop the contour and load the layer as a plain fill.
- **A `SubLayer` carries its own `numPasses`**, independent of its layer's, so a contour can run
  a different number of passes from the fill it rides on. Settled with
  `probe/08-sublayer-passes.lbrn`. LightBurn's own files omit default values, so a saved
  single-pass Fill+Line shows nothing either way; this writer emits it explicitly.
- **Text is anchored at the TOP of the line.** Glyphs run from the `XForm` translation *minus*
  the height, up to the translation.
- Text `Height` is a cap height. Glyphs with brackets, ascenders or descenders ink roughly 30 %
  taller than the nominal height — the ink box is not the `Height`.

## Assumed — do not trust without checking

- **`doOutput`**, the element marking a layer as not output. If the name is wrong, a layer meant
  as a guide **will fire the laser**. LightBurn omits default-valued elements when it writes, so
  a saved file with every layer enabled does not show this one. Probe: `09-do-output`.
- **`frequency`**, for the fiber Q-switch rate — both the element name and whether the value is
  Hz or kHz. Probe: `10-frequency-units`.
- **Pulse duration** for MOPA: not modelled at all, and the name is not known well enough to
  guess. This one is settled the other way round — have LightBurn save a file with the value
  set, and read the name out of what it wrote.

## What LightBurn writes versus what this writer emits

`probe/07-fill-line.lbrn` is a **file LightBurn itself saved** (AppVersion 2.1.04) and is the
most valuable reference here. LightBurn reads both forms, so none of these differences need
changing:

| LightBurn 2.1 writes | This writer emits |
|---|---|
| `FormatVersion="0"`, plus `DeviceName` / `AskForSendName` | `FormatVersion="1"`, no device |
| `<V vx=".." vy=".." c0x=".." …/>` elements, `<P T="L" p0=".." p1=".."/>` | `VertList` / `PrimList` strings |
| `Font="Arial,-1,100,5,400,0,0,0,0,0"` (a Qt font descriptor) | `Font="Arial"` |
| Text keeps a `<BackupPath>` of rendered outlines | live text only |
| omits elements sitting at their default value | writes them explicitly |

## Settling a new question

Write a probe into `../probe/` that isolates **one** question, with a known-good shape alongside
for comparison, and design it so the competing answers look obviously different. Then have the
file opened and the result reported from the UI.

**Getting another file that LightBurn itself saved is the highest-value thing to ask for.** One
such file previously settled three open questions at once. When a question comes up, ask for the
case to be built in LightBurn and saved as `.lbrn`, rather than guessing at it.

## Constraints that shape what can be generated

- **LightBurn has 30 colour layers, C00–C29.** A card that wants two layers per cell therefore
  tops out at fourteen cells plus an outline.
- All numeric output must go through invariant-culture formatting. A comma decimal separator
  produces a file LightBurn silently mis-parses.
