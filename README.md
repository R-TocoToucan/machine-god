# Stellar Command (SSV Ardent)

**Genre:** Idle / Self-Improvement / Visual Novel / Subculture  
**Platform:** Windows (Unity URP)

---

## Core Concept

You are the captain of the SSV Ardent, a massive warship pushing through hostile star systems. Combat runs fully automatically in the background — your job isn't to fight. Your job is to *live well*.

Complete real-life daily habits and quests to supply the ship with resources. Those resources fund upgrades, power auto-battles, clear stages, and unlock story content with a cast of characters who actually care whether you went to the gym today.

## Core Loop

```
Complete real-life quest
    → Earn ship resources (Energy Cells, Data Cores, Supplies)
        → Upgrade weapons & systems
            → Auto-battle progresses
                → Clear stage
                    → Unlock story / characters
```

## Key Systems

### 1. Daily Quest System

Players define their own habit goals — exercise 30 minutes, study 20 minutes, maintain a sleep routine, cook meals. Each habit maps to a ship resource type:

| Habit Category | Resource |
|---------------|----------|
| Exercise / Work | Energy Cells |
| Study / Learning | Data Cores |
| Sleep / Diet / Cooking | Supplies |

Quests are fully customizable. The game doesn't dictate what "self-improvement" means — you do.

### 2. Desktop Wallpaper Mode

The game lives on your desktop. Using Win32 API (`SetParent`, `SetWindowLong`), Stellar Command runs as an animated live wallpaper with a floating HUD overlay.

**Three display modes:**
- **Wallpaper** — Embedded behind desktop icons, always visible
- **Floating HUD** — Compact overlay showing ship status and quest progress
- **Fullscreen** — Traditional game view for story content and menus

### 3. Auto Combat

Battles are 100% automatic. No player input during combat. DPS and defense are determined by:
- Ship upgrade tier
- Daily quest completion rate

Miss your habits → ship underperforms. Stay consistent → watch it dominate.

### 4. Character Affection System

Four crew members, each with their own personality and linked gameplay systems. Earn affection points through quest completion and story choices to unlock Visual Novel episodes.

| Character | Role | Personality | Linked To |
|-----------|------|-------------|-----------|
| **Yuira** | Navigator | Serious, tsundere | Exercise / work quests |
| **Serina** | Engineer | Cheerful, playful | Ship upgrades |
| **Artemis** | Tactical AI | Cool → gradually humanizes | Combat / stage clears |
| **Minha** | Medical Officer | Warm, naggy | Sleep / diet / exercise quests |

### 5. Weekly Self-Report

Every Sunday evening, a popup asks the player to self-report:
- Takeout / fast food frequency
- Impulse spending

**Honest reporting is rewarded.** Over-reporting triggers a narrative "supply delay" penalty — the ship's logistics get disrupted. The system encourages accountability, not perfection.

### 6. Visual Novel Story

Story episodes unlock per stage clear. Full branching narrative with romance routes for each crew member, powered by Naninovel. Choices carry weight across the campaign.

## Tech Stack

| Component | Technology | Purpose |
|-----------|------------|---------|
| Engine | Unity URP | Lightweight rendering, stylized space visuals |
| Desktop Integration | Win32 API | Live wallpaper, floating HUD, system-level windowing |
| Narrative System | Naninovel | Branching dialogue, character expressions, VN episodes |
| Language | C# | All gameplay and systems logic |
| Platform | Windows first | macOS planned for later |

## Characters

### Yuira — Navigator
*"You skipped your run again, didn't you? ...I can tell."*

Serious and sharp, with a tsundere edge. She tracks the ship's course and doesn't tolerate slacking — but her concern runs deeper than she lets on. Linked to exercise and work-related quests.

### Serina — Engineer
*"Ooh, new parts! Let me see, let me see!"*

Endlessly cheerful and playful. Lives in the engine room and gets genuinely excited about every upgrade. Her enthusiasm is infectious. Linked to the ship upgrade system.

### Artemis — Tactical AI
*"Emotional response noted. Filing under: irrelevant. ...For now."*

Starts as a cold, efficient combat AI. Over time — through stage clears and story progression — begins developing something uncomfortably close to feelings. Linked to combat performance and stage clears.

### Minha — Medical Officer
*"Did you eat a real meal today? And no, instant ramen doesn't count."*

Warm, caring, and just a little naggy — like a big sister who worries too much. Monitors crew (and captain) wellbeing. Linked to sleep, diet, and exercise quests.

## License

TBD
