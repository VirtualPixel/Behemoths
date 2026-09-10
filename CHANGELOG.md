# Changelog

## 1.0.2

- Config changes apply to the bosses already in the level, from the REPOConfig menu or a save of the cfg file. Size, ColliderCap, HeightCap, WidthCap, health, and resistance all retune live; health is rebuilt from the vanilla base so it never stacks. Made for dialing in the body on a boss you are looking at.
- New WidthCap alongside HeightCap, for maps whose doors don't like a wide body. Reach follows the width cap when one is set.
- Setting DamageResistance to 0 now actually clears it on a live boss.

## 1.0.1

- Bosses have the body they look like. The collider cap defaulted to 1, so a giant Robe had a vanilla-sized hitbox and grab body inside a huge mesh: shots and grabs went through most of it and the game rescued you off its tiny collider. Now the body follows the size, pathing keeps its vanilla width so they still get through doors, and the "close enough to swing" distance each monster has written into its attack code grows with it, so a giant actually reaches you. Height holds at vanilla by default (HeightCap) so a wide boss still walks through doorways instead of lying on its side in one.

## 1.0.0

- First release.
- Boss levels roll with a chance that climbs along the game's own difficulty curve, from BaseChance at the start of a run to MaxChance at level 10.
- The map scales the roll: Swiftbroom Academy and the Museum lean toward Behemoths, McJannek Station leans away, Headman Manor is the plain chance, and every map has its own slider (modded maps share one).
- No boss levels before level 3, the same way the game holds its nastier monsters back. Configurable.
- The cooldown between boss levels is saved with the run, so quitting and loading does not reset it.
- Behemoth monsters: grow-gun scaling plus health, damage, and damage-resistance multipliers.
- Behemoth hits reach every player in the lobby, host or not, mod or no mod.
- A fairness cap keeps a hit you could survive at full health survivable at full health (75% of max health by default).
- Behemoths on the move shake the host's camera when they're close.
- Boss orbs: 2.5x value, twice the size, heavier, amber glow, tougher against losing value, never shrink back down.
- Boss-round loot: every other valuable in the level worth more, added after the haul goal is set so it's pure profit and doesn't cut the spawn count.
- Boss levels announce themselves through the game's moon-phase popup, retitled BOSS LEVEL. On a level that is also a moon change, the moon's name and modifiers stay above the boss lines. Toggleable.
- Everything configurable, with in-game sliders under REPOConfig.
- Requires ScalerCore 1.0.6.
