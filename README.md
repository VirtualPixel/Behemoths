# Behemoths

Boss levels for R.E.P.O. The deeper your crew pushes, the more likely the next level turns into a boss level: every monster on it is a towering, tougher, harder-hitting Behemoth, and the orbs they drop are huge, heavy, and worth a small fortune. You never know which level it will be, only that some maps are Behemoth country. High risk, high reward, and a whole new reason to dread the third level.

**HOST ONLY.** Only the host needs Behemoths. The host's settings rule the run. For other players to *see* the monsters and orbs at their giant size, they need [ScalerCore](https://thunderstore.io/c/repo/p/Vippy/ScalerCore/) installed (a free, required dependency). Players without ScalerCore still take the extra damage and still get paid the boosted cash; they just see everything at normal size.

> **A note from Vippy**
>
> After way too long of a break, I'm finally back and working on these again :D Every one of my mods just got a full pass: bugs fixed, reports read, a few things I'd always meant to do. New builds and fixes land in [Vippy's Discord](https://discord.gg/kKqhck2NrP) before they hit Thunderstore, so come hang out. Thanks for sticking around.

## What a boss level does

- **It announces itself.** When a boss level begins, the game's moon-phase popup announces it as a BOSS LEVEL. Only the host sees it (the mod only runs there), so it's on you to warn the crew, or to let the first Behemoth do it. Can be turned off.
- **Climbing odds.** Each level rolls a chance to be a boss level. The chance starts low and climbs along the game's own difficulty curve, the same one that adds monsters as you go, topping out at level 10.
- **Some maps more than others.** The map you land on scales the roll. By default Swiftbroom Academy and the Museum lean toward Behemoths (big halls), McJannek Station leans away (tight corridors), and Headman Manor is the plain chance. Every map has its own slider, and modded maps get one shared slider.
- **Never on level 1 or 2.** Boss levels can't roll before level 3, the same way the game holds its nastier monsters back until you have two levels behind you. Configurable.
- **Cooldown.** After a boss level, a configurable number of levels (3 by default) must pass before another can roll, so they stay special. The cooldown is saved with the run, so quitting and loading a save does not reset it.
- **Behemoth monsters.** Every monster on a boss level is scaled up: bigger, heavier, deeper sounding. They also hit harder, carry more health, and shrug off a share of incoming damage, so they take real punishment to bring down. By default they are 1.4x bigger, body included: what you see is what you shoot, grab, and get hit by, and their swing reaches as far as it looks. Height holds at vanilla so they still fit doorways and ceilings, and pathing keeps its vanilla width so they always find the door. Push SizeMultiplier higher for more bulk, or hold the body back with the caps where a map's doors need it.
- **Hits that hurt, but stay fair.** Behemoth hits do double damage by default, for every player in the lobby. A fairness cap keeps any hit you could survive at full health survivable at full health: a boosted hit tops out at 75% of your max health unless the normal hit was already worse than that. You leave the encounter hurting, not dead from one touch you didn't see coming.
- **Tremors.** A Behemoth on the move shakes your camera when it's close, so you feel one coming through the walls before you see it. Host only: nobody else runs the mod, and camera shake doesn't travel.
- **Boss orbs.** The orb a Behemoth drops is twice the size, heavier, worth 2.5x the cash of a normal one, amber-glowing, and tough enough to take a beating without losing its value. It never shrinks back down. Hauling one is a job.
- **Richer loot.** Every other valuable in a boss level is worth more too (1.5x by default). This is added after the level's value is set, so it's pure profit: the map still spawns its normal number of valuables and the extraction goal stays the same.

## Configuration

Everything is exposed in the config file (and in-game via [REPOConfig](https://thunderstore.io/c/repo/p/nickklmao/REPOConfig/) if you have it, rendered as sliders):

| Setting | Default | What it does |
| --- | --- | --- |
| Enabled | true | Master switch for boss levels. |
| AnnounceBossLevel | true | Announce boss levels through the game's moon-phase popup, reading BOSS LEVEL. On a level that is also a moon change, the boss screen shows instead of the moon one (the moon's modifiers still apply). |
| Boss Levels / BaseChance | 10% | Boss-level chance at the start of a run. |
| Boss Levels / MaxChance | 60% | Boss-level chance from level 10 on. The climb between the two follows the game's difficulty curve. Never sits below BaseChance, so BaseChance 100 = guaranteed. |
| Boss Levels / CooldownLevels | 3 | Levels between boss levels. 0 allows back-to-back. Saved with the run. |
| Boss Levels / EarliestLevel | 3 | First level number that can be a boss level. |
| Map Odds / Manor | 1.0 | Multiplier on the chance at Headman Manor. |
| Map Odds / Arctic | 0.75 | Multiplier on the chance at McJannek Station. |
| Map Odds / Wizard | 1.25 | Multiplier on the chance at Swiftbroom Academy. |
| Map Odds / Museum | 1.25 | Multiplier on the chance at the Museum of Human Art. |
| Map Odds / OtherMaps | 1.0 | Multiplier on the chance on any other map, modded ones included. |
| Boss Monsters / SizeMultiplier | 1.4 | How large Behemoths are, look and body. Height is held by HeightCap. |
| Boss Monsters / HealthMultiplier | 3.0 | How much more health they have. |
| Boss Monsters / DamageMultiplier | 2.0 | How much harder they hit. |
| Boss Monsters / HitCap | 75% | A boosted hit never takes more than this share of a player's max health, unless the normal hit already did. 100 turns the cap off. |
| Boss Monsters / DamageResistance | 0.5 | Fraction of incoming damage shrugged off, on top of the extra health. 0.5 = takes half. |
| Boss Monsters / ColliderCap | 0 | Holds the whole body (hitbox, grab body, reach) at this multiple of vanilla while the look keeps growing. 0 lets the body follow SizeMultiplier. |
| Boss Monsters / HeightCap | 1.0 | How tall the body may get. 1 keeps it door height. 0 lets it grow as tall as it looks, which leaves a big one stuck on its side in the first doorway. |
| Boss Monsters / WidthCap | 0 | How wide the body may get. 0 follows the look. Set a number, say 1.5, when a map's doorways are giving a wide boss trouble. |
| Boss Monsters / Tremors | true | Behemoths on the move shake the host's camera when they're close. |
| Boss Orbs / ValueMultiplier | 2.5 | Cash value of a boss orb. |
| Boss Orbs / SizeMultiplier | 2.0 | How large a boss orb looks. |
| Boss Orbs / WeightMultiplier | 2.5 | How heavy a boss orb is, set apart from its size. |
| Boss Orbs / Durability | 5.0 | How much abuse a boss orb takes before it chips. 5 = roughly five times tougher than a normal valuable. |
| Boss Orbs / Glow | true | Amber glow on boss orbs so the big prizes stand out. Host only. |
| Boss Round Loot / ValuableMultiplier | 1.5 | Value of every other valuable in a boss level (pure profit, see above). |
| LogLevel | Off | Off logs one line per level with the boss roll. Debug adds per-monster, per-hit, and per-orb lines. Verbose is a full trace. |

Want pure chaos? Crank the size, drop the cooldown to 0, set EarliestLevel to 1, and raise MaxChance. For a rare scary event, lower BaseChance and MaxChance and lengthen the cooldown. Zeroing a map's slider keeps Behemoths off that map entirely.

### Tuning with the game running

Every boss value applies to the Behemoths already in the level. Move a slider in REPOConfig, or edit the cfg file and save it, and within about a second the live bosses take the new size, body caps, health, and resistance. Damage and reach read the config on every hit anyway. The console logs a `[Tune]` line each time so you can see it land. Handy for dialing in a collider size on a boss that is standing right in front of you.

## Compatibility

- Built on ScalerCore, so it shares its scaling, physics, audio, and multiplayer sync.
- Cash and damage changes ride vanilla networking, so they apply to every player regardless of which mods they run. For players other than the host, the host sends the extra damage the moment it sees a Behemoth's hit land. That arrives as a second hit right behind the normal one, so it flashes twice, and under heavy lag a hit the host saw and you dodged can still sting.
- The Tick keeps its normal health. For the Tick health is hunger, and a bigger pool would either stop it biting or make it look full forever to everyone but the host. It still grows, hits harder, and shrugs off damage.
- Damage a monster deals without being credited for it (a throw that tumbles you into a wall, a pit) is not boosted.
- The boss cooldown is stored in the run's own stats, so it survives quitting to the menu and loading the save, and a new run starts clean.

## Installation

Install with Gale or r2modman and ScalerCore comes along as a dependency. By hand: drop `Behemoths.dll` into `BepInEx/plugins/` next to ScalerCore. Only the host needs it.

## Come hang out

I'm Vippy. I make R.E.P.O. mods and I read every bug report.

| | |
|---|---|
| **[Vippy's Discord](https://discord.gg/kKqhck2NrP)** | Bug reports, test builds before Thunderstore, and a say in what comes next. Come say hi. |
| **[R.E.P.O. Modding Server](https://discord.gg/9fDzZ9sk95)** | The whole modding scene, not just me. |
| **[More of my mods](https://thunderstore.io/c/repo/p/Vippy/)** | Everything else I've made for R.E.P.O. |
| **[Source on GitHub](https://github.com/VirtualPixel/Behemoths)** | The code, if you want to poke at it or send a fix. |

## Keep the mods coming

Everything I make is free and stays free. Two ways to help if you feel like it, neither one expected:

- **[Ko-fi](https://ko-fi.com/vippydev)**: buy me a coffee and your name goes on the supporters list in my Discord. Every coffee buys another evening on the next update.
- **[BisectHosting](https://bisecthosting.com/vippy)**: hosting a server for Minecraft or anything else your crew plays? Code `vippy` takes 25% off, and I get a cut at no cost to you. It's where my own servers live.

[![25% off BisectHosting servers with code vippy](https://www.bisecthosting.com/partners/custom-banners/71eecea6-f5bb-437d-ac56-f6fee4266193.png)](https://bisecthosting.com/vippy)
