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
- **`doOutput` is the element that marks a layer as not output**, and `0` means it does not
  fire. Unchecking Output in LightBurn 2.1.04 wrote `<doOutput Value="0"/>`. Evidence:
  `probe/12-fiber-do-output-hide-saved.lbrn`. LightBurn omits the element when the layer *is*
  output, which is why a saved file with everything enabled shows nothing either way; this
  writer emits it explicitly so a reference layer is never ambiguous.
- **`hide` is the editor-visibility element** — unchecking Show wrote `<hide Value="1"/>`. Same
  evidence file. Visibility only: it is `doOutput` that decides whether the laser fires, and the
  two are independent.
- **`frequency` is the Q-switch rate element, and its value is in hertz.** A UI showing 5 kHz
  wrote `<frequency Value="5000"/>`. Evidence:
  `probe/11-fiber-frequency-qpulsewidth-saved.lbrn`.
- **`QPulseWidth` is the MOPA pulse-duration element, and its value is in nanoseconds.** A UI
  showing 150 ns wrote `<QPulseWidth Value="150"/>`. Same evidence file. This one could not have
  been guessed; it came from asking for a saved file.
- **Text alignment is explicit**, as `Ah` and `Av` attributes on the `Shape`. So "anchored at the
  top of the line" is the behaviour of the *default* `Av`, not a property of the format — the
  anchor can be set rather than worked around.
- `maxPower2` exists alongside `maxPower`, written even for a single-source device.
- **All four of those elements are read back from the format this writer emits**, not only from
  LightBurn's own. Probe `10-fiber-layer-settings` wrote them at `FormatVersion="1"` with the
  same 5 kHz and 150 ns, and all four read correctly in the UI.
- **Element order inside a `CutSetting` does not matter.** LightBurn writes `frequency` and
  `QPulseWidth` before `priority` and `doOutput`; this writer puts them after, and the same probe
  read correctly either way.
- Text `Height` is a cap height. Glyphs with brackets, ascenders or descenders ink roughly 30 %
  taller than the nominal height — the ink box is not the `Height`.

## Assumed — do not trust without checking

- **`type="Image"` as a layer type.** `CutSettingType.Image` serialises to it and a unit test
  asserts the string, but no probe has ever put an Image layer in front of LightBurn. The test
  asserts what the writer writes, which is not evidence about what LightBurn reads — exactly the
  trap the third rule above names. **Do not emit an Image layer until it has been read back in
  the UI.**

Every *other* element this writer emits has been seen read back in LightBurn's UI from a file
written the way this writer writes it.

Keep this section. The next element added to the writer belongs here until it has been seen in
the UI — an element that has only been *written* is an assumption, however plausible its name.

## Not known at all — needed for the geometry scope of OQ-7

Neither of these exists in the writer, and nothing here has been established. Both want the same
treatment as `QPulseWidth`: get a file LightBurn saved, then prove the elements read back from
the format this writer emits.

- **A bitmap shape.** How image data is carried (embedded base64, a path to the source file, or
  both), how the image's placement relates to its `XForm`, and which settings ride on the layer
  rather than the shape. Note that an embedded image ends the property that made
  `FormatVersion="1"` worth keeping — such a file can no longer be checked by eye.
- **Depth-map marking.** Which image mode modulates depth rather than dithering, and what bit
  depth survives the path from a 16-bit PNG or TIFF into the file. Grey level to micrometres is
  not a format question at all — it is a per-(machine, lens, material) calibration.

## Text is live text, and the minimum is what is already written

**This writer emits live text and never outlines** (OQ-12). The attribute set it writes —
`Font`, `H`, `LS`, `LnS`, `Bold`, `Italic`, `Ah`, `Av`, `Str`, plus the transform — is what
`probe/04-text-group` rendered correctly, so the minimum LightBurn needs is settled and met.
A bare family name (`Font="Arial"`) is enough; LightBurn's own Qt descriptor
(`Arial,-1,100,5,400,0,0,0,0,0`) is not required, and weight and slant have their own `Bold`
and `Italic` attributes.

`H` is a cap height, so a required character height is met by setting it.

**Fonts resolve on the host that runs LightBurn**, not the one that runs the agent — under WSL
those are two different font sets, and a name that does not resolve is substituted silently. The
answer is to use a font that is certainly there, not to build checking machinery: typeface,
weight and size are changed in seconds by whoever has the file open, and effort belongs in the
burn parameters instead. (R-G18, R-G19)

## What LightBurn writes versus what this writer emits

`probe/07-fill-line.lbrn` is a **file LightBurn itself saved** (AppVersion 2.1.04) and is the
most valuable reference here. LightBurn reads both forms, so none of these differences need
changing:

| LightBurn 2.1 writes | This writer emits |
|---|---|
| `FormatVersion="0"`, plus `DeviceName` / `AskForSendName` | `FormatVersion="1"`, no device |
| `<Thumbnail>` (base64 PNG), `<VariableText>`, `<UIPrefs>`, `<Notes>` | none of them |
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
