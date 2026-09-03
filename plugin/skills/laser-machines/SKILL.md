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

This skill owns the physical side: what the owner has, what LightBurn must be told about it,
which recipes that machine needs, and what a bad mark means. Its knowledge is per-machine and
can only be settled by burning something.

It does not own the `.lbrn` format or the generator. Those are `laser-lightburn`, which is a
service: this skill decides *what* to burn and *why*, that one decides how the file has to be
written so LightBurn does what was intended.

The owner has basic experience - they can run the machine and the software. They are stuck on
application, not on operation. Do not explain what a layer is; do explain why 40 kHz gives a
different result from 200 kHz.

## Safety comes before parameters

Warn **before** the answer, unprompted, at the moment it becomes relevant - not on request and
not as a footer (R-S1, R-S5). Say it once, where it applies. Repeating it every message trains
the owner to skip it.

Three things are worth interrupting for:

- **Materials that must not be lasered at all**: PVC and vinyl (hydrogen chloride - ruins the
  machine and the lungs), PTFE, polycarbonate, ABS, galvanised steel and high-zinc brass (zinc
  oxide fume), carbon fibre composite. If a job names one of these, say so before discussing
  settings, and offer what can be marked instead.
- **Toxic products from metals that are perfectly normal to mark**: stainless steel produces
  hexavalent chromium and nickel oxide aerosol; chrome, cadmium and beryllium alloys are worse.
  These need extraction, not an open window - and this is the case that gets missed, because the
  material itself is unremarkable.
- **Eyewear matched to the wavelength.** 1064 nm, 10.6 µm and 450 nm are not interchangeable and
  glasses for one are no protection against another (R-S3). The wavelength is in the machine
  record; if it is unknown, that is a reason to establish it before the first burn, not after.

And one standing rule that applies to every recipe this skill ever hands over: **the parameters
are a starting point, and a test on a sample of the actual material is always required**
(R-S4). Not a disclaimer to recite - a thing to actually make happen.

## Provenance: a candidate must never read as a result

At the moment a machine is registered there is **no verified data at all**. Every number that
exists then came off the internet or out of a calculation, and it stays visibly that until a
burn says otherwise (R-R7).

Five provenances, and the word travels with the numbers wherever they are shown:

| `provenance` | Means | May it go on a workpiece? |
|---|---|---|
| `vendor-default` | From the machine or source vendor's own table | No. Test card first. |
| `internet` | Found in a forum, a video, a supplier's page | No. Test card first. |
| `computed` | Reasoned out here from the physics and the machine's ranges | No. Test card first. |
| `user-tested` | Burned on this machine, result recorded, criteria not fully met | No. |
| `user-verified` | Burned on this machine and met its acceptance criteria (R-R14 step 5) | Yes. |

Three rules that make this real rather than decorative:

- **Never state a parameter without its provenance in the same breath.** Not in a later
  sentence, not in a footnote - in the same table or line as the number.
- **When the owner asks "what settings" and only candidates exist, lead with that**, then give
  the candidate. The answer to a question is not permission to answer it as though it were
  settled.
- `internet` provenance is incomplete without **what it was written for** - which machine, what
  power, which lens (R-R16). The distance between that machine and this one is what says whether
  the candidate is usable at all, and a candidate without it is worth very little.

## Test burns are mandatory before production

Not optional, and not waived by the owner being in a hurry (R-W2.5). The instinct this skill
exists to resist is taking a number from the internet straight to a workpiece.

Being asked for a number is not authorisation to skip the card. If there is no verified recipe
for the material in hand, say so, give the candidate with its provenance, and say what to burn
it on first.

## Where the data lives

Everything the owner has - machine records, recipes, regulatory findings, their settings - is in
one directory, and **its location is derived, never searched for**. This skill is installed at
`<install>/skills/laser-machines/`, where `<install>` is a `.claude` directory. The store is that
directory's **sibling**, one level up:

```
<install>/../laser-skill-data/
  config.md                          output directory, LightBurn version, default machine
  machines/<id>.md                   one per machine
  recipes/<machine>/<lens>/<id>.md   one per line of inquiry, with its own burn history
  regulatory/                        cached findings, each with the date it was retrieved
```

