---
id: blackening-engraved-floor-stainless-steel
work: annealing
goal: black-marking
material_family: stainless-steel
applies_to: [fiber, mopa]
priority: 1
regulatory: false
---

## Acceptance criteria

This entry is the **second layer over a deep-engraved mark**: the cut is made first, then this
pass blackens its floor and walls so no bright metal is left exposed. It exists because a
regulatory serial cut to depth is legible but glaring, and because bare freshly-cut stainless is
the least corrosion-resistant surface on the part.

- The floor of the cut reads black, not grey, viewed straight down into it.
- The walls of the cut are blackened too, not just the floor — the walls are what catches the
  light and makes an unblackened mark shine.
- **Depth is unchanged by this pass**: measured before and after, the difference is within the
  gauge's resolution. A blackening pass that cuts deeper is an engraving pass and this is the
  criterion that catches it.
- No blackening spreads outside the engraved geometry onto the surrounding surface.
- Legibility is improved, not reduced: the mark reads at arm's length after the pass.

## How to verify

Measure depth before and after on the same coupon and the same spot. Look into the cut straight
down and at an angle for the walls. Then check the surround for overspill in daylight.

The pass runs over the **same geometry** as the cut, on its own layer with its own settings, so
the file has to carry that geometry twice (R-W3.8). Registration is free when both layers come
from one generated file and is a real problem when they do not — which is why this is generated
rather than reproduced by hand.
