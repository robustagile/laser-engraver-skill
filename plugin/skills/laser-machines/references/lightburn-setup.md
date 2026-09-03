# Configuring LightBurn for the machine

Workflow 1 step 2, the software half (R-W1.2). It ends with `state: configured`, which means
exactly two things: **LightBurn talks to the machine, and its device profile describes the right
field for the lens in use.** Not that anything has been proven to mark true - that is
`calibration.md`.

LightBurn v2 is the only target (R-E1). Its exact version, patch included, is recorded in
`config.md`, because the `.lbrn` format and the UI both change between them.

> **The UI specifics below are unverified.** They were written from knowledge of LightBurn, not
> from a copy open beside the machine. On the first real setup, check each one against the
> owner's LightBurn and **correct this file** - the owner with the software open is the
> authority, and a wrong menu path wastes their time and teaches them to distrust the rest.

## Before the wizard: does the licence cover this device class?

LightBurn's licence tiers do not all cover all machines, and a key bought for one class may not
open another. Establish this before spending an hour on a profile that cannot be created - the
owner's account page and the device wizard between them will say.

This is a fact about a vendor's commercial policy, so do not assert what it costs or what covers
what: have the owner look, and record what they find in `## Notes`.

## Creating the device

Ask which it is first, because the two paths share almost nothing.

**Fiber or MOPA (galvo).** The device is a galvo type, and the thing that matters most is the
**field correction file** the machine vendor supplies - a `.cor` (or the vendor's equivalent)
generated for that scan head and that lens. Without it the field is geometrically wrong at the
edges no matter how well the rest is set up, and no amount of calibration in the machine record
compensates for its absence. Ask for it by name; it is usually on the USB stick that came with
the machine, next to the EZCAD installation.

One correction file per lens. Switching the lens without switching the file is a common and
confusing failure: the centre looks right and the edges do not.

**CO2 (gantry, DSP).** The device is the controller, not the machine - Ruida, Trocen or
TopWisdom - and the connection is USB or Ethernet. LightBurn can usually find it. If the
controller was never identified during registration, go back to `onboarding.md`: guessing a
controller and connecting to it is how settings get written to the wrong device.

## The working field

Set the field to the **lens in use**, from the machine record - and if the record has more than
one lens, ask which is mounted rather than assuming the first.

Then verify it rather than trusting it: send a frame at the full field and watch where the head
or the red dot actually goes. A field set larger than the machine's is the failure that ruins
work silently, because everything inside the smaller area still looks correct.

## Origin and orientation

The two settings that make a file that opens and marks in the wrong place (R-W3.3): where the
origin is, and which way Y increases.

Establish them by test, not by reasoning: place a small mark deliberately off-centre - near one
named corner - burn or frame it, and see which physical corner it appears in. Nothing else
settles it, because a mirrored Y and a rotated workpiece look identical for a symmetrical shape.

Record `origin` and `axis_y` in the machine record. They are the two fields most often lost when
a device profile is recreated, and the whole job comes out mirrored when they are.

## Focus

Set focus the way the machine offers it - a motorised Z, the vendor's spacer, or a manual column
- and note the working distance. This is the coarse setting only; the true focus height is found
by burning in `calibration.md`, and on a galvo it is where a surprising amount of mark quality
lives.

## Air assist and extraction

Not optional on metals (see the safety section in `SKILL.md`), and worth checking now while the
machine is being set up rather than in the middle of a job. Extraction that exists but is not
switched on is the normal state of a new bench.

## What must be true before calling it configured

- LightBurn is connected and the machine responds to a frame command.
- The device's field matches the lens the record says is mounted, and framing the full field
  agrees with the physical field.
- On a galvo, the vendor's correction file for that lens is loaded.
- `origin` and `axis_y` are established by test and written into the record.
- The LightBurn version is in `config.md`.

Then set `state: configured` and say what has *not* been established yet: that the machine marks
at the size it was told, and that the red dot tells the truth. Those are next.
