# Calibration

Workflow 1 step 3. It ends with `state: calibrated`, and it is the step that turns a machine
LightBurn can talk to into one whose output can be trusted: **a commanded size comes out at that
size, and the red-dot frame agrees with the mark.**

Everything here is drawn by hand in LightBurn - a rectangle, a few lines, a grid of crosses. No
generated file is needed, which is why this workflow is not waiting on the generator.

Every outcome is written into the machine record's `## Calibration` section with the date
(R-M8). This is not bookkeeping: a scale correction that lives only in LightBurn's device
settings is silently lost the moment the profile is recreated, and then every job comes out the
wrong size. It happened on the previous machine - one lens needed a 0.8 correction, without
which everything was 25 % oversize, and nothing anywhere else knew.

## Scale: commanded size versus measured size

1. Draw a rectangle of a known size - 50 x 50 mm is convenient - on cheap material of the kind
   the machine will actually be used on.
2. Mark it at settings that leave a readable edge and remove as little as possible.
3. Measure both axes with calipers, across the mark's centres rather than its outer fuzz.
4. Correction per axis is `commanded / measured`. Apply it where the machine offers it (a galvo's
   correction or scale setting, a DSP's steps per mm).
5. **Re-burn and re-measure.** A correction that was not confirmed is a hypothesis.

Record: date, commanded size, measured size per axis, the correction applied, and where it was
applied. If X and Y differ by more than a per cent or so, say so in `## Notes` - that is a
scan-head or mechanical asymmetry, not something to average away.

## Distortion: the edges, not the centre

A field that is right in the middle and wrong at the edges is the normal failure of a galvo
without the correct correction file, and of a gantry with a slack belt.

Draw a grid of small crosses across the whole field - the four corners and the centre at
minimum. Mark it, then measure the distance between the outermost crosses in each direction and
compare against the commanded distance. Judge the corners by eye too: a cross that is no longer
square at the corner of the field is distortion, not a scaling error, and scaling will not fix
it.

If it is bad, the answer is the vendor's correction file for that lens (`lightburn-setup.md`),
not a number in the machine record.

## The red dot: does the frame tell the truth?

On a fiber or MOPA the red pointer is a separate beam, and its agreement with the actual mark is
a setting, not a given. A frame that lies is worse than no frame - work gets positioned
confidently and wrongly.

1. Frame a rectangle, and mark where the red dot says the corners are - a pencil on masking tape
   is enough.
2. Mark the same rectangle.
3. Measure the offset between the two in X and Y, and check the size as well as the position:
   the pointer can be offset, scaled, or both.
4. Correct it where the machine offers it, then repeat until it agrees.

Record the offsets and the date. A red-dot alignment drifts after a knock, so a dated entry is
what lets a later session ask whether it is still true.

## Focus: found by burning, not by the spacer

The vendor's spacer gets close. The true focus height is where the mark is narrowest and
brightest, and finding it takes one burn:

- Draw a row of short lines. Mark each at a different Z height, in steps of 0.5 mm through the
  expected focus and 2 mm either side of it, and label each line with its height.
- The finest, most defined line is focus. If two adjacent lines look equal, focus is between
  them; if the whole row looks the same, the step was too small or the material is too forgiving
  - use a step of 1 mm and a material that shows the difference.

Record the working distance, how it was measured, and the material it was measured on. On a
galvo, also record the spot width of the finest line if it can be measured - that is the real
spot size, and it replaces the estimate from registration (`onboarding.md`).

## What must be true before calling it calibrated

- A commanded size was measured on material and matches, per axis, after correction.
- The field's corners were checked, not just its centre.
- The red-dot frame agrees with the mark in position and size, or the machine has no red dot.
- Focus was established by burning and the working distance is recorded.
- Every one of the above is in `## Calibration` with its date.

Then set `state: calibrated`, and say what that does and does not mean: the machine now marks
where and at the size it is told, and **nothing yet is known about what settings any material
wants**. That is workflow 2.
