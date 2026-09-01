# Changelog

## Unreleased

- Requires ScalerCore 1.0.4.
- A placed valuable that rolls its value after the boss-level sweep is multiplied when it rolls, instead of being pinned at the placeholder value.
- A boss that is still scaled when it spawns again is left alone rather than toggled back to vanilla size.
- On a level that is both a moon change and a boss level, the popup keeps the moon's name and modifiers above the boss lines.
- The announcement is the moon-phase popup, host side; the config text and README said banner, truck message, and stinger.
- README: LogLevel documented, install notes, contact section.

## 1.0.0

- First release.
- Boss levels with a climbing chance and a configurable cooldown.
- Behemoth monsters: grow-gun scaling plus health, damage, and damage-resistance multipliers.
- Boss orbs: 2.5x value, twice the size, heavier, amber glow, tougher against losing value, never shrink back down.
- Boss-round loot: every other valuable in the level worth more, added after the haul goal is set so it's pure profit and doesn't cut the spawn count.
- Boss levels announce themselves: red number + BOSS LEVEL caption on the arrival screen, a taxman warning on the truck screen, and a stinger. Toggleable.
- Everything configurable, with in-game sliders under REPOConfig.
