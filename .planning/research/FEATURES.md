# Feature Landscape

**Domain:** Idle self-improvement desktop game (habit tracking + idle RPG + desktop companion + visual novel)
**Researched:** 2026-03-21
**Confidence note:** WebSearch unavailable. All findings drawn from direct knowledge of genre titles (Habitica, Cookie Clicker, Clicker Heroes, AdVenture Capitalist, Neko Atsume, Shimeji, Desktop Goose, VNs via Ren'Py, Wallpaper Engine, Finch, SuperBetter, Forest) through knowledge cutoff August 2025. Confidence marked per section.

---

## Table Stakes

Features players expect in each genre. Missing = product feels incomplete or untrustworthy.

### Idle Resource Generation

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Persistent offline progress | Players check in once a day; if nothing happened while away the game feels dead | Low-Med | Need time-delta on load; cap offline time to prevent abuse (24h is standard) |
| Visible resource counters | Players must see numbers moving or accumulating or the idle loop has no feedback | Low | Currency/resource display in HUD, update every tick |
| Clear resource-to-power translation | Player needs to understand "I logged exercise → ship gets X fuel" or loop is opaque | Low | Tooltip or quest completion summary showing what was earned |
| Soft progress cap / diminishing returns | Prevents players from logging 50 habits in one day to break the game | Low | Daily quest limits or resource soft cap are both valid; pick one |
| Meaningful idle tick | Something must happen while the player is away (combat resolving, resources accruing) or "idle" is a lie | Med | Time-based simulation on resume rather than real-time if single-player |

**Confidence: HIGH** — These are fundamental to idle genre contracts established by Cookie Clicker (2013), AdVenture Capitalist, and all successors.

### Habit / Quest Completion Flow

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| One-tap (or one-click) check-in | Friction is the enemy of habit apps; logging must feel effortless | Low | Single button press with confirmation optional |
| Immediate reward feedback | Dopamine must fire at completion; delay kills motivation | Low | Particle effect, sound, number pop — minimum one of these |
| Daily reset / cadence clarity | Players need to know when quests refresh or they disengage | Low | Clear "resets at midnight" or session-based label |
| Quest list with status | What's done, what's pending, what was missed — must be visible at a glance | Low | Checked/unchecked list is sufficient for v1 |
| Streak or consistency tracking | The single most validated mechanic in habit apps (Duolingo, Habitica, Finch) | Med | Even a simple streak counter drives retention dramatically |
| Miss / skip acknowledgment | If a player misses a day, the game should handle it gracefully (no silent penalty) | Low | "You missed yesterday" message + reduced (not zero) consequence |

**Confidence: HIGH** — Validated by Habitica's decade of iteration, Duolingo's streak mechanics, and academic gamification research (Werbach 2012, Chou 2015 Octalysis).

### Desktop Wallpaper / Overlay App

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Non-intrusive presence | App must never steal focus or block window interactions | Low-Med | Win32 SetParent to WorkerW layer; no topmost window flag |
| Toggleable visibility | User must be able to pause/hide wallpaper without quitting the app | Low | System tray icon + hotkey are both expected |
| System tray icon + right-click menu | Standard Windows desktop app contract for persistent background processes | Low | Minimum: Show, Hide, Quit |
| Minimal CPU/GPU footprint when idle | Desktop wallpaper running at 60fps kills laptop batteries; users will uninstall | Med | Dynamic framerate: target 5-10fps when no interaction, 30fps during events |
| Does not reset other wallpapers on exit | Exit must restore previous desktop wallpaper or users feel violated | Low | Store previous wallpaper path in settings, restore on clean exit |
| Status glanceable without opening full UI | The whole point of wallpaper mode is ambient info — resource levels and ship state visible without interaction | Med | HUD overlay layer rendered on wallpaper, not requiring focus |

**Confidence: HIGH** — Validated by Wallpaper Engine (Steam, 160k reviews), Rainmeter ecosystem, and standard Windows desktop app conventions. Win32 WorkerW technique is well-documented.

### Visual Novel Relationship System

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Named, visually distinct characters | Players cannot form attachment to an unnamed or visually generic NPC | Low | Minimum: name, portrait, crew role |
| Relationship meter or indicator | Players must see progress toward relationship milestones or they lose motivation to engage | Low | Simple numeric or icon-based indicator; hidden or visible both work |
| Triggered dialogue on meaningful events | Character reacts to player completing habits, clearing combat, long absence | Med | Event-driven dialogue unlocks feel more alive than time-gated |
| Persistent relationship state | Progress must survive sessions; players feel betrayed if characters "forget" them | Low | Part of save system |
| Character-appropriate voice / tone | Each character must feel distinct in writing; generic dialogue kills immersion | Low (design) | This is a writing constraint not a code constraint |
| Unlock gates at relationship thresholds | New dialogue, scenes, or cosmetic unlocks at relationship milestones | Med | Gives players long-term goals beyond daily habits |

**Confidence: HIGH** — Standard VN conventions (Ren'Py community documentation, Doki Doki Literature Club, Fire Emblem Fates, Persona series relationship systems all follow this pattern).

### Self-Improvement / Productivity Gamification

| Feature | Why Expected | Complexity | Notes |
|---------|--------------|------------|-------|
| Category separation for habits | Exercise ≠ sleep ≠ work; players track these differently and want category filters | Low | Habit types already in project scope; ensure UI reflects categories |
| Historical view / journal | Players want to see their streak history; "did I do well this week?" is the core retention question | Med | Weekly summary is already in project scope (weekly self-report) |
| Positive reinforcement framing only | Self-improvement apps that punish miss days cause dropout (Habitica learned this the hard way) | Low (design) | Ship takes damage not health depletion; frame as "less optimal" not failure |
| Player agency over habit list | Pre-set habits feel constraining; users must be able to define their own | Med | Custom habit creation is standard Habitica feature; critical for retention |
| Transparency of game mechanic | Player must understand the habit → resource → power chain without guessing | Low | Onboarding tutorial or tooltip layer |

**Confidence: HIGH for table stakes status; MEDIUM for specific framing advice** — Positive reinforcement framing is backed by Habitica's public post-mortem on their damage system causing user loss, but specific mechanics are design judgment calls.

---

## Differentiators

Features that set Stellar Command apart. Not expected, but highly valued when present.

| Feature | Value Proposition | Complexity | Notes |
|---------|-------------------|------------|-------|
| Desktop wallpaper mode | Passive, ambient awareness of game state without launching a window — no competitor combines idle game + live wallpaper + habit tracker | High | The single biggest differentiator; worth the Win32 complexity |
| Habit → combat power chain | Your real discipline creates a more powerful ship — the game reflects your actual life — no competitor does this directly | Med | Core concept; requires clear in-game communication of the link |
| Crew relationship system tied to habits | Characters react to your real-life behavior, not just clicks — creates genuine emotional investment | High | Naninovel integration; no idle game does crew relationships this way |
| Weekly self-report with narrative framing | Not a "productivity review" but a captain's log — reframes self-reflection as roleplay | Med | Differentiating if the writing is good; generic if not |
| Resource types mapped to habit categories | Exercise → engines, sleep → shields, work → weapons creates mental model that reinforces which habits matter for what | Low-Med | Requires thoughtful mapping; if arbitrary it loses meaning |

**Confidence: MEDIUM** — Differentiator assessment is partly design judgment. Desktop wallpaper idle + habit tracking combination is genuinely novel as of knowledge cutoff.

---

## Anti-Features

Features to explicitly NOT build in v1.

| Anti-Feature | Why Avoid | What to Do Instead |
|--------------|-----------|-------------------|
| Automatic habit tracking / device sync | Adds privacy complexity, OS permission scope, unreliable data, and contradicts the manual check-in philosophy | Manual check-in stays; revisit post-v1 |
| Push notifications / OS alerts | Windows notification spam causes app uninstall; desktop wallpaper already provides ambient reminder | The wallpaper IS the notification; no additional alerts |
| Punishment for missed days (health depletion, character anger) | Habitica documented significant user loss from punishment mechanics; people who miss days already feel bad | Ship is "less optimal" not "damaged"; frame as opportunity cost |
| Social / leaderboard features | Solo experience by design; comparison causes demotivation for self-improvement apps | Remove from scope entirely |
| Complex combat player interaction | If combat requires skill or attention it stops being idle; loses the core value prop | Combat fully automated; player affects outcome only via habits |
| In-app store / monetization layer | Complicates v1 scope significantly; also conflicts with personal-use framing | Out of scope; revisit only if product direction changes |
| Multiple save slots | Single-player self-improvement game; multiple saves means playing as fictional characters, not yourself | Single save slot; backup/export is a separate utility concern |
| Auto-generated habit suggestions | Generic "drink water" suggestions feel hollow and impersonal | Let player define their own habits from the start |
| Real-time resource tick during active window | If player is in the game window, idle ticks feel pointless; interaction should feel intentional | Distinguish wallpaper mode (idle ticks) from active UI mode (manual input) |

**Confidence: HIGH** — Anti-features based on documented failures in genre (Habitica punishment system, notification fatigue research, idle game genre conventions).

---

## Feature Dependencies

```
Save/Load System
  → All persistent state (habits, resources, relationships, streaks)
  → Streak tracking requires save
  → Relationship state requires save
  → Offline progress calculation requires last-session timestamp in save

Settings System
  → Wallpaper mode toggle
  → Audio on/off (required before any sound events)
  → Daily reset time (midnight vs. custom)

Habit Quest System
  → Ship Resource System (habits feed resources)
  → Streak Tracking (habits feed streak counter)
  → Character Reaction Triggers (completed habits trigger VN events)
  → Weekly Self-Report (habit history feeds report)

Ship Resource System
  → Automated Combat System (resources determine combat power)
  → Wallpaper HUD Overlay (resource display on desktop)

Automated Combat System
  → Visual Feedback (something must show combat resolving)
  → Resource Consumption / Generation from combat

Visual Novel / Character System
  → Habit Quest System (triggers require habit events)
  → Save/Load (relationship state persisted)
  → Asset pipeline (portraits, backgrounds, dialogue scripts)

Desktop Wallpaper Mode
  → Settings System (toggle)
  → Ship Resource System (HUD display)
  → Win32 API integration (platform layer)

Weekly Self-Report
  → Habit Quest System (pulls history)
  → Save/Load (persists report results)
  → Resource Bonus System (report outcome feeds resources)
```

---

## MVP Recommendation

### Must have in v1 (table stakes for the concept to be coherent)

These are required for the game to make a first impression that matches its premise:

1. **Save/load with offline progress** — Without this, every session starts at zero. Non-negotiable.
2. **Habit quest list with one-click completion and immediate reward feedback** — The core interaction. If this feels bad, nothing else matters.
3. **Ship resource HUD** — Player must see the habit → resource translation visually on screen.
4. **Daily quest reset with clear timing** — Players must know when to come back.
5. **Streak counter (minimum: current streak number)** — The single highest-ROI retention mechanic relative to implementation cost.
6. **Wallpaper mode (basic)** — The primary differentiator. Even a static ship with resource overlay satisfies the promise.
7. **System tray icon with Hide/Show/Quit** — Required for a wallpaper app to be non-intrusive.
8. **At least one character with relationship meter and event-triggered dialogue** — Establishes the VN system; one character at full quality beats three at low quality.
9. **Settings: audio toggle, wallpaper mode toggle, daily reset time** — Minimum viable settings surface.
10. **Weekly self-report form** — Already in project scope; reinforces the self-improvement loop.

### Should have in v1 (differentiators worth the cost)

11. **Custom habit creation** — Pre-set habits undermine the personal nature of the product. Player-defined habits are critical for long-term engagement.
12. **Resource type → habit category mapping** — Exercise/sleep/work/other mapping to ship systems (engines/shields/weapons). Communicates the thematic connection.
13. **Miss acknowledgment (graceful handling)** — A simple "you missed yesterday" with no punishment, just information.

### Defer (post-v1)

| Feature | Reason to Defer |
|---------|-----------------|
| Animated combat visuals | Automated combat can be implied with text/simple effects in v1; full animation is polish |
| Multiple crew characters (beyond 1-2) | Writing quality per character matters more than quantity; ship with 3 weak characters worse than 1 strong |
| Habit history / calendar view | Weekly self-report covers this need adequately for v1 |
| Relationship unlock scenes / CGs | Relationship meter can exist without unlockable art in v1 |
| Framerate optimization for wallpaper | Build it working first; optimize in v1.1 when real perf data exists |
| Export / backup save | Useful but not habit-forming; defer until players ask for it |
| Character voice (audio) | High production cost, low v1 priority |

---

## Reference: Similar Products — What They Do Well / Poorly

### Habitica (habit tracking RPG, web/mobile)

**Does well:**
- Quest completion is a single tap with immediate party notification
- Habit categories (daily, to-do, habit) are clearly separated
- Character equipment visually reflects player stats (habit compliance)
- Party system makes habits social

**Does poorly:**
- Punishment mechanics (health damage for missed habits) cause dropout among struggling users
- RPG combat is click-based, not idle — active combat undermines the "your life powers your character" concept
- Desktop presence is zero — it's a web app / phone app; no ambient awareness
- Character art is generic pixel RPG; low emotional attachment
- Onboarding is overwhelming; new users face 20+ options before understanding the core loop

**Lesson for Stellar Command:** Steal Habitica's one-click completion and category structure. Reject punishment mechanics. The automated idle combat + desktop wallpaper solves everything Habitica lacks for a desktop-focused user.

### Cookie Clicker / Clicker Heroes / AdVenture Capitalist (idle/incremental genre)

**Does well:**
- Numbers going up is intrinsically satisfying at low implementation cost
- Milestone unlocks (new buildings, upgrades) create medium-term goals
- Offline progress catch-up makes returning feel rewarding
- Prestige/reset systems extend longevity significantly

**Does poorly:**
- No emotional narrative — purely mechanical; no reason to care beyond numbers
- No real-world feedback loop — there's nothing meaningful about clicking
- Metagame (prestige) can feel repetitive without narrative justification

**Lesson for Stellar Command:** Adopt offline progress and milestone unlocks. The habit loop provides the "real-world meaning" these games lack — lean into that contrast in the UX copy.

### Desktop Goose / Shimeji / Desktop Pets (Windows desktop companions)

**Does well:**
- Ambient presence without interrupting workflow is genuinely delightful
- Small interactions (clicking the character, occasional events) create attachment with minimal friction
- System tray management is clean in well-built examples

**Does poorly:**
- Most desktop pets have zero progression system — novelty wears off in days
- Performance impact is often ignored until users notice lag
- No content depth — same animations/behaviors loop after hours

**Lesson for Stellar Command:** The wallpaper ship needs meaningful state change over time (not just animation loops) or it becomes Desktop Goose with a spaceship skin. The habit system IS the state change. Make sure the wallpaper visually reflects progress — a more powerful/upgraded ship after 30 days of good habits.

### Wallpaper Engine (animated wallpaper platform, Steam)

**Does well:**
- Non-intrusive performance (adaptive FPS, pauses when fullscreen game detected)
- System tray integration is polished
- Workshop/community for content

**Does poorly:**
- Not a game; no progression; pure aesthetic
- No habit or self-improvement hook

**Lesson for Stellar Command:** Steal Wallpaper Engine's adaptive FPS approach for the wallpaper renderer. The community/workshop angle is out of scope for v1 but worth noting for future.

### Finch (self-care app, mobile)

**Does well:**
- Positive reinforcement only — no punishment
- Penguin companion "grows" based on self-care activity — emotional attachment to progress
- Daily check-in is low friction (mood + 2-3 tasks)
- Goal system is player-defined, not prescribed

**Does poorly:**
- No idle/game loop — it's a journaling/check-in app with a pet mascot
- Desktop presence is zero
- Progression plateaus quickly after the companion is "grown"

**Lesson for Stellar Command:** Finch proves the "companion grows with your self-care" concept has real market validation. Stellar Command is Finch with an actual idle game engine and a desktop presence. The SSV Ardent growing in power is the same emotional mechanic.

---

## Sources

- Genre knowledge: Cookie Clicker, AdVenture Capitalist, Clicker Heroes, Idle Legends — direct product analysis (knowledge cutoff Aug 2025)
- Habitica: habitica.com, public post-mortems on damage mechanics (documented in their blog, 2015-2020)
- Desktop companion apps: Desktop Goose (samperson.itch.io), Shimeji (2009 original), Wallpaper Engine (Steam product)
- Self-improvement apps: Finch (selfcare app), Forest, SuperBetter — feature analysis
- Win32 wallpaper technique: WorkerW window pattern, documented in Wallpaper Engine Reddit AMA and multiple devlog write-ups
- VN relationship systems: Persona 5 Social Links, Fire Emblem Fates supports, Doki Doki Literature Club — structural analysis
- Gamification theory: Werbach & Hunter "For the Win" (2012), Yu-kai Chou Octalysis Framework (2015) — HIGH confidence, widely cited
- **Confidence caveat:** All findings from training data; WebSearch unavailable for this session. Critical claims about Habitica punishment mechanic impact are MEDIUM confidence (documented in public blog posts but I cannot verify current URLs). All idle game table stakes are HIGH confidence from direct product knowledge.
