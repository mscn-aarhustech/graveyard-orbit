# Prototype Validation

Validated on **November 5, 2026** in Linux x64 (Ubuntu 22.04), with .NET SDK 10.0.403, Raylib-cs 8.0.0 (native raylib 6.0), and Box2D.NET 3.1.654.

## Automated checks

| Check | Result |
| --- | --- |
| Locked NuGet restore | Passed |
| Release solution build | Passed, zero warnings and zero errors |
| xUnit sample suite | 22 passed, zero failed, zero skipped |
| Asset checks | Manifest files present; cache reuse/disposal; missing-file errors; path boundary checks |
| Custom physics samples | Inverse-square gravity, mass scaling, centre handling, thrust, initial circular velocity, selected invalid inputs |
| Box2D integration | Velocity integration, real bullet/asteroid contact, and gameplay shot-to-contact-to-score path |
| Native raylib integration | Real Kenney PNG decoded without creating a window |
| Coverage collector | XPlat Code Coverage generated a Cobertura report; no coverage target imposed |
| Linux framework-dependent publish | Built and launched from outside the repository |
| Browser checks in headless Chromium | Callsign trimming, save/reload, text rather than markup, whitespace rejection/recovery, blocked storage, and no horizontal overflow at 390px and 320px |
| Website resource/JavaScript errors | None during the browser checks |

## Graphical and audio smoke checks

Ran the desktop application with a real native raylib window under Xvfb, using Mesa software rendering and an ALSA null audio sink. Both OGG sound files loaded and decoded successfully. The ship, asteroid and laser textures loaded into the graphics context. The program rendered 120 frames, generated a shot event, and shut down cleanly with textures, framebuffer and audio device disposed.

The executable reported:

```text
SMOKE frames=120 shot=True audio=True entities=8
```

The same smoke check passed for a published Linux build launched from another working directory. A gameplay screenshot is included at teaching/images/prototype.png. The screenshot is validation evidence, not a promised final art direction.

The null audio sink exercises device initialization, decoding and playback calls. It does **not** verify audible output through speakers. Synthesized smoke-test input does **not** verify physical keyboard and mouse usability.

## Remaining manual checks

Windows and macOS execution have not been verified in this generating environment. The included GitHub Actions matrix is configured to build/test on all three desktop platforms, but it has not been executed in your repository. GitHub Pages deployment and repository settings have not been changed or verified.

Before classroom rollout, have students or instructors on Windows and macOS perform: restore/build/test; launch from their IDE; thrust/rotate/fire; die and restart repeatedly; click mute/restart after resizing; verify audible laser/explosion sounds; and check their Pages deployment. On Linux, repeat with the school's actual graphics and audio hardware.

The source ZIP excludes bin/obj directories, package caches, local tool installations, and a .git directory. This is a source starter, not a bundled executable installer.
