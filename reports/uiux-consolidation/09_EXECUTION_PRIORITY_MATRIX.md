# 09 — EXECUTION PRIORITY MATRIX
**Goal:** identify the work that closes **~90% of the UI/UX crisis**, ranked P0→P3, with rationale, dependencies, and sequencing.

---

## 1. What "the crisis" actually is (and what 90% looks like)

The crisis the player experiences, in order of visibility:
1. **The front‑end looks broken** — doubled logos/titles, garbled buttons, bleed‑through (Report 04).
2. **Wrong identity** — realistic knights/orcs/dragons instead of stick figures (Report 02).
3. **The battlefield is primitive** — ECS shapes on a flat background (Report 06).
4. **Brand drift** — "Bulwark" everywhere vs the intended name (Report 01).

**Closing ~90% = make the screens the player sees most (boot → menu → mode select → match → results → shop) look clean, on‑identity, and un‑doubled, with a battlefield that reads as a stick war.** That is overwhelmingly a **UI‑architecture + top‑screen art** problem, not a "produce every asset" problem. The long tail (every secondary screen's bespoke key art, every biome, full roster, cosmetics) is the last 10% of *perceived* quality at a large share of the cost.

## 2. The matrix

### 🔴 P0 — closes the bulk of the crisis (do first; mostly blocking)
| Item | Why P0 | Report |
|---|---|---|
| **Legal/IP decision on the name** ("Stick Empire Rise" vs original) | Gates every brand surface + de‑risks the whole product; a *decision*, cheaply made now | 01 §0 |
| **Adopt the clean layered UI architecture** (remove dirty sources: no full‑mockup backdrops, no baked‑text chrome) | This *is* the "broken UI" fix — it makes doubling/bleed structurally impossible | 04, 05 |
| **Author the clean UI KIT** (frames, panels, buttons ×states, tabs, icons, currency pills — **no baked text**) + atlas | Replaces the contaminated chrome everywhere at once via the existing builder API | 05 U1 |
| **UI‑free background plates for the top ~8 screens** (Splash, Loading, MainMenu, ModeSelect, Store, Settings, Victory/Defeat) | Kills the most‑seen doubling; restores identity where it matters most | 03, 05 U2 |
| **Stick archetype sheet** (king, mage, archer, swordsman, spearman, miner + faction tints) | The single source the kit, plates, characters, and correction prompts all reuse → consistency | 08, 03 |
| **Remove the dual UI system** (legacy `UiFlow` front‑end) | Eliminates a whole bleed‑through vector; cheap | 04, 05 U4 |
| **Fix visible brand strings** (display name + live logo/splash text) — *after legal* | Cheap, high‑visibility rebrand win | 01 R1 |

### 🟠 P1 — the rest of the visible 90%
| Item | Why P1 | Report |
|---|---|---|
| **Battlefield parallax base + statues** (1 Siege biome) | The in‑match screen is the second‑most‑seen surface; statues = win condition readability | 06, 07 |
| **Shared stick rig + 5 core units** (swordsman, archer, spearman, miner, mage) | Makes the on‑field army real and on‑identity | 08 |
| **UI‑free plates for remaining screens** | Finishes removing baked‑UI backgrounds | 05 U2 |
| **TMP SDF font migration** | Crisp, localizable text (current legacy `Text` is a prestige gap) | 05 U3 |
| **Correct the high‑traffic monster screens** (MainMenu dragons, ModeSelect zombie, MatchIntro/Commander) | Removes the most‑seen identity violations | 02, 03 |
| **Device fidelity gate** (screenshot vs reference for all 35 screens) | Prevents regression of the crisis | 05 U6 |

### 🟡 P2 — depth & completeness
| Item | Report |
|---|---|
| Full unit roster + King/Commander hero + **stick‑undead / stick‑brute** | 08 |
| Additional biomes (Greenfield, Ashen, Frost) + weather/ambience | 06, 07 |
| Character/key‑art layer on hero screens (Profile, Commander, results) | 05 U5 |
| Correct remaining lower‑traffic monster screens | 02, 03 |
| First cosmetic skins (Skins/Store) | 08 |

### ⚪ P3 — polish / deferred
| Item | Report |
|---|---|
| Live day/night cycle, advanced post‑FX, foreground fly‑throughs | 06 |
| Decorative prop density, extra cosmetics/emotes | 07, 08 |
| **Deep code rename** (package id / namespaces) | 01 R4 (explicitly deferred / not recommended) |

## 3. Dependency graph (critical path)

```
Legal name decision ──► visible brand strings
Stick archetype sheet ──► UI kit ──► (re-skin chrome)
                      └─► background plates ──► (re-skin backgrounds)
                      └─► character rig ──► core units ──► battlefield
Clean architecture (Report 05) ──► all of the above land safely
TMP font asset ──► text migration
Device fidelity gate ──► guards every screen before "done"
```

The **archetype sheet** and the **clean UI kit** are the two true bottlenecks: almost everything visible depends on them. Fund those first.

## 4. Why this ordering (rationale)

- **Architecture before assets:** pouring more art onto the current dirty‑source pipeline would re‑create the doubling (Report 04). Fix the pipeline (P0) so every asset added afterwards lands clean.
- **Most‑seen surfaces first:** boot→menu→match→results→shop are ~90% of session screen‑time; perfecting a rarely‑seen modal's key art is P2/P3.
- **One shared source (archetype sheet) → consistency:** the kit, plates, characters, and correction prompts must all draw the *same* stick designs, or the game looks incoherent. Author it once, reuse everywhere.
- **Cheapest high‑visibility wins early:** removing the dual UI system and fixing brand strings are low‑effort, high‑perception P0/P1 items.
- **Defer the irreversible/low‑value:** deep code rename (Report 01 R4) and day/night cycle add cost/risk for little perceived gain — P3 or never.

## 5. The 90% statement

> Ship **P0 + P1** and the UI/UX crisis is ~90% closed: a clean, un‑doubled, on‑identity front‑end on every high‑traffic screen, a stick‑war battlefield with readable statues and a real 5‑unit army, crisp text, and a cleared brand — built on an architecture that makes the failure unable to recur. P2/P3 add depth and polish, not crisis relief.
