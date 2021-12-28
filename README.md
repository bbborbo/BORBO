# BORBO

## Full Changelog Notes
Each "Category" (State of XYZ) is divided into smaller subsections, called "Packets" of changes. These packets contain a small amount of changes, which may all be enabled or disabled at once, however the changes within these packets may not be enabled or disabled individually. Further, each category also includes custom content, which may all be enabled or disabled individually, as well as "Essential" changes that CANNOT be disabled without disabling the entire category.

The idea of having packets of changes is to allow users to customize the scope of BalanceOverhaulRBO to a finer degree, without being able to separate coupled changes. For example, packets of changes might include a set of buffs AND nerfs to Ceremonial Dagger, or they would be included as a pair of changes that complement each other, such as reworking various stun items. This same principle applies to Essential changes and their items as well. The items in BalanceOverhaulRBO serve as a means to an end for balance purposes - they would not fit into Risk of Rain 2 without the Essential changes of each category that accompany them. 

# State of Defense + Healing

### ESSENTIAL CHANGES...
```
Healing:
- Medkit now heals for 8% of max health (+25 flat per stack)
- Monster Tooth now heals for 3% of max health (+8 flat per stack)
- Harvester's Scythe healing reduced from 8(+4 per stack) to 3(+3 per stack)

Mobility:
- Goat Hoof speed bonus reduced from 14% to 10%
- Hopoo Feather vertical jump bonus reduced from 50% to 0%
- Hopoo Feather horizontal jump bonus reduced from 50% to 30%

Defense:
- Tougher Times dodge chance function modified; now approaches 60% as stacks approach infinity (instead of 100%)


Added Items: Borbo's Band, Frozen Turtle Shell
Added Equipment: Master Ninja Gear
Added Scavenger: Baba the Enlightened
```

### OTHER...
```
(Dynamic Jump packet)
- Holding the jump input now lets you jump up to 130% as high
- Releasing the jump input early now lets you jump up to 80% as high

(Jade Elephant packet)
- Buff can now stack
- Reduced armor bonus from 500 to 200
- Increased buff duration from 5 seconds to 10 seconds

(Medkit packet)
- Changed healing timer to be a debuff; can now be cleansed by Blast Shower

(Bison Steak packet)
- Reverted to pre-CUM behavior
- Killing an enemy increases health regen by 2 hp/s for 3 seconds (+3 seconds per stack)
- Meat regen boost can now stack

(K'kuhana's Opinion packet, D+H)
* Wanted to keep Opinion relevant despite healing nerfs. Feel free to turn this packet off if you hate what this mod stands for. *
- Damage increased from 250% of healing taken to 350%
- Now pushes enemies hit away from the player

(Droplet General packet)
- Increased pickup radius of all droplets (monster tooth, bandolier, ghors tome) from 1 meter to 2.5 meters
- Improved droplet pickup behavior to prioritize players over minions

(Monster Tooth packet)
- Increased droplet lifetime from 5 seconds to 15 seconds

(Titanic Knurl packet)
- Grants 15 armor unconditionally; configurable

(Rose Buckler packet)
- Grants 10 armor unconditionally; configurable

(Repulsion Armor Plating packet)
- Grants 2 armor unconditionally; configurable

(Energy Drink packet)
- (If DuckSurvivorTweaks is loaded) Increased speed bonus to 40% (+25% per stack)
- (If DuckSurvivorTweaks is NOT loaded) Reduced speed bonus to 20% (+12.5% per stack)
```

# State of Health

### ESSENTIAL CHANGES...
```
Added Items: Utility Belt, Flower Crown
Added Equipment: Remarkably Stable Tesla Coil (replaces Unstable Tesla Coil)
Added Scavenger: Bobo the Unbreakable
```

### OTHER...
```
(Barrier General packet)
- Barrier now decays by 1/6 of current barrier (instead of 1/30 of max barrier) each second
- Aegis now also reduces barrier decay rate by 33%

(Infusion packet)
- Reduced health limit from 100 health to 30; configurable
- Health gained from infusion now also increases by 20% for each player level
* With these changes, Infusion effectively acts like an addition to the base health of every survivor *
```

# State of Interaction

### ESSENTIAL CHANGES...
```
(Boss Items)
- All bosses now have a chance to drop their boss items on death
- Boss items no longer drop from the teleporter (Someone remind me to look into replacing this feature at a later date)
* This can be configured for Aurelionite *
- Removed Overgrown Printers entirely

(Misc)
- Chill Slow now also reduces attack speed of those afflicted by 60%
- Reduced Lunar Chimera Wisp base attack speed to 70%

Added Items: AtG Missile Mk.3 (replaces Mk.1), Wicked Band, Magic Quiver, Permafrost
Added Equipment: Old Guillotine (replaces Old Guillotine)
```

### OTHER...
```
(Shattering Justice packet)
- Shattering Justice now instantly pulverizes on hits that deal more than 800% base damage on top of vanilla effects

(Tar packet)
- Now also reduces attack speed of those afflicted by 30%

(Kit Slow packet)
- Now also reduces attack speed of those afflicted by 30%
- Increased Clay Templar attack speed proportionally so that it doesn't fire slower due to slowing itself while shooting

(Chronobauble packet)
- Now also reduces attack speed of those afflicted by 50%

(Resonance Disc packet)
- This was supposed to change the spin per kill and decay rate but it doesn't anymore because Hopoo reworked it. Someone remind me to take another look at this some time.
- Reduced proc coefficient of Main Beam to 0.5

(Jellynuke packet)
- Reduced proc coefficient of Genesis Loop from 1.0 to 0.3
- Reduced damage coefficient of Genesis Loop from 6000% to 3000%
- The attack durations of Genesis Loop and Vagrant's Jellynuke are no longer affected by slows in response to slow debuff changes

(Planula packet)
- Completely reworked; now creates a Grandparent sun over your head while you are stationary.

(Shatterspleen packet, INT)
- Completely reworked; now fires volleys of void spikes after dealing more than 500% damage in a single hit

(Blacklist packet)
- All healing items are AI-Blacklisted
- Scavengers can no longer spawn with healing equipment

(Enigma packet)
- Effigy of Grief is now flagged as Enigma-compatible
- Milky Chrysalis is now flagged as Enigma-compatible

(Stun packet)
- Removed Stun Grenade
- Reworked Royal Capacitor to be more of a group stun item; Increased blast radius from 3m to 13m
- Reduced Royal Capacitor damage from 3000% to 1000%

(Backup packet)
- Reduced cooldown of The Back-up from 100s to 60s
```

