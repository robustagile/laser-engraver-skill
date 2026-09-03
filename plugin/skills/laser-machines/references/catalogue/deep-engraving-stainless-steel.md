---
id: deep-engraving-stainless-steel
work: deep-engraving
goal: regulatory-marking
material_family: stainless-steel
applies_to: [fiber, mopa]
priority: 1
regulatory: true
---

## Acceptance criteria

- Depth **at least 0.0762 mm (0.003 in)** at the *shallowest* point of the *shallowest*
  character, not averaged over the mark.
- Character height **at least 1.5875 mm (1/16 in)**, measured on the part rather than trusted
  from the design.
- Legible without magnification, and still legible after the part is finished, blasted or
  passivated.
- Floor of the cut is even; no dished centre and no re-deposited slag left after cleaning.
- **No bare bright metal left exposed.** A cut to depth in stainless is legible but glaring, and
  a freshly cut floor is the least corrosion-resistant surface on the part. The job therefore
  carries a **second layer that blackens the cut** over the same geometry —
  `blackening-engraved-floor-stainless-steel` — and the depth criterion above is measured again
  after that pass, unchanged.

The two figures above are the US minimums for firearm marking. **They must be confirmed against
the regulatory cache, and re-looked-up if that finding is more than 12 months old, before any
compliance job** (R-C2). They are recorded here as the criterion, not as the citation.

## How to verify

Measure depth with a depth gauge or a depth micrometer on the actual part where the part allows
it, otherwise on a coupon burned in the same session with the same settings and the same focus.
Measure character height with calipers on the part. Both measurements are mandatory: this is the
one criterion where a visual judgement is worth nothing.
