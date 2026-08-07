# Miniboss: Mobs with Cool Abilities

Minibosses are elite mobs spawned when doing a Miniboss Room. Each miniboss has its own gimmick, and will drop unique gears and buffs (buffs are WIP).



### Miniboss Room Logistics

1. The player uses uses the interactable
2. The interactable chooses a miniboss to spawn (with perks, the player can choose which miniboss spawns)
3. Close all walls between the room
4. Spawn the miniboss
5. If player defeats the miniboss, the room unlocks
6. Miniboss drops coins, a buff, and a gear (with perks, it does not drop the gear, instead opens a UI for the player to choose a gear from the miniboss).

Note: Miniboss gears cannot appear in starter chest.



### Miniboss Design Principles

* Minibosses should serve as elite mobs
* Players are intended to 1v1 the miniboss without interference from mobs of other rooms
* A miniboss may terraform the room if seen fit
* A miniboss can use any behaviour scripts, as long as they fit the intended playstyle
* Each miniboss should have a set of unique abilities
* Abilities should not be overkill, such as "do something or get one-shot"
* Abilities should also have clear courtesy, sufficient for the players to dodge
* Gears dropped by a miniboss, and their abilities, should relate to their corresponding miniboss
* A miniboss may only be available after a certain floor, the same goes for their gears, done by rarity.



### Blue Lobster (Floor 1+)

An aggressive lobster with 2 claws. These claws may be detached from the lobster to perform certain attacks. Blue Lobster may only perform melee attack when its claws are intact.

It can also "jumpscare" players by teleporting to them, then immediately attacking.

#### Attacks

* Jumpscare: Emits a purple particle at where the player stands. After 0.8 seconds, teleports to the particle amd performs an attack with +80% attack.
* Clawler: Wait 1 secomnd, sends its claws forward, 1 towards the player's position, and 1 towards where the player is going.
* Clawspin: Positions its claws front-left and front-right of the player respectively, then the claws attack by performing a 360 degree spin.
* Shell Smash: After dropping below 50% HP, enters immune state, then breaks its shell and throws them at the player 1 second after. After this, permanently gains +100% attack, -50% defence and +30% speed.

Gears:

* Battle-Damaged Claw (Common): melee weapon that increases the player's attack and movement speed.
* Detached Shell (Common): chestplate that provides decent defence, increases crit rate.
* Blue Lobster's Shell (Epic): upgrade of Detached Shell, with an ability to increase the player's attack, movement speed, and reduces defence when below 50% HP
* Battle Claw (Epic): upgrade of Battle-Damaged Claw, with an ability to launch 2 claws forward and hit up to 5 enemy mobs on its way.
* Heat-seeking Claw (Mythical): upgrade to Battle Claw, where the ability is upgraded s.t. the 2 claws home towards the closest enemy mob.



### Vampress (Floor 2+)

A queen vampire who summons minions to assist her in battle.

* Major: Spawns a major that attacks the player
* Swarm: Spawns many mobs that attack the player
* Blood Spill: After dropping below 50% HP, enters immune state and signals. After \~10 seconds passed, kills all summons, then deals damage to everyone in 10 units radius, damage based on the amount killed. Gains atk, def, based on the amount killed. Enters phase 2 and attacks with a ranged weapon instead.
* Bloodrain: spawns projectiles like how she did when spawning swarm, but the projectiles actually damage.

Gears:

* Drained Crown (Common): Helmet that provides HP and Mana Regen
* Defunct Staff (Rare): Ranged weapon that provides Knockback
* Infused Staff (Epic): Upgrade of Defunct Staff, with additional ability that summons mobs to fight by the player's side
* Blood-Infused Crown (Legendary): Upgrade of Drained Crown, with additional ability that increases the player's Atk by how much damage they took or dealt
* Vampress' Staff (Mythical): Upgrade of Infused Staff, the ability comes with right clicking when the summoned mobs are active to quickly kill them and deal massive AoE damage based of number of them killed; longer time implies more damage

