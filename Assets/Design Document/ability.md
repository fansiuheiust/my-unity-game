### Brief

Abilities are scripts that can be added to a mob.

They can either be passive or be triggered by a key (E: damage enemy, Q: charged ability, X: movement ability, C: buff self/debuff enemy).

Multiple abilities can share the same trigger key.

Ability can come from equipping a gear, or unlocking from Perk Tree.

Ability should be removed from a mob upon armor unequipment.



## Classes

AbilityObject: abstract monobehaviour logic of ability, derive from it to create new abilities

Ability: stores the stats and description for an ability by hash table similar to Perk Tree



#### Ability Database

Before game starts, make a dictionary of ability classes hashed by the class name using reflection

#### Common fields and methods shared by all AbilityObjects

Mob Owner

Method to disable the script during cooldown, and reenable it when cooldown ends

Method Init called by owner mob, which sets owner, and cooldown of the ability.

Abstract method called in Init to subscribe to events.

Abstract method called in Init to set fields based on the given Ability

#### Common fields and methods shared by perk tree abilties

PlayerPerk

abstract method called in Init which should be used to set data according to the perk tree



### TODO

Perk as abilities:

Assume perk cannot be deactivated. Unlocking perk gives player an ability, leveling up an ability removes and readds the ability, loading from save adds ability

