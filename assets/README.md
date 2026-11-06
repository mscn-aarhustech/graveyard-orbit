# Assets

The three supplied archives are unpacked into separate pack folders, preserving filenames, case, licenses, PNGs, vectors, spritesheets, previews, fonts, and sounds. Operating-system metadata (desktop.ini, .DS_Store, Thumbs.db) is omitted. Assets are source content, not generated files.

The game uses only five entries from asset-manifest.json. The extension pack is retained for student tasks but is not loaded by the prototype. The planet and stars are drawn procedurally. The renderer uses a 640 x 360 canvas and nearest-neighbor scaling; the supplied Kenney artwork is downscaled, not an entirely new pixel-art set.

## Updating a pack

Replace the contents of the matching pack folder, not the parent kenney folder. Preserve the new pack's license and update PACKS.json provenance. Check filename case on Linux. Update the manifest if a selected filename changed. If adding a runtime asset, also add its Content entry in GraveyardOrbit.Game.csproj. Run the asset tests and launch the game.

The explicit Content entries intentionally keep unused source assets out of build/publish output. This is small enough to understand without writing an asset pipeline. A useful student feature is generating these entries from the manifest.

The website uses a separate ship image so GitHub Pages only publishes docs/. Update that copy if the website artwork changes. Do not remove its license.

SpriteManager owns GPU textures and must be disposed while the window/context is alive. AssetCatalog owns logical paths, not GPU objects. SoundManager owns sounds and the audio device.
