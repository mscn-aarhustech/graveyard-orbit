# Graveyard Orbit

A small C# space shooter for your first experience reading and changing someone else's code.

Install the **.NET 10 SDK**, then run these commands from the repository root:

```bash
dotnet restore GraveyardOrbit.sln --locked-mode
dotnet test GraveyardOrbit.sln
dotnet run --project src/GraveyardOrbit.Game
```

On Windows, open GraveyardOrbit.sln in Visual Studio with .NET 10 support. On macOS/Linux, or Windows, open the folder in VS Code with C# tooling and use the same terminal commands.

**W/S** thrust, **A/D** turn, **Space** shoot, **Shift** boost, **Ctrl** precise thrust, **F11** fullscreen, **Enter** restart, **M** mute, **Esc** exit. The mute and game-over restart buttons also accept mouse clicks.

There is room to fly: a 100 x 64 metre arena, a ship-following camera, and solid outer walls. Fullscreen shows more of the same world instead of enlarging the sprites. Gentle bumps are safe; hard impacts destroy ships and asteroids. Clear the rocks by shooting or pushing them into collisions.

Start with [the beginner guide](docs/Guide.md). Test results and platform limitations are in [validation notes](docs/Validation.md).

Original code uses [MIT](LICENSE). Kenney assets retain their original CC0 licenses. The root assets folder is preserved unchanged; dependency license texts are in docs/licenses.
