---
name: laser-lightburn
description: >-
  Write LightBurn `.lbrn` project files, and deliver cut settings as text for a job the user
  drew themselves. Use when a laser test card or job file has to be generated, when a question
  is about the `.lbrn` format or what LightBurn does with an element, or when layer parameters
  must be handed over for typing into LightBurn's cut settings. A service to `laser-machines`:
  it holds no opinion about materials and runs no dialogue about what to engrave.
---

# LightBurn file generation

TODO: one paragraph - the mechanics half. Given a machine record and a card or job
specification, produce a `.lbrn`. LightBurn v2 is the only target (R-E1).

## Never emit an element that has not been read back in LightBurn's UI

TODO: the one rule that must never be missed. LightBurn silently normalises what it does not
recognise - an invented layer type was loaded, rewritten as a plain one, and its contour
dropped, with no error. The `doOutput` case (R-G15) is why this is a hard rule: guess it
wrong and a layer meant as a guide fires the laser. Verified versus assumed is
`references/lbrn-format.md`; nothing from the assumed column may be emitted.

## "It opens" is not verification

TODO: nor is a round-trip diff - LightBurn v2 rewrites the file on save whether or not it
understood an element. Verification is visual or measured in the UI. An inferred constant
must not be asserted by a test built on the same inference.

## Prerequisite: the .NET SDK

TODO: R-N8 - checked here on first use, not by the installer, because a .NET program cannot
check for .NET. Platform-specific guidance when it is missing.

## Where files are written

TODO: R-G4 and OQ-9 - the output directory is a `config.md` setting pointing at a path
LightBurn can open, which on this setup means Windows-visible while the agent runs in WSL.
Generated files are write-only and disposable: re-run the generator, never re-read a saved
file (R-G3).

## What the writer can emit today

TODO: shapes, layers and sub-layers, live text, transforms, arrays. And what it cannot:
no bitmap shape, no verified `Image` layer, so nothing raster (R-G16), which blocks the
`photo-marking-*` catalogue entries and depth maps (R-G17).

## Building and running the generator

TODO: the projects under `tools/`, how they are built on first use, and how they are invoked.

## The job specification

TODO: the JSON handed to the generator. `design-data-formats.md` §7.

## Reading the machine record

TODO: R-G10 - field size, spot, frequency ceiling come from the record, not from the
conversation.

## Test cards

TODO: -> `references/test-cards.md`.

## Jobs

TODO: -> `references/jobs.md`. Two modes, neither a fallback of the other (R-W3.4).

## Mode 2 - parameters as text, not a file

TODO: R-W3.5 to R-W3.7. Fields named and united as LightBurn's UI names them; every value
the user must type is present, including one deliberately left at a default; provenance and
acceptance criteria travel with the numbers. A geometry-free `.lbrn` is rejected outright.
`.lbset` is v2 - deferred, not rejected, so nothing here may foreclose it.

## Reference files

TODO: the table - what each file is for and when to read it.