So an install at `~/.claude` puts it at `~/laser-skill-data/`, and one at `<project>/.claude`
puts it at `<project>/laser-skill-data/`. Resolve it once, from the path this file was loaded
from, and use it for the rest of the session.

It is deliberately **outside** `.claude/`: Claude Code guards writes inside a `.claude`
directory, a permission rule in settings does not lift that guard, and a store in there could
not be written at all without an interactive approval each session. Measured, not assumed.

**Do not go looking for it.** A `find` for `laser-skill-data`, a sweep of the home directory, a
look into the plugin's own repository - all wrong, and the last one is forbidden outright: if
this skill directory is a symlink into a development clone, the clone is still not somewhere to
read from at run time (R-N4). Listing `<store>/machines/` is the whole of discovery.

**Its absence is an answer, not a problem to investigate.** No `laser-skill-data/` means no
machine has ever been registered here; an empty `machines/` means the same. Say so, offer
`/laser-machine`, and make it explicit that anything said before a machine record exists rests
on what the owner tells you in this conversation and on nothing recorded. Neither absence is a
reason to search somewhere else.

**Nothing creates it but a real write.** The installer never touches it (R-N7). This skill
creates it at the moment it first has something of the owner's to keep - not in advance, and not
to test whether it could.

A project-level `.claude` is commonly committed to git, so before the first write check that the
store is ignored, and give the `.gitignore` line if it is not (R-N9).

## Routing by the state that is found

Nothing may be assumed to still be in the conversation: onboarding happens one day, calibration
over the following weeks, a job months later. Read the store, branch on what is there, and never
assume the owner arrived in the right order.

| What is found | What is next |
|---|---|
| No store, or no `machines/` | Nothing is registered. Registration - `references/onboarding.md` |
| `state: registered` | LightBurn does not know the machine yet - `references/lightburn-setup.md` |
| `state: configured` | It talks but has not been proven true - `references/calibration.md` |
| `state: calibrated` | Recipes - `references/recipe-base.md` |
| A record with a `## Unknown` section that is not empty | Whatever is listed there may block the step being asked for. Check it before starting. |

**The state advances on evidence, not on intent.** `configured` means LightBurn talks to the
machine and a device profile exists. `calibrated` means a commanded size was measured on
material and matched, and the red-dot frame agreed with the mark. The owner saying a machine is
calibrated is not the evidence; a measurement written into `## Calibration` is.

## Workflow 1 - onboarding and setup

Three steps, and the state after each one is the point of them (R-W1.1 to R-W1.3):

1. **Register** - what the owner has, what they know, what has to be researched.
   `references/onboarding.md`. Ends at `registered`.
2. **Configure LightBurn** - a device profile that talks to the machine, with the right field
   for the lens in use. `references/lightburn-setup.md`. Ends at `configured`.
3. **Calibrate** - commanded size equals measured size, the red dot tells the truth, focus is
   established, and all of it is written down. `references/calibration.md`. Ends at
   `calibrated`.

For a machine of unknown origin, step 2 starts further back: identify the controller and
establish whether LightBurn can drive it at all. That it cannot is a legitimate outcome (R-M11)
and finding it out early is worth more than any recipe.

None of the three needs the generator. Calibration geometry is a rectangle and a few lines -
drawn by hand in LightBurn, which is why this workflow is not waiting on anything.

## Workflow 2 - building the recipe base

`references/recipe-base.md`, and the catalogue in `references/catalogue/` is the coverage map:
entries whose `applies_to` includes this machine's type, minus the recipes that exist for it, is
the work list (R-R13).

Research and seeding can be done with no machine present - that is the half that produces
candidates. Calibration needs the machine on the bench, and until it happens every number is a
candidate no matter how good the source was.

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

| File | When to read it |
|---|---|
| `references/onboarding.md` | Registering a machine, or picking up a half-finished registration |
| `references/lightburn-setup.md` | Getting LightBurn to talk to it, and to know its field |
| `references/calibration.md` | Proving the machine marks where and at the size it was told |
| `references/recipe-base.md` | Coverage, researching candidates, seeding, calibrating recipes |
| `references/troubleshooting.md` | A mark came out wrong and the symptom is the way in |
| `references/materials.md` | The permitted `material_family` / `material_surface` tokens |
| `references/catalogue/` | Which recipes a machine of this class needs, and their criteria |
