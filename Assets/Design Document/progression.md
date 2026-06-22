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

* <class> Fixation: Gears can only be generic or <class>-oriented
* Utilitarianism: Starter chest contains 10 explosives/20 explosives/1 infinite explosive
* Fated <gear type>: <gear type> obtained from starter chest must be at least rare/epic/legendary/mythical
* Selective <gear type>: <gear type> obtained from starter can be selected from all common/rare/epic/legendary/mythical <gear type>s that you have obtained once

#### 3.3. Coin of Class (Class and Archetype (blatant name stealing from Wynncraft))

Mainly gained from mob drops, but can be found in loot chests or upon completing a floor

##### 3.3.1. Melee

Wields a melee weapon.

##### 3.3.2. Ranged

Wields a ranged weapon.

##### 3.3.3. Mage

Manipulates mana-based items.



### 4\. Gear and Buff Progression

A gear's or a buff's base stats will be scaled by the player's level.

The stats multiplier will follow a sigmoid-like shape from 1 to a greater value.



### 5\. Enemy Stats Increase

The enemies' base stats and their weapons' base stats will be scaled by the floor's level.
The scaling will be linear.

