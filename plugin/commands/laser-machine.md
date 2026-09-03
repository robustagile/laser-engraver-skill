---
description: Register a laser machine, configure LightBurn for it, and calibrate it
argument-hint: [machine name, or nothing to pick up where onboarding stopped]
---

Workflow 1: get a machine from "I have it" to "it marks where and at the size it is told".

Use the `laser-machines` skill. It says where the data store is - derive that path, do not search
for it - and it carries the safety and provenance rules that apply throughout.

Then branch on what is actually in the store, not on what the argument suggests:

- **No store, or no machines** - nothing has ever been registered. Start registration
  (`references/onboarding.md`).
- **A machine named in `$ARGUMENTS`** - work on that one. If no such record exists, say so and
  offer to register it rather than guessing which existing record was meant.
- **One machine, no argument** - continue it from its `state` and its `## Unknown` section.
- **Several machines, no argument** - list them with their states and ask which.

From there the state decides the step: `registered` -> LightBurn setup, `configured` ->
calibration, `calibrated` -> there is nothing left in this workflow, so say what the machine is
ready for and point at `/laser-recipes`.

Read `## Unknown` before asking anything. Re-asking for facts already in the record is the
fastest way to lose the owner's trust in the tool.

Write the record before the conversation ends, even when half of it is still unknown, and say
plainly what is in it and what is not.
