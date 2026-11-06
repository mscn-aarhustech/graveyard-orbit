# Validation

Checked on **November 6, 2026**, in Linux x64 with .NET SDK 10.0.403, Raylib-cs 8.0.0 (native raylib 6.0), and Box2D.NET 3.1.654.

| Check | Result |
| --- | --- |
| Locked restore and Release build | Passed, zero warnings/errors |
| Sample xUnit tests | 13 passed, none failed or skipped |
| Coverage collector | Produced a Cobertura report; no coverage target |
| Initial orbit momentum | Pair momentum preserved; initial whole-system mass-centre velocity zero; coasting remains within floating-point tolerance |
| Gravity | Equal-and-opposite inverse-square forces; a bullet causes measurable planet response |
| Contact rules | Gentle contact survives; hard asteroid collisions clear rocks without a shot |
| Rigid bodies | Slow bodies stack; a fast body cannot cross the outer wall |
| Camera | Ship stays centred; larger viewports reveal more metres at unchanged zoom |
| Native raylib | Real Kenney PNG decoded |
| Root assets | No changes from the dev base commit |

A native window ran under Xvfb at 1920 x 1080 with Mesa software rendering and an ALSA null sink. Both OGG sounds decoded, sprites were loaded/cached, 180 frames rendered, a shot was generated, and all resources shut down cleanly. Fullscreen changed the logical width from 640 to 960 and back to 640 without resetting the game.

```text
SMOKE frames=180 shot=True audio=True fullscreenChanges=2 logicalWidth=640->960->640
```

A separate input check sent synthetic keyboard/mouse events through the actual window: F11 entered 1920 x 1080 and restored 1280 x 720; resizing, thrust/turn/fire, restart, and mute inputs caused no runtime errors; Escape exited cleanly. This checks the native input path, not physical keyboard hardware or a human's gameplay experience.

A Linux framework-dependent publish was launched from outside the repository and passed the same fullscreen smoke test. Build output contains only selected runtime assets and the original license texts. No tool scripts, build products, or screenshots are committed.

Windows/macOS execution and audible speaker output were not verified in this environment. Test these on classroom machines. Gameplay balancing, stable piles on the curved moving planet, and collision-chain usability also need human feedback. Circular asteroids can roll; friction is not glue.

Momentum is not cancelled each frame. Thrust, external wall reactions, ammunition creation, and destruction can change the system's momentum or mass during play. Zero initial drift and two-way gravitational forces are tested independently of those gameplay effects.
