
# Dungeon Game: Renamed
A solo dungeon crawler project inspired by Hypixel Skyblock Dungeon, rebuilt from scratch from my version used for a school's Open Day.

## Demonstration

### Dungeon Procedural Generation
Structure of a typical dungeon generated: (green: starting room; red: final room; blue connection: optional rooms)
![Small room](Demos/dungeon_demo_small.png)

Structure of a big generated dungeon
![Medium room](Demos/dungeon_demo_medium.png)

Structure of an even bigger dungeon
![Large room](Demos/dungeon_demo_large.png)

### Puzzles
Jump Game Description
![Jump Game Desc](Demos/jump_game_description.png)

Jump Game Playthrough
![Jump Game Demo](Demos/jump_game_demo.gif)

Light as Steel Description
![Light as Steel Desc](Demos/light_as_steel_description.png)

Light as Steel Playthrough (sending a train up)
![Light as Steel Demo 1](Demos/light_as_steel_jump.gif)

Light as Steel Playthrough (collision)
![Light as Steel Demo 2](Demos/light_as_steel_collision.gif)

### Minibosses
Vampress Summoning Swarm
![Vampress Summoning](Demos/vampress_spawn.gif)

Vampress Phase Change (player must kill enough swarms, or Vampress does a deadly attack, and each captured swarm buffs the Vampress)
![Vampress Phase Change](Demos/vampress_phase_change.gif)

Vampress Second Phase Attack: Blood Rain
![Vampress Blood Rain](Demos/vampress_blood_rain.gif)

## List of features

### Combat
- [x] Floating capsule-based movement
- [x] Stats
- [x] Collision-based melee weapons
- [x] Collision-based blocking
- [x] Basic hostile melee mob behaviour
- [ ] Navmesh for elite mobs navigation
- [x] Ranged weapons
- [x] Effects
- [ ] Abilities
- [ ] Melee weapon swing movement presets
- [ ] Boss

### Dungeon
- [x] Block-based main path generator
- [x] Side path generator
- [x] Rooms of any shapes where all blocks are next to each other horizontally XOR vertically
- [x] Layering downward when the main path gets trapped
- [ ] Room behaviours
- [ ] All room types
- [x] Dungeon Builder

### Building blocks
- [x] Basic interactables
- [x] Blocks that can disappear

### Progression
- [x] Perk tree
- [x] Coins to unlock perks
- [x] Permanently save coins
- [ ] Effects of Perk tree
