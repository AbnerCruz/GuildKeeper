
---

# 🏰 ProjectE - Guild Keeper

**Guild Keeper** is a medieval guild management simulator developed in **MonoGame**. Players take on the role of a Guild Master responsible for base building, recruiting disposable heroes, and managing the lifecycle of dungeons—which evolve from deadly threats into lucrative resource mines.

---

## 🎯 High Concept

Manage a guild where heroes are financial assets, and exploration doesn't end with the boss's death, but rather with the total exhaustion of the dungeon's natural resources.

## 💎 Design Pillars

* **Heroes as Investments:** *Permadeath* ensures that losing veteran heroes has a significant financial and strategic impact.
* **The Dungeon Cycle:** Combat (Risk)  Mining (Passive Reward)  Depletion (End).
* **Functional Aesthetics:** Base customization is not just visual; furniture and decorations provide essential passive buffs.

---

## ⚙️ Core Mechanics

### A. The Hub (The Guild)

* **System:** 2D Grid (Tilemap) with object slots.
* **Buff Logic:**
* **Bed:** HP/Stress recovery for idle heroes.
* **Strategy Table:** XP bonus on missions.
* **Decoration:** Passive stress mitigation.



### B. Adventurers (Assets)

* **Attributes:** Strength (Damage), Vitality (HP), and Speed (Mission time).
* **Stress System:** Upon reaching 100%, the hero enters *Burnout* (leaves the guild or stops working).
* **Traits:**
* 🟢 **Robust:** HP bonus.
* 🟢 **Greedy:** More gold in loot.
* 🔴 **Coward:** Flees if HP is below 20%.
* 🔴 **Drunkard:** Periodically consumes guild gold.



### C. Dungeons (Double Loop)

1. **Phase 1: Threat (Active):** Requires combat heroes. The goal is to defeat the Boss.
2. **Phase 2: Mine (Passive):** After victory, the dungeon becomes a mine. Requires allocating miners to extract iron and stone until resources are depleted (Idle).

---

## 🚀 Development Roadmap (MVP)

### Phase 1: "Invisible" Logic (Core Engine)

* [ ] Implementation of `Hero`, `Dungeon`, and `ResourceBank` classes.
* [ ] Procedural generator for Dungeon attributes.
* [ ] Mathematical combat system (Damage vs. HP) without interface.
* [ ] State conversion logic: `Dungeon (HP <= 0) -> Mine`.

### Phase 2: Visual Foundation (MonoGame Basic)

* [ ] Grid System rendering (10x10).
* [ ] Base UI: Recruitment and Construction buttons using `Rectangle`.
* [ ] Entity sprites and floating status bars (HP/Stress).

### Phase 3: Gameplay Loop

* [ ] Expedition Menu (Hero Selection -> Dispatch).
* [ ] Timer System for real-time missions.
* [ ] Miner allocation system.
* [ ] Furniture Shop with persistent buffs on the Grid.