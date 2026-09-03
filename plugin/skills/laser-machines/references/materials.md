# Material vocabulary

The permitted tokens for `material_family`, `material_surface` and the known grades. Recipes and
catalogue entries must use these tokens; anything free-form belongs in the body (R-R3).

A controlled vocabulary is what makes two recipes comparable at all. `anodised` versus `raw`
aluminium is a larger difference than aluminium versus brass, and a base that spells the first
one three ways cannot tell the user that.

## Families

| Token | Covers | Known grades |
|---|---|---|
| `stainless-steel` | Austenitic and martensitic stainless | `304`, `316`, `17-4`, `410`, `420` |
| `aluminium` | Wrought aluminium alloys | `6061`, `7075`, `5052`, `2024` |
| `titanium` | Commercially pure and alloyed | `grade-2`, `grade-5` |
| `brass` | Copper-zinc alloys | `C260`, `C360` |
| `plastic` | Engineering thermoplastics | `abs`, `pc`, `pa`, `pom`, `pe`, `pp` |
| `stone` | Natural stone | `slate`, `granite`, `marble` |

`stone` covers what a user calls "rock" as well as slate; the difference between slate and
granite is a grade, not a family, because it changes the settings without changing the goal.

Steel that is not stainless is deliberately absent until there is a machine to test it on.

## Surfaces

| Token | Meaning |
|---|---|
| `raw` | As supplied, no deliberate finish |
| `mill` | Mill finish, as rolled |
| `brushed` | Directional brushed finish |
| `polished` | Mirror or near-mirror |
| `anodised` | Anodised aluminium, dyed or clear |
| `painted` | Painted or powder-coated |
| `riven` | Natural cleft face, slate |

`anodised` is a surface, not a family: the substrate is still aluminium, but the layer the laser
interacts with is the anodic coating, and every setting differs because of it.

## Grades

A grade is optional and free-form when it is not in the table above — but if the table has a
token for it, that token must be used. An unknown grade is recorded as `unknown` rather than
guessed, because the alloy changes the result more than most users expect.
