# Puzzle Room: A Breadth-first Challenge for Players

Puzzle rooms feature many puzzles (still 1 puzzle per room) to test the player. Player must pass the puzzle to gain rewards. The more optimal a player can solve puzzles, the more they can gain. In most cases.



### Room Structure

Puzzle rooms should appear the same when the player enters it: an interactable object at the center to start the challenge. If a puzzle changes the room's terrain, it should happen after the challenge starts



### Puzzles

There should be many puzzles, spanning in diverse areas <- which is not yet achieved. Here are some examples:

* Math: given n numbers, n-1 operators, find the result within 10 seconds. Faster solving implies better rewards, m is treated as the player taking 2^m times longer to solve. (afterword: nothing special about this, it is just a sample puzzle after all)
* Counting Game: 5 seconds of gameplay, (+-) (same color) (\*/)(same color) operators attached to a value appear on screen every second, expiring on the next second. Clicking will cause player score to <operator>= <value> (+=, -=, \*=, /=). Player must gain at least half of best solution to pass. (afterword: despite its genericity, it turned out to be quite a challenging game, requiring quick comparison of + vs \*)
* "Light" as Steel: in lanes forming a shape of #, trains will pass either left to right or top to bottom. Ramps are 1-use objects on a track that send a train flying over a few lanes. Goal: make all trains go to the opposite lane without collision. Collision happens when a ground train hits another ground train, or a flying train hits another flying train. (afterword: my first gimmicky puzzle, tough but cheese-able by spamming ramps on 1 lane. A ramp charging system of 2 ramps/wave of train capped at 5 should be introduced)
* Jump Game V: Refer to Leetcode. Given n horizontally placed towers, player is at index i, and they can only jump to another shorter tower with no tower taller than current index between, at most a distance d. Player must make at least half of optimal jumps to pass. (afterword: it was originally too trivial, but after increasing minimum number of jumps, it did turn out to be the challenge)
* Beamteract: Beam emitter of 3 distinct colors on the left, receiver of derangement of the 3 distinct colors on the right. Every interval, a random emitter will receive +1 charge, capped at like 5. Receiving beyond charge wastes the charge. Clicking emitter causes it to send charge (as beam) to the receiver of the same color once per interval. If 2 beams cross, charge will be destroyed. Goal: send as many charges to the receiver as possible.

Eventually, there should be like 50 to 100 games. But most may just turn into UI-slop instead of changing how the puzzle room looks like physically.



### Rewards

As seen in puzzles, most are just be half as optimal to pass. For those, variation of reward to how optimal the player is should be linear towards 100% when you are 100% optimal. For time-limited puzzles, the "optimal" time would just be half of the puzzle's time limit.

Three rewards may be harvested from puzzles: coins, buffs, and gears. They vary to optimality (how close to optimal the player is).

* 50%: Coins can be obtained
* 75%: add 1 buff to the rewards
* 100%/90%/85%/75%: Player can choose the obtained coin type (requires corresponding RNG perk)
* 100%: obtain 1 gear (more customization of the gear requires corresponding RNG perks, includes: rarity guarantee, choice of gear type, n choose 1 instead of 1 choose 1)

