---
name: laser-machines
description: >-
  Fiber, MOPA and CO2 laser engraving and marking - the machine and the material side.
  Register a machine and calibrate LightBurn for it; research, seed and calibrate burn
  recipes per material; choose the parameters for a job; diagnose a mark that came out
  wrong. Use whenever the subject is laser engraving equipment, burn parameters (power,
  speed, frequency, pulse duration, line interval, passes, defocus), a test burn, or a
  result that looks wrong - grey instead of black, burned through, nothing visible.
---

# Laser machines, recipes and calibration

TODO: one paragraph - what this skill owns (equipment, recipes, calibration, troubleshooting)
and what it does not (the `.lbrn` format and the generator, which are `laser-lightburn`).

## Safety comes before parameters

TODO: R-S1 to R-S5. Proactive, never on request. Forbidden materials, wavelength-specific
eyewear, the standing "test on a sample first" disclaimer.

## Provenance: a candidate must never read as a result

TODO: R-R7, R-R15. Web-sourced and computed numbers stay visibly distinguishable from a
burned result, at every point where they are shown.

## Test burns are mandatory before production

TODO: R-W2.5. Resist taking a number from the internet straight to a workpiece.

## Where the data lives

TODO: locating `laser-engraver/` beside the install, the project-level `.gitignore` check,
`config.md`. `design-composition.md` §2, `design-data-formats.md` §6.

## Routing by the state that is found

TODO: the two ladders. Machine: `registered` -> `configured` -> `calibrated`. Recipe:
`candidate` -> `calibrating` -> `verified`. A command branches on what it finds; it never
assumes the user arrived in order. Nothing may be assumed to still be in the conversation.

## Workflow 1 - onboarding and setup

TODO: R-W1.1 to R-W1.3. -> `references/onboarding.md`, `references/lightburn-setup.md`.

## Workflow 2 - building the recipe base

TODO: R-W2.1 to R-W2.4, R-R14. Coverage from the catalogue, research, seeding, calibration.
-> `references/recipe-base.md`, `references/calibration.md`.

## Workflow 3 - production, the material side

TODO: R-W3.1, R-W3.4. Choosing the recipe and the mode; handing the rest to
`laser-lightburn`. -> `references/jobs.md` in that skill.

## Workflow 4 - troubleshooting by symptom

TODO: R-W4.1. -> `references/troubleshooting.md`.

## Regulatory requirements

TODO: R-C1 to R-C3. Know that they exist, look them up, cache with a retrieval date.

## Handing work to laser-lightburn

TODO: what that skill needs and what it returns; the machine record is the channel, not the
conversation (R-G10).

## Reference files

TODO: the table - what each file is for and when to read it.
