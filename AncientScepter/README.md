
## Standalone Ancient Scepter

[![github issues/request link](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/PoseHelper/github_link.webp)](https://github.com/DestroyedClone/AncientScepter/issues) [![discord invite](https://raw.githubusercontent.com/DestroyedClone/PoseHelper/master/PoseHelper/discord_link.webp)](https://discord.gg/DpHu3qXMHK)

- Adds the Ancient Scepter from Risk of Rain 1 as a standalone mod
- Code is almost entirely by ThinkInvisible, this is merely a port of that, without the TILER2 dependency and with a custom model

[![](https://raw.githubusercontent.com/DestroyedClone/AncientScepter/master/AncientScepter/readme/logbookpage.webp)
]()

| ![](https://raw.githubusercontent.com/DestroyedClone/AncientScepter/master/AncientScepter/readme/croco.webp) | ![](https://raw.githubusercontent.com/DestroyedClone/AncientScepter/master/AncientScepter/readme/logbookicon.webp) |
|--|--|

## Upgrades
    - Commando: Suppressive Fire > Death Blossom (2x shots, fire rate, and accuracy. 2x autoaim range, hold primary fire to fire normally)  
	    - -OR-  Grenade > Carpet Bomb (0.5x damage, throw a spread of 8 at once)
    - Huntress: Arrow Rain > Burning Rain (1.5x duration and radius, burns)  
	    - -OR-  Ballista > Rabauld (5 extra weaker projectiles per shot, for 2.5x TOTAL damage)
    - Bandit: Lights Out > Assassinate (2x bullets)
	    - -OR-  Desperado > Renegade (25% (+0.35% per token) chance to ricochet to another enemy on hit up to 8 times within 30m. -10% distance & damage per bounce. Unaffected by luck.) 
    - MUL-T: Transport Mode > Breach Mode (0.5x incoming damage, 2x duration; after stopping, retaliate with a stunning explosion for 100% of unmodified damage taken)
    - Engineer: TR12 Gauss Auto-Turret > TR12-C Gauss Compact (+1 stock, +1 placed turret cap)  
	    - -OR-  TR58 Carbonizer Turret > TR58-C Carbonizer Mini (+2 stock, +2 placed turret cap)
    - Artificer: Flamethrower > Dragon's Breath (hits leave lingering fire clouds)  
	    - -OR-  Ion Surge > Antimatter Surge (2x damage, 4x radius)
    - Mercenary: Eviscerate > Massacre (2x duration, kills refresh duration [hold special to leave early])  
	    - -OR-  Slicing Winds > Gale-Force (4x stock and recharge speed, fires all charges at once)
    - REX: Tangling Growth > Chaotic Growth (2x radius, pulses additional random debuffs)  
	    - -OR-  DIRECTIVE: Harvest > COMMAND: Reap (Spawns extra fruit that gives random buffs)
    - Loader: Charged Gauntlet > Megaton Gauntlet (2x damage and lunge speed, 7x knockback)  
	    - -OR-  Thunder Gauntlet > Thundercrash (3x lightning bolts fired, cone AoE becomes sphere)
    - Acrid: Epidemic > Plague (victims become walking sources of Plague, chains infinitely)
    - Captain: Orbital Probe > 21-Probe Salute (1/3 damage, 7x shots, hold primary to fire continuously)
	    - -OR-  OGM-72 'DIABLO' Strike > PHN-8300 'Lilith' Strike (30s, 2x blast radius, 100,000% damage, blights everyone in line of sight)
    - Heretic: Nevermore (Special) > Perish Song (After 30 seconds, deals 5000% fatal damage to you and nearby enemies.)
	- Railgunner: Supercharge (Special) > Hypercharge (-20 armor on hit, +0.5 proc coefficient)
	    - -OR-  Cryocharge > Permafrosted Cryocharge (Explodes on contact with a frost blast, dealing 200% damage to all enemies within 6m. Slows on hit by 80% for 20 seconds.)
	- Void Fiend: Crush/Corrupted Crush -> Reclaimed Crush/Forfeited Crush (Also affects nearby enemies or allies within 25m. Corrupted Supress has +2 charges.)

    - Alloy Vulture: Windblade > Repeated Windblade (50% chance to fire twice)
    - Aurelionite: Eye Laser > Piercing Eye Laser (Additional 35% of damage dealt pierces walls)

## Implemented ModCompat
This refers to compatibility that's included just with this mod, and will not be an exhaustive list of every mod that implements this.
- BetterUI [⚡](https://thunderstore.io/package/XoXFaby/BetterUI/)

## To-Do/Issues
* Mithrix Scepter
* CaptainBustedAirstrike support:
	* Currently it uses the reduced cooldown from the standard alt airstrike.
* Alternate Replacement for Diablo Strike
* Further improvements to scepters(?)
* Railgunner, Void Fiend
* Fix Void Fiend's Scepter desyncing overrides if the Scepter is lost in corrupt mode

## For Devs
- Adding a ScepterReplacer to a character that already has an existing ScepterReplacer for that slot and variant will replace it.
	- This change allows developers that modify or outright change existing skills to have their own Scepter skill for existing characters.
- All icons for Scepter Skills in this mod are now publicly accessible. See the static class `Assets.SpriteAssets`.

## Credits
* **ThinkInvisible** [⚡](https://thunderstore.io/package/ThinkInvis/)[🐙](https://github.com/ThinkInvis) - Original code and implementation
* **DestroyedClone** [⚡](https://thunderstore.io/package/DestroyedClone/)[🐙](https://github.com/DestroyedClone) - Porter, Maintainer
* **rob** - Fixed code
* **bruh** and **redacted** - Made the Scepter model
* **swuff** [🐙](https://github.com/swuff-star) - Updated Textures, Consultation
* **QandQuestion**  - Lore
* **Moffein** [⚡](https://thunderstore.io/package/Moffein/)[🐙](https://github.com/Moffein) - Consultation
* **Mico27** [⚡](https://thunderstore.io/package/Mico27/)[🐙](https://github.com/Mico27/) - Orb creation help
* **TheTimeSweeper** [⚡](https://thunderstore.io/package/TheTimesweeper/)[🐙](https://github.com/TheTimeSweeper) - UnusedMode, Alternate Item Model, Item Displays, Other help
* **/vm/** ⚡🐙 - Idea for Captain's Lilith Strike
* **Zenithrium** [⚡](https://thunderstore.io/package/Zenithrium/)[🐙](https://github.com/Zenithrium/) - Nemmando IDR
* **RandomlyAwesome** [⚡](https://thunderstore.io/package/RandomlyAwesome/)[🐙](https://github.com/yekoc) - BetterUI Compat Fix, Various Scepter Skill fixes