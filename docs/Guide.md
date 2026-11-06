# Beginner Guide

## The three projects

The solution contains a game, a small physics library, and a few example xUnit tests. Dependencies and versions are written in each project file, rather than hidden in shared configuration. There is no website, GitHub automation, editor-specific setup, or framework to learn first.

| File | What to read it for |
| --- | --- |
| Game/Program.cs | Start here: the window, controls, loop, fullscreen, and cleanup |
| Game/Game.cs | Level creation, gravity, firing, collision rules, score |
| Game/Entity.cs | An object whose physical state belongs to Box2D |
| Game/PhysicsWorld.cs | Box2D bodies, contact/hit events, and arena walls |
| Game/Camera.cs | Follow the ship and convert metres into screen coordinates |
| Game/Renderer.cs | Sprites, the HUD, and DrawPlanet |
| Game/Assets.cs | Find, cache, and dispose textures/sounds |
| SpacePhysics/SpaceMath.cs | Gravity, thrust, and initial orbit velocities |
| Tests/SampleTests.cs | A few useful tests, not a complete test suite |

The paths above abbreviate src/GraveyardOrbit.Game, src/GraveyardOrbit.SpacePhysics, and tests/GraveyardOrbit.Tests.

## Follow one tracer bullet

Press Space. Program reads Controls; Game creates a bullet; SpaceMath calculates gravity; PhysicsWorld steps Box2D; Game reads collisions and changes score; Renderer draws the result and Assets plays the sound. This narrow, working end-to-end path is the tracer bullet. Extend it with one small feature before creating a big framework.

## Physics you can inspect

Coordinates use metres and seconds, with +X right and +Y down. Mass comes from Box2D. The planet is 1000 mass units, ship 1, asteroids 3 or 4, and bullets 0.01. Every game body is dynamic. Each pair gets a Newtonian gravity force, applied equally and oppositely. There is no entity-kind or body-type filter in gravity. The gameplay gravity constant is 0.035; a small minimum-distance clamp prevents a singularity if centres overlap.

CreateCircularOrbit sets both bodies' velocities, preserving their pair momentum. Each new orbit preserves the momentum already assigned to the other bodies. The whole initial system starts with zero centre-of-mass velocity; only floating-point roundoff is corrected once. Orbits are initial conditions, not fixed circular paths. All bodies subsequently affect one another.

We do not force the mass centre's velocity back to zero during play. Thrust and contact with the externally fixed walls can change total momentum. Firing includes equal-and-opposite muzzle recoil, but this stub creates ammunition without deducting its mass from the ship. Destruction removes mass too. Fuel/ammunition accounting is a useful later exercise.

Box2D handles friction (0.7), low restitution (0.05), rotation, contacts, and continuous collision detection. Slow objects can settle together; circular objects may also roll. A normal impact speed of at least 3 m/s destroys asteroids and the ship, not the planet. This uses Box2D's hit-event speed, not absolute travel speed. Gentle nudges can build a collision chain without firing. Bullets always destroy asteroids they hit and expire after four seconds.

The four walls bound a fixed 100 x 64 metre rectangle. They are static external constraints, not massive gravitating game entities. Crossing a radius no longer kills the ship. A hard wall impact can still destroy it under the same collision rule as other impacts. After ship destruction the simulation freezes until restart.

## Camera and fullscreen

The camera follows the ship, not the planet. Every metre always covers 16 logical pixels; every logical pixel covers 2 window pixels. At 1280 x 720 the logical viewport is 640 x 360. At 1920 x 1080 it is 960 x 540, revealing more world at the same scale. Resizing/F11 recreates the render target and repositions the HUD without resetting any physical state. Returning from fullscreen restores the previous window size/position. A fullscreen display no larger than the window cannot reveal extra area.

## Assets and platforms

Do not change root assets unless an exercise explicitly calls for it. The packs, manifest, README and notices are preserved from dev. Only selected runtime files are copied by the game project. Adding a sprite needs a manifest entry and a project Content entry. The legacy notice in assets refers to the former root THIRD_PARTY_LICENSES directory; the unchanged license texts now live in docs/licenses, and are copied there beside published builds.

On Ubuntu 24.04, install libgl1, libx11-6, libxcursor1, libxi6, libxrandr2, libxinerama1 and libasound2t64. Ubuntu 22.04 uses libasound2 instead. Use an actual graphical desktop and OpenGL-capable driver. No audio device should produce a silent game rather than stop play. Raylib-cs packages native Windows x64/x86, Linux x64, and macOS Intel/Apple Silicon binaries; Linux ARM64 is not supported out of the box here.

## Your first team change

Use a shared team fork. Clone it, create a feature branch, make a small change, run tests, push the branch, and ask a teammate to review a pull request. The teacher's current development branch is dev; use it as the PR base when contributing back.

```bash
git switch dev
git pull --ff-only
git switch -c feature/my-small-change
dotnet test GraveyardOrbit.sln
git add .
git commit -m "Describe the change"
git push -u origin feature/my-small-change
```

Pick one task: change a control and update its instructions; add an on-screen speed indicator; add pause; expose gravity or impact speed as a setting; add a fuel gauge; improve an error message. Explain which files changed and demonstrate the result. For a conflict exercise, two teammates edit the same HUD text on separate branches, then resolve the conflict together.

AI assistance is allowed. You must understand the submitted code and verify its behavior. Discuss licensing separately: this project currently uses standard MIT, not a no-training restriction.

## Test what matters

SampleTests demonstrates pure maths, asset resolution, native decoding, rigid-body integration, walls, stacking, collision-driven destruction, and viewport changes. Box2D tests run sequentially. Add one focused test or manual check for your own feature rather than trying to cover every line.

```bash
dotnet test GraveyardOrbit.sln --filter "Category!=Native"
dotnet test GraveyardOrbit.sln --collect:"XPlat Code Coverage"
dotnet run --project src/GraveyardOrbit.Game -- --smoke-test
```

The smoke test renders 180 frames, fires, enters/exits fullscreen, and saves smoke.png. It needs a display. It is not a substitute for trying a real keyboard, mouse and speakers. To deliberately update a dependency, change its version in the project file, run `dotnet restore GraveyardOrbit.sln --force-evaluate`, review the changed lock files, and rerun tests.
