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

Everything the owner has — machine records, recipes, regulatory findings, their settings — is in
one directory, and **its location is derived, never searched for**. This skill is installed at
`<claude>/skills/laser-machines/`, so the store is:

```
<claude>/laser-engraver/
  config.md                          output directory, LightBurn version, default machine
  machines/<id>.md                   one per machine
  recipes/<machine>/<lens>/<id>.md   one per line of inquiry, with its own burn history
  regulatory/                        cached findings, each with the date it was retrieved
```

Resolve `<claude>` once, from the path this file was loaded from, and use it for the rest of the
session.

**Do not go looking for it.** A `find` for `laser-engraver`, a sweep of the home directory, a
look into the plugin's own repository — all wrong, and the last one is forbidden outright: if
this skill directory is a symlink into a development clone, the clone is still not somewhere to
read from at run time (R-N4). Listing `<claude>/laser-engraver/machines/` is the whole of
discovery.

**Its absence is an answer, not a problem to investigate.** No `laser-engraver/` means no
machine has ever been registered here; an empty `machines/` means the same. Say so, offer
`/laser-machine`, and make it explicit that anything said before a machine record exists rests
on what the owner tells you in this conversation and on nothing recorded. Neither absence is a
reason to search somewhere else.

**Nothing creates it but a real write.** The installer never touches it (R-N7). This skill
creates it at the moment it first has something of the owner's to keep — not in advance, and not
to test whether it could.

A project-level `.claude` is commonly committed to git, so before the first write check that the
store is ignored, and give the `.gitignore` line if it is not (R-N9).

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
