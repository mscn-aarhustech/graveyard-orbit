# Student Guide

## First hour: understand before editing

Build, test, and launch the game. Try coasting without thrust, firing while moving, reverse thrust, a collision, restart, mute, and window resizing. Record one surprise and identify where its behavior is defined.

Read Program.cs, GameSession.cs, PhysicsWorld.cs, then Gravity.cs. Trace a shot from Space to its sprite and sound. Draw a small dependency diagram and explain why the custom physics library does not update entity positions directly. Locate the five manifest entries and describe what happens if one source file is renamed.

Inspect the sample tests. Which use fakes, which execute Box2D, and which invoke native raylib? Explain what a passing texture-cache test does not prove about on-screen graphics.

## A few days: one team, small changes

Follow CONTRIBUTING.md to create a shared team fork and configure upstream. Pick one small issue from BACKLOG.md. Write acceptance criteria and identify likely files before creating a branch. Deliver the smallest end-to-end change, then ask a peer to review it.

Your pull request should contain test evidence, a screenshot for visual changes, and a short explanation of the design. Review someone else's pull request before merging your own. Use the actual GitHub conversation rather than agreeing privately and clicking Approve without evidence.

For a conflict exercise, two teammates change the same HUD text on separate branches. Merge one, update the other branch, and resolve the conflict together. Explain why the chosen result preserves both intended changes.

## A few weeks: deepen the feature

Extend the first tracer bullet with fuel, prediction, asteroid fragments, or additional levels. Add tests around the new rules rather than aiming for a coverage percentage. Revisit the architecture only when your feature exposes a concrete need.

Investigate an unfamiliar subsystem. Submit a concise bug report with reproducible steps, a hypothesis, evidence, a fix, and a regression check. A surprising mechanic is not automatically a bug: compare it with the documented rules first.

## Reflection

Explain a code path you did not write. Show a useful review comment you gave or received. Describe a real merge or conflict. Identify one assumption your tests caught and one behavior that still needs manual verification. If AI helped, explain how you verified its suggestions.
