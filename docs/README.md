# Website

Static HTML, CSS, and JavaScript. No framework or backend. All local links are relative so hosting below /graveyard-orbit/ works.

The callsign form stores a maximum of 24 characters in this browser only. It uses textContent instead of injecting markup and tolerates unavailable localStorage. The CSS is responsive and includes visible keyboard focus. The orbital illustration is decorative; this page does not run the desktop game.

Preview using python3 -m http.server 8000 --directory docs from the repository root. Publish by enabling GitHub Pages with GitHub Actions as its source. See ../README.md.

Ship image: Kenney Space Shooter Remastered, CC0. See images/KENNEY-LICENSE.txt. Original website code uses the root MIT license.
