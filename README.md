# Stellar Command

## Game Concept

Stellar Command is a sci-fi strategy game where players command a fleet across procedurally generated star systems. Players manage resources, build and upgrade ships, engage in tactical real-time battles, and navigate branching narrative arcs that shape the fate of their faction. The game blends strategic fleet management with story-driven decision-making.

## Tech Stack

| Component | Technology |
|-----------|------------|
| **Engine** | Unity (Universal Render Pipeline) |
| **Platform** | Windows (Win32 API for native integrations) |
| **Narrative System** | Naninovel (visual novel–style dialogue, branching story, cutscenes) |
| **Language** | C# |
| **Rendering** | URP with custom shader graphs for space environments |

### Why This Stack

- **Unity URP** — Lightweight, performant rendering suited for stylized space visuals with scalable quality settings.
- **Win32 API** — Native Windows hooks for system-tray integration, custom windowing, overlay notifications, and low-level input handling.
- **Naninovel** — Production-grade narrative engine that integrates directly into Unity, handling dialogue trees, character expressions, localization, and save/load for story state.

## Core Loop

1. **Explore** — Discover new star systems, scan for resources and anomalies.
2. **Build** — Construct and upgrade your fleet, research new technologies, manage crew.
3. **Engage** — Real-time tactical combat with pause-and-plan mechanics.
4. **Decide** — Story events powered by Naninovel present choices that affect faction relations, crew loyalty, and available missions.
5. **Expand** — Establish outposts, secure trade routes, and grow your influence across the sector.

## Development Phases

### Phase 1 — Foundation
- Unity project setup with URP pipeline
- Core scene structure and navigation framework
- Win32 API integration layer (windowing, system tray)
- Basic ship movement and camera controls

### Phase 2 — Combat & Systems
- Real-time tactical combat prototype
- Ship loadout and upgrade system
- Resource management and economy loop
- UI framework (HUD, menus, fleet overview)

### Phase 3 — Narrative Integration
- Naninovel setup and story scripting pipeline
- Branching dialogue system with consequence tracking
- Character portraits, expressions, and voice hook support
- Story-triggered events tied to gameplay state

### Phase 4 — Content & World
- Procedural star system generation
- Mission variety (combat, exploration, diplomacy, escort)
- Faction reputation system
- Crew management and loyalty mechanics

### Phase 5 — Polish & Release
- Visual polish (VFX, lighting, post-processing)
- Audio design and soundtrack integration
- Save/load system with story and fleet state persistence
- Performance optimization and playtesting
- Steam build and release pipeline

## License

TBD
