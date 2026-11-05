# Starter Backlog

These are issue seeds, not existing GitHub issues. Choose a few; do not attempt every task. The baseline contains no deliberately planted bugs.

| Task | Size | Acceptance criteria / evidence |
| --- | --- | --- |
| Add a fuel gauge | Small | Thrust consumes fuel; empty fuel prevents thrust; restart refills; HUD and one unit test |
| Remap one control | Small | Change binding in one place; update README/site; verify held versus pressed semantics |
| Add a website theme toggle | Small | Works with keyboard; persists locally; restricted storage does not break the page |
| Add an alternate ship from the extension pack | Small | Manifest and build output updated; Linux filename case checked; cache still behaves correctly |
| Improve missing-asset diagnostics | Small | Message gives the logical name and corrective action; focused failure test |
| Pause and resume | Medium | Physics, cooldown and bullet lifetime stop; resume does not jump; input documented |
| Add asteroid fragments | Medium | Hit creates smaller bodies outside contact iteration; points cannot be counted twice |
| Predict the ship's orbit | Medium | Separate prediction, no mutation of Box2D bodies; path responds to current position/velocity; units documented |
| Add fuel mass and the rocket equation | Medium | Define simplified model and units; verify limiting cases; restart is correct |
| Generate runtime asset copy entries | Medium | Manifest remains authoritative; no unused pack content published; automated check |
| Improve CI with graphical smoke testing | Medium | Linux job uses a virtual display; screenshot uploaded; native image test still runs |
| Discuss licensing and AI training | Discussion | Compare standard MIT with a proposed policy; distinguish requests from license conditions; preserve third-party licenses; do not casually relicense others' work |

## Bug-hunting prompts

Check resize/letterboxing and clickable controls; opposing keys; Ctrl plus Shift; restart while firing; repeated dispose; multiple contacts in one step; a missing PNG; uppercase/lowercase filenames; audio unavailable; callsigns containing markup; callsigns containing only whitespace; storage blocked by browser settings.

If you cannot reproduce a bug, write down what you checked. Do not invent a failure to satisfy the assignment. Missing features can become feature issues instead.
