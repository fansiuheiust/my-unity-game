# Progression

## How players progress through the game.



### 0\. Abstract

* This game is a rogue-lite, meaning that the player will gain permanent progress by running dungeons. However, he will have to replay the dungeon should he die
* There will be multiple floors, with each floor featuring a unique final boss
* For now, it is assumed that there will be 10 floors
* For each dungeon run, the player has to start from the first floor
* Progression will be granted in the format of progression points of different categories
* Progression points can be gained by opening loot chests, dropped from mobs, or from end of floor rewards
* Progression points can be used to buy perks, for the following:
1.  	Choose to either skip (no or less rewards), or increase the difficulty (more rewards) of floors of lower difficulty
2.  	Predetermine items or buffs from the starting chest/loot chests in the dungeon to a category, rarity, or even specify items
3.  	Gain miscellaneous stat boosts
* Perks of each category will be presented as a tree
* Each player has a level from 1 to 20, determined by number of progression points
* Gears and buffs obtained will be stronger as the player levels up



1. ### Floors

The dungeon consists of 10 floors totally.

Each floor has its own final boss.

When ascending from a floor, the player will lose all of his gears, and can choose at most 3 buffs to carry over

Each rarity spans 2 floors, i.e.:

* Floor 1-2: Common
* Floor 3-4: Rare
* Floor 5-6: Epic
* Floor 7-8: Legendary
* Floor 9-10: Mythical



### 2\. Leveling

There will be 20 levels.

The player levels up as he collects progression points.

The level curve follows a quadratic shape from level 1 to 20.



### 3\. Progression Points and their Perks

Progression points are points that will be kept permanently once the player leaves the dungeon either by failing or clearing it.

There are 3 types of progression points: namely Coin of Clearance, Coin of RNG, Coin of Class.

Each type of coin has 5 tiers: Common, Rare, Epic, Legendary, Mythical (as in mythical algorithm pull).

Higher tier coins can be decomposed to lower tier ones, but lower tier coins cannot be upgraded to higher tier ones (this is to discourage grinding low floors repetitively).

The higher the floor, the higher tier the dropped coins are (linear)



#### 3.1. Coin of Clearance (skip/buff lower floors)

Mainly gained from completing a floor, but can appear in loot chests or mob drops.

For each completed floor, the player can choose 2 approaches: either skip or buff the floor.

For skipping, the progression is as follows:

* Floor Skipper: The floor will consist of 10/25/50/100% less rooms, with there being only the start room and final room in the final tier
* Scavenge: Gain 1/2/3/4 coin(s) per skipped room, can only trigger once per hour (Req: Floor Skipper 1)
* Rebuff: Gain 1/2/4/6 buff(s) after entering the floor (Req: Floor skipper 4)
* Autoslay: Final boss spawns with 10/25/50% less HP. (Req: Floor Skipper 4)
* Ultimate Skipper: When entering the dungeon, automatically enters the next floor, and choose 3 buffs from Rebuff (Req: Autoslay 3)

For buffing, the progression is as follows:

* Scaler: The difficulty and rewards of this floor is 1/2/3 floors higher
* Master Scaler: The difficulty and rewards of this floor is the highest floor you have cleared (floor 7+: +4 floors)
* Bossier Boss: The final boss has 1/2/3 more abilities, while end-of-dungeon rewards are increased by 30/65/120%



#### 3.2. Coin of RNG (predetermine loots)

Upgrades:

* <class> Fixation: 20%/40%/60%/80%/100% of the gears obtained will be drawn only from the <class> pool
* Utilitarianism: Starter chest contains 10 explosives/20 explosives/1 infinite explosive (implementation will be delayed)
* Selective <gear type>: You can select <gear type> of rarity at most common/rare/epic/legendary/mythical that you have obtained before from starter chest.
* Fated <gear type>: <gear type> obtained from starter chest must be at least rare/epic/legendary/mythical

#### 3.3. Coin of Class (Class and Archetype (blatant name stealing from Wynncraft))

Mainly gained from mob drops, but can be found in loot chests or upon completing a floor

##### 3.3.1. Melee

Wields a melee weapon.

* Melee: Wielding a melee weapon gives you 5%/10%/25% more damage. (Excl: Ranged, Mage)

##### 3.3.2. Ranged

Wields a ranged weapon.

* Ranged: Wielding a melee weapon gives you 5%/10%/25% more damage. (Excl: Melee, Mage)
* Faster Charge: Reduces weapon charging time by 5%/15%/30%/50%.
* Snipe: Projectiles' damage increase by up to 5%/10%/20%/33% as it travels. Max projectile damage is attained if it has travelled half of its range. (Excl: Epins)
* Epins: Projectiles' damage increase to 5%/10%/20%/33%, this effect decreases as it travels, losing the effect if it has travelled half of its range. (Excl: Snipe)
* Barrage (Damage Ability): Automatically shoots projectile at a rate 1/1.25/2 times your attack speed for 3/3/5 seconds. Cooldown: 10 seconds.
* Agility: Increases dodge movement magnitude by 5%/10%/20%/33%. (Excl: Skillful Dodge)
* Skillful Dodge: Increases dodge immunity duration by 0.05/0.1/0.15/0.2 seconds. (Excl: Agility)
* ...

3 mutually-exclusive ultimates to choose from

* Sharpshoot: Enters focus mode (Reduced FOV and first person mode, lock movement, ult again to force exit). In this state, looking at an enemy at most 1/1/2 times your attack range forces it in front of you as long as they are not knockback immune (bosses). You can attack 2/3/5 times. For each attack, shoot a beam that damages everyone in front of you at most 1/1/2 times your attack range, dealing 200%/200%/300% damage. Max duration: 10/10/10 seconds.
* Freezeshot: For the next 10/10/10 seconds, hitting an enemy stuns it unless it is immune. At the end of the duration, shoots 4/6/10 environment-piercing projectiles to each hit enemy in a span of 0.5/0.5/0.5 seconds, each dealing 100%/100%/100% damage.
* TBA

##### 3.3.3. Mage

Manipulates mana.

* Mage: Increases your mana regeneration by 5%/10%/25%. (Excl: Melee, Ranged)



### 4\. Gear and Buff Progression

A gear's or a buff's base stats will be scaled by the player's level.

The stats multiplier will follow a sigmoid-like shape from 1 to a greater value.



### 5\. Enemy Stats Increase

The enemies' base stats and their weapons' base stats will be scaled by the floor's level.
The scaling will be linear.

