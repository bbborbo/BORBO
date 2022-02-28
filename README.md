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

Mobility:
- Goat Hoof speed bonus reduced from 14% to 10%
- (If HuntressBuffUltimate is NOT loaded) Energy Drink speed bonus reduced to 20% (+12.5% per stack)
- Hopoo Feather vertical jump bonus reduced from 50% to 0%
- Hopoo Feather horizontal jump bonus reduced from 50% to 30%

Defense:
- Tougher Times dodge chance function modified; now approaches 60% as stacks approach infinity (instead of 100%)
```

### FEATURES & CONTENT...
```
Added Items: Borbo's Band, Frozen Turtle Shell
Added Equipment: Master Ninja Gear
Added Scavenger: Baba the Enlightened
```

### OTHER...
```
(Harvester's Scythe packet)
- Healing reduced from 8(+4 per stack) to 3(+3 per stack)

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
* Wanted to keep Opinion relevant despite healing nerfs. *
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
```

# State of Health

### ESSENTIAL CHANGES...
```
(Barrier)
- Barrier now decays by 1/6 of current barrier (instead of 1/30 of max barrier) each second
- Aegis now also reduces barrier decay rate by 33%
```

### FEATURES & CONTENT...
```
Added Items: Utility Belt, Flower Crown
Added Equipment: Remarkably Stable Tesla Coil (replaces Unstable Tesla Coil)
Added Scavenger: Bobo the Unbreakable
```

### OTHER...
```
(Infusion packet)
- Reduced health limit from 100 health to 30; configurable
- Health gained from infusion now also increases by 20% for each player level
* With these changes, Infusion effectively acts like an addition to the base health of every survivor *
```

# State of Interaction

### ESSENTIAL CHANGES...
```
Misc:
- Chill Slow now also reduces attack speed of those afflicted by 60%
- Reduced Lunar Chimera Wisp base attack speed to 70%
```

### FEATURES & CONTENT...
```
Added Items: AtG Missile Mk.3 (replaces Mk.1), Wicked Band, Magic Quiver, Permafrost
Added Equipment: Old Guillotine (replaces Old Guillotine)
```

### OTHER...
```(Shattering Justice packet)
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
Razorwire:
- Reduced proc coefficient from 0.5 to 0.2
- Increased damage from 160% to 360%
- Added an internal cooldown for 1 second between razors

Damage:
- Runald's Band changed to deal 600% base plus 100% (+100% per stack) total damage instead of 250% total
- Kjaro's Band changed to deal 600% base plus 100% (+100% per stack) total damage instead of 250% total
- Sticky bomb moved to green tier. Now has a 5% chance (+5% per stack) to deal 320% damage (+40% per stack).
```

### FEATURES & CONTENT...
```
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
- Now creates pools of napalm on detonation instead of burning directly

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