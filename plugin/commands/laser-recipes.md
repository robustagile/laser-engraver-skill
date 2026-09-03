---
description: Research, seed and calibrate burn recipes for a machine and lens
argument-hint: [material or catalogue entry, or nothing for the coverage view]
---

Workflow 2: from "no recipes exist for this machine" towards a base of verified ones.

Use the `laser-machines` skill, and `references/recipe-base.md` for the method. The catalogue in
`references/catalogue/` is the coverage map.

Branch on what the store holds:

- **No machine registered** - this workflow has nothing to attach a recipe to. Say so and point
  at `/laser-machine`.
- **A machine not yet `calibrated`** - research and seeding are still worth doing now, and say
  why the rest waits: a candidate cannot be calibrated on a machine whose commanded size is not
  yet its real size.
- **`$ARGUMENTS` names a material or a catalogue entry** - work that line of inquiry.
- **No argument** - show coverage for the machine and its mounted lens, ordered by priority, with
  each recipe's state and provenance, and propose what to do next.

Which lens is mounted matters: a recipe is valid for the (machine, lens) pair, so ask when the
record lists more than one.

Every number that has not been burned on this machine is a candidate, and says so wherever it
appears. Research and seeding produce candidates only - the promotion to verified needs a test
card and a confirmation burn, and the card generator does not exist yet, so say that plainly
rather than implying a recipe can be finished today.
