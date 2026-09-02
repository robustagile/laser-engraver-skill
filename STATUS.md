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
- **Repository conventions** — `CLAUDE.md`, `.gitattributes`, `.gitignore`.
- **Human-facing docs** — `README.md`, `INSTALL.md`, this file.

**No code exists.** There is no `plugin/` directory, nothing is installable, and nothing has
been generated or burned.

## Open problems

- **The inherited material has not been imported** (R-N4). The `.lbrn` writer, the probe files,
  the reference file LightBurn itself saved, the test cards and the golden tests all still live
  only in the prior experimental directory. Everything downstream waits on this.
- **Two format elements are unverified** (R-G15), and one of them is a safety matter:
  - `doOutput` — the element marking a layer as non-firing. Guess the name wrong and a layer
    meant as a guide fires the laser.
  - `frequency`, and for MOPA the pulse-duration element — names unknown, and the writer emits
    neither. These are the two most important levers a MOPA has.

  Both can be settled with LightBurn alone, no laser needed, in two steps: read the names out of
  a file LightBurn wrote, then confirm they are honoured in a file written the way the plugin
  writes them.
- **OQ-7** — how far geometry generation goes in v1: layer settings only, layers plus geometry,
  or variable data as well.
- **OQ-8** — whether to keep emitting the older `.lbrn` format version or match what LightBurn
  v2 writes natively.
- **Neither verification machine is on hand.** The MOPA 60 W and the CO2 are both in transit and
  the arrival order is unknown, so nothing may be planned that depends on which lands first
  (R-M10).

## Where to start the next session

1. **Import the inherited material into `plugin/`** (R-N4) — writer, shapes, path encoding,
   validation, the probe files including the one LightBurn saved, the card generators and the
   golden tests. Nothing else can proceed without it, and it is mechanical work.
2. **Settle R-G15, step one:** ask the owner to create a galvo device profile in LightBurn, set
   frequency and pulse duration on a layer, save a `.lbrn`, and hand it over. Read the element
   names out of it.
3. **Settle R-G15, step two:** emit those elements in the format the plugin writes, and have the
   owner confirm LightBurn shows the values in its UI. Step one alone proves nothing about the
   format version the plugin emits.