# State of Damage

### ESSENTIAL CHANGES...
```
Razorwire
- Reduced proc coefficient from 0.5 to 0.2
- Increased damage from 160% to 360%
- Added an internal cooldown for 1 second between razors

Misc
- Runald's Band changed to deal 600% base + 100% total damage instead of 250% total
- Kjaro's Band changed to deal 600% base + 100% total damage instead of 250% total
- Sticky bomb moved to green tier. Now has a 5% chance (+5% per stack) to deal 320% damage (+40% per stack).

Added Items: Chefs Stache, The New Lopper, Enchanted Whetstone
Added Scavenger: Chipchip the Wicked
```

### OTHER...
```
(Crit packet)
- Ocular HUD now passively doubles Critical Strike damage
- Lensmaker's glasses now grant 7% Critical Strike chance instead of 10%

(Death Mark Fix packet)
- Minor rework. Now increases damage dealt to an enemy by 30%(+30% per stack) for 7 seconds.

(Molten Perforator packet)
- Reduced proc coefficient from 0.7 to 0

(Charged Perforator packet)
- Reduced proc coefficient from 1.0 to 0

(Shatterspleen packet, DMG)
- Reduced proc coefficient from 1.0 to 0
- Has no effect if the Shatterspleen rework is active

(Fireworks packet)
- Reduced proc coefficient from 0.2 to 0

(N'Kuhana's Opinion packet, DMG)
- Reduced proc coefficient from 0.2 to 0

(Little Disciple packet)
- Reduced proc coefficient from 1.0 to 0

(Resonance Disc packet)
- Reduced all proc coefficients to 0

(Ceremonial Dagger packet)
- Reduced proc coefficient from 1.0 to 0
- Reduced projectile lifetime from 10s to 3s

(Will-o-the-Wisp packet)
- Reduced proc coefficient from 1.0 to 0
- Changed blast radius from 12m (+2.4 per stack) to 16m
- Changed damage from 350%(+280% per stack) to 350%(+200% per stack)

(Gasoline packet)
- Reduced initial damage from 150% (+0% per stack) to 50% (+0% per stack)
- Changed burn damage from 150% (+75% per stack) to 175% (+100% per stack)

(Glowing Meteorite packet)
- Blast falloff model changed from Linear to None

(Warcry packet)
- Warcry now grants +100% damage and +50% attack speed instead of +100% attack speed
```

# State of Economy

### ESSENTIAL CHANGES...
```
- Increased scaling of interactable prices from 1.25 to 1.6
- Legendary Chests now have a cost multiplier of 10x instead of 16x
- Big Drones now have a cost multiplier of 8x instead of 14x
- AWU now has adaptive armor to help match chest cost scaling
- Monsters spawned by the teleporter event drop even less gold. Spend more time in combat!

Added Item: Golden Gun
Added Scavenger: Gibgib the Greedy
```

### OTHER...
```
(Printer packet)
- Printers now only spawn on stages 2, 4, and 5
- Increased Green printer spawn rate
- Increased Red printer spawn rate (ESPECIALLY on stage 5)

(Scrapper packet)
- Scrappers now only spawn on stages 1 and 3
- Increased Scrapper spawn rate

(Newt packet)
- Every newt altar now only has a 30% chance to spawn, including ones which were previously guaranteed to spawn

(Blood packet)
- Blood shrines now grant 2 chests worth of gold per health bar sacrificed
```

# State of Difficulty

### ESSENTIAL CHANGES...
```
- Reduced the rate at which the Difficulty Coefficient scales over stages from 115% to 110%
- Increased the rate at which the Difficulty Coefficient scales over time from 100% to 120%
- The Difficulty Slider starts at a higher level depending on the difficulty setting. No longer are the days where every difficulty starts just as easy as the others.
- The radius of the teleporter's particles is smaller depending on the difficulty setting. 125% in drizzle, 100% in rainstorm, 50% in monsoon, and no particles in eclipse.
- Tier 2 elites spawn earlier depending on the difficulty setting. Stage 1 in eclipse, stage 4 in monsoon, stage 6 in rainstorm (the default), and stage 11 in drizzle.
- Monsoon Only: All monsters gain unique scaling stat bonuses.

Added T1 Elites: Frenzied, Volatile
Added T2 Elite: Serpentine
```

### OTHER...
```
(Elite Stat packet)
- Reduced the health boost of base Tier 1 elites from 4x to 3x
- Reduced the health boost of Tier 2 elites from 18x to 9x
- Reduced the damage boost of base Tier 1 elites from 2x to 1.5x
- Reduced the damage boost of Tier 2 elites from 6x to 4.5x

(Overload packet)
- Overloading bombs no longer stick to targets
- Increased overloading bomb blast radius from 6m to 9m
- Reduced overloading bomb lifetime from 1.5s to 1.2s
- Overloading elites now deal far less knockback to players
- Increased the damage of overloading bombs from 50% total to 150% total
```



