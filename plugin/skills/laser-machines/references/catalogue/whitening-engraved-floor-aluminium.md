---
id: whitening-engraved-floor-aluminium
work: etching
goal: white-marking
material_family: aluminium
applies_to: [fiber, mopa]
priority: 1
regulatory: false
---

## Acceptance criteria

This entry is the **second layer over a deep-engraved mark in aluminium**: the cut is made first,
then this pass lightly etches its floor so the mark reads as matte white rather than as bright
metal catching the light. It is the laser-only answer to contrast on aluminium, where annealing
to black is not available — the other answer is `ink-bonding-aluminium`, and both are legitimate.

- The floor of the cut reads matte white to light grey from every angle, with no specular glint
  left when the part is tilted. Tilting is the test; straight on, a bright floor and a white one
  can look alike.
- The walls of the cut are treated as well as the floor. The walls are what glints.
- **Depth is unchanged within the gauge's resolution.** Measured before and after this pass, a
  regulatory depth that was met must still be met, and must not have grown either — a whitening
  pass that cuts is an engraving pass.
- Nothing spreads outside the engraved geometry onto the anodised or bare surface around it.
- The contrast survives handling: rubbing with a cloth does not restore the shine.

## How to verify

Depth gauge before and after, same coupon and same spot. Then tilt the part under a single light
source and look for glint from the floor and from the walls separately. Then rub the mark with a
cloth and look again.

The pass runs over the **same geometry** as the cut, on its own layer with its own settings
(R-W3.8).
