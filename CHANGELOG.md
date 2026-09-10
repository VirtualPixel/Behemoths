# Changelog

## 1.0.0

First release.

- Boss levels roll with a chance that climbs along the game's own difficulty curve, from BaseChance at the start of a run to MaxChance at level 10. Never before level 3, and a cooldown between them that is saved with the run.
- The map scales the roll: Swiftbroom Academy and the Museum lean toward Behemoths, McJannek Station leans away, Headman Manor is the plain chance, and every map has its own slider (modded maps share one).
- Behemoths have the body they look like: hitbox, grab body, and the swing they reach you with all follow the size. Height holds at door height so a wide one still walks through doorways instead of lying on its side in one, and pathing keeps its vanilla width so they always find the door.
- Health, damage, and damage resistance multipliers, with a fairness cap so a hit you could survive at full health stays survivable at full health.
- Behemoth hits reach every player in the lobby, host or not, mod or no mod.
- Behemoths on the move shake the host's camera when they're close.
- Boss orbs: 2.5x value, twice the size, heavier, amber glow, tougher against losing value, never shrink back down. Every other valuable in a boss level is worth more too, added after the haul goal is set so it's pure profit.
- Boss levels announce themselves through the game's moon-phase popup, retitled BOSS LEVEL. On a level that is also a moon change the boss screen takes the moon's slot; the moon's modifiers still apply.
- Config changes land on the bosses already in the level, from the REPOConfig menu or a saved cfg edit, so you can dial in a body on a boss standing in front of you.
- Requires ScalerCore.
