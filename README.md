# Behemoths

Boss levels for R.E.P.O. The deeper your crew pushes, the more likely the next level turns into a boss level: every monster on it is a towering, tougher, harder-hitting Behemoth, and the orbs they drop are huge, heavy, and worth a small fortune. High risk, high reward.

**HOST ONLY.** Only the host needs Behemoths. The host's settings rule the run. For other players to *see* the monsters and orbs at their giant size, they need [ScalerCore](https://thunderstore.io/c/repo/p/Vippy/ScalerCore/) installed (a free, required dependency). Players without ScalerCore still take the extra damage and still get paid the boosted cash; they just see everything at normal size.

## What a boss level does

- **It announces itself.** When a boss level begins, the game's moon-phase popup announces it as a BOSS LEVEL. The host sees it (the mod only runs there); other players find out the usual way. Can be turned off.
- **Climbing odds.** Each level rolls a chance to be a boss level. The chance starts low and grows the further you get, capped at a ceiling you set.
- **Cooldown.** After a boss level, a configurable number of levels (3 by default) must pass before another can roll, so they stay special.
- **Behemoth monsters.** Every monster on a boss level is scaled up: bigger, heavier, deeper sounding. They also hit harder, carry more health, and shrug off a share of incoming damage, so they take real punishment to bring down. By default they *look* about 1.4x bigger while the body they collide, path, see, and attack with stays vanilla, so they fit doors, see you, and hit you normally, they just look huge. Push SizeMultiplier higher for an even bigger look, or raise ColliderCap if you want them physically bigger too (at the cost of doors and reach).
- **Boss orbs.** The orb a Behemoth drops is twice the size, heavier, worth 2.5x the cash of a normal one, amber-glowing, and tough enough to take a beating without losing its value. It never shrinks back down. Hauling one is a job.
- **Richer loot.** Every other valuable in a boss level is worth more too (1.5x by default). This is added after the level's value is set, so it's pure profit: the map still spawns its normal number of valuables and the extraction goal stays the same.

## Configuration

Everything is exposed in the config file (and in-game via [REPOConfig](https://thunderstore.io/c/repo/p/nickklmao/REPOConfig/) if you have it, rendered as sliders):

| Setting | Default | What it does |
| --- | --- | --- |
| Enabled | true | Master switch for boss levels. |
| AnnounceBossLevel | true | Announce boss levels through the game's moon-phase popup, reading BOSS LEVEL. |
| Boss Levels / BaseChance | 10% | Starting boss-level chance. |
| Boss Levels / ChancePerLevel | 5% | Added to the chance for each level completed. |
| Boss Levels / MaxChance | 60% | Ceiling the climbing chance can reach. Never caps below BaseChance, so BaseChance 100 = guaranteed. |
| Boss Levels / CooldownLevels | 3 | Levels between boss levels. 0 allows back-to-back. |
| Boss Levels / EarliestLevel | 1 | First level number that can be a boss level. |
| Boss Monsters / SizeMultiplier | 1.4 | How large Behemoths look. The body they behave with stays at ColliderCap, so this is purely the appearance. |
| Boss Monsters / HealthMultiplier | 3.0 | How much more health they have. |
| Boss Monsters / DamageMultiplier | 2.0 | How much harder they hit. |
| Boss Monsters / DamageResistance | 0.5 | Fraction of incoming damage shrugged off, on top of the extra health. 0.5 = takes half. |
| Boss Monsters / ColliderCap | 1.0 | The size a boss physically behaves at (collision, pathing, attack range and reach height), apart from its look. 1 keeps the body vanilla so it fits doors and attacks you normally while looking big. |
| Boss Orbs / ValueMultiplier | 2.5 | Cash value of a boss orb. |
| Boss Orbs / SizeMultiplier | 2.0 | How large a boss orb looks. |
| Boss Orbs / WeightMultiplier | 2.5 | How heavy a boss orb is, set apart from its size. |
| Boss Orbs / Durability | 5.0 | How much abuse a boss orb takes before it chips. 5 = roughly five times tougher than a normal valuable. |
| Boss Orbs / Glow | true | Amber glow on boss orbs so the big prizes stand out. Host only. |
| Boss Round Loot / ValuableMultiplier | 1.5 | Value of every other valuable in a boss level (pure profit, see above). |
| LogLevel | Off | Off logs one line per level with the boss roll. Debug adds per-monster and per-orb lines. Verbose is a full trace. |

Want pure chaos? Crank the size, drop the cooldown to 0, and raise MaxChance. Want a rare scary event? Lower BaseChance and ChancePerLevel and lengthen the cooldown.

## Compatibility

- Built on ScalerCore, so it shares its scaling, physics, audio, and multiplayer sync.
- Cash and damage changes ride vanilla networking, so they apply to every player regardless of which mods they run.
- A handful of special monster attacks deal fixed damage the game does not route through the normal damage path; those stay at their vanilla value while size, health, and orbs still apply.
- The boss cooldown lives in memory for the run. Quitting to the menu and loading the save keeps the level count (so the climbing chance carries on) but clears the cooldown, so the first level back can roll a boss.

## Installation

Install with Gale or r2modman and ScalerCore comes along as a dependency. By hand: drop `Behemoths.dll` into `BepInEx/plugins/` next to ScalerCore. Only the host needs it.

## Contact

| Purpose | Where |
|---|---|
| Bug reports and suggestions | [GitHub Issues](https://github.com/VirtualPixel/Behemoths/issues) |
| Questions, test builds, or just hanging out | [Vippy's Discord](https://discord.gg/kKqhck2NrP) |
| R.E.P.O. modding in general | [R.E.P.O. Modding Server](https://discord.gg/9fDzZ9sk95) |

Everything I make stays free. If one of these mods made your runs better and you feel like saying thanks, there is a [Ko-fi](https://ko-fi.com/vippydev).
