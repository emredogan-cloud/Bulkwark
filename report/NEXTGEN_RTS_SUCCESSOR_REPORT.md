# Next-Generation Direct-Control RTS — Successor Blueprint

**A strategic + technical vision report**, derived from the reconstruction in `STICK_WAR_MASTER_RECONSTRUCTION_DOSSIER.md`.
**Not** a remake, clone, or asset/IP reproduction. We extract *design, technical, product, and live-ops lessons* from a successful mobile RTS-lite and use them to architect a stronger, more modern, more varied original successor. Every lesson is traced to a recovered finding (cited as *[dossier §]*). Opinionated by design; tradeoffs explained.

---

## 1. Executive summary

The reconstruction revealed *why* a 2014-rooted stick-figure RTS is still shipping (v2026.1.787) and earning on Unity 6: a **tight, legible core loop** (mine → train → push → destroy the enemy statue), a **signature agency hook** (you can directly control individual units, not just deploy them), **performance-first systems** that run hundreds of units on mid-range phones, and a **mature, remote-tunable F2P/live-ops backbone** with a clever client-side currency-integrity model *[dossier §5–8]*.

It also revealed the ceilings: **one lane, no terrain, shallow tactical depth**; **reactive-heuristic AI** with no adaptation; a **client-authoritative economy** backstopped only by telemetry-and-obscuration; and a **dated monetization mix** (interstitials, opaque loot boxes) that modern players and regulators increasingly reject *[dossier §12–13]*.

**Successor philosophy (one sentence):** *Keep the agency and the legible loop; replace the shallow battlefield with a real tactical layer (terrain + formations + a counter system + a roguelite spell draft); make the economy server-authoritative and ethically cosmetic-led; and lead with PvE + async competition before ever paying for real-time netcode.* The result is a skill-expressive, fair, tactically deep RTS-lite — the open gap between Stick War's casual depth and Clash Royale's PvP collection grind.

---

## 2. Lessons from Stick War: Legacy

**Why it worked (evidence-grounded):**

| Strength | Recovered evidence | Why it retained players |
|---|---|---|
| **Direct unit control** | per-unit `*Controls`/`*Ai`; possess-and-micro a single unit *[§5.1]* | Converts "watch your army" into "I made that play" — agency is the hook, rare in mobile RTS |
| **Legible core loop** | single `Game.unity` scene; Miner→Gold→train→push→Statue *[§4, §9]* | Low cognitive load; one screen, one objective; instantly understandable |
| **Lane-readable combat** | `AiDistance` weights `dy×10` (same-row lock), statue priority ×15 *[§5.1]* | Targeting "reads" as a coherent front line; players predict outcomes |
| **Persistent progression** | Armory upgrade IDs (`swordwrath_sword`…), 1 UpgradePoint/level, cross-battle *[§6]* | Replaying levels has a meta-payoff; sense of permanent growth |
| **Performance-first AI** | randomized **3-sample** targeting (O(1)/unit), timestamp-throttled stance FSM (2 s/10 s) *[§5.1]* | Hundreds of units smooth on a 2022 mid-tier phone (≈660 MB RSS) → huge device reach |
| **Remote-tunable economy** | 3-tier `RemoteConfig→SO→literal` resolver; diminishing-returns `max(5, base−5·playCount)` *[§6]* | Live-ops can retune the whole economy without app updates; anti-grind protects pacing |
| **Cheap, fair competition** | async Tournament ladder, **no real-time netcode** *[§9]* | Zero matchmaking latency/cost; works on bad connections; cheap to operate |
| **Integrity without heavy server** | distributed **CRDT** per-device currency ledger + `ServerTimeManager` HTTP-Date anchor + obscured values *[§6, §8]* | Defends F2P economy cheaply against the common client attacks |
| **Small, expressive content** | ~9 units, 18 spells, **Spine 2D**, 16 scenes *[§9]* | Tiny content footprint, high readability, cheap to animate/extend |

**Where it now feels dated:**

- **Tactical ceiling:** one lane, **no terrain, no formations, no flanking**; the "counter system" is mostly flat stats + a few damage types (`NORMAL/FIRE/CRIT/POISON/VOLTAIC`) *[§5.2]*. Depth caps within hours.
- **AI is reactive, not adaptive:** three power-ratio thresholds (0.7/0.8/0.6) + situational booleans *[§5.1]*. Predictable; no learning, no encounter authoring depth.
- **Economy posture is fragile:** **client-authoritative** with **telemetry-only** tamper response (no client enforcement) *[§8]*; full save JSON is even logged to logcat *[§7]*. A modern title is expected to be server-authoritative on currency/progression.
- **Monetization is 2016-era:** interstitials interrupting menu flow, **opaque loot boxes**, gem gacha *[§6, §10-validation]*. Loot-box opacity is now a retention drag and a regulatory liability.
- **No social/seasonal spine:** async *ghost* tournament only; no clans, no ranked seasons, no co-op *[§9]*. Modern retention lives in social + seasonal cadence.
- **Sim tech doesn't scale up:** MonoBehaviour + manual pooling on the main thread *[§4]* is fine for the current scale but blocks richer/larger simulations, deterministic replays, and (eventually) real-time PvP.

---

## 3. Core design philosophy

| Verdict | Items | Rationale |
|---|---|---|
| **Keep (timeless)** | Direct unit/squad control; mine→train→push loop; statue/base objective; legible single-front framing; persistent upgrade meta; diminishing-returns anti-grind; 3-tier remote-config live-ops; CRDT currency ledger; trusted-time anchor; Spine 2D readability; async-first competition | These are *why it worked* and are independent of era; they are the franchise's actual DNA |
| **Evolve** | Combat (add terrain/formations/counters/synergy); AI (layered utility + influence maps + adaptive director); economy → **server-authoritative**; spells → **draftable roguelite loadout**; tournament → **ranked seasons + clans**; sim → **deterministic ECS hot-path** | The strengths have a low skill ceiling and a fragile trust model; modern depth + integrity require these upgrades |
| **Discard (legacy limitations)** | One static lane; flat stat-only counters; opaque loot boxes; interstitial interruptions; client-authoritative currency; telemetry-only anti-cheat as the *only* line; logging save state; client-random A/B bucketing | Dated, low-depth, or trust/UX/regulatory liabilities |

**Distinction that should guide every call:** *timeless design* = the player-facing fantasy and the live-ops/integrity primitives; *legacy limitation* = the shallow battlefield, the predatory/opaque monetization, and the client-trust economy. Preserve the former aggressively; replace the latter without nostalgia.

---

## 4. Combat evolution

**Recovered baseline** *[§5.2]*: a clean centralized `Unit.Damage` **modifier chain** (`×2.0` backstab, `×0.3` massive, `×0.2` reflect/lifesteal, `×0.1` statue shield, `×0` throttled small hits), a 5-member damage-type enum, status effects (freeze=timescale 0 for 1.3 s; burn 8/s/13 s), and arrow ballistics with statue/massive reductions. The *architecture* is good and worth keeping; the *inputs* are flat flags and there is no spatial dimension.

**Evolution — add tactical dimensions without breaking legibility:**

1. **From one lane to a shaped front.** Replace the single corridor with a **continuous battlefield with chokepoints, elevation, and cover**. Keep the front-line readability the original got from `dy×10` targeting, but make position *mean* something: high ground → +range/+accuracy; chokes → AoE value spikes; cover → the existing arrow-reduction concept becomes positional, not a flat constant.
2. **Formations & squads.** Promote the signature "control a unit" to "**command a squad**" with cohesion, facing, and spacing. The original's `backstab ×2.0` flag becomes **real positional flanking** (facing-derived), turning a binary into a skill expression. Loose vs tight formation trades AoE-vulnerability for melee-density.
3. **A genuine counter system, layered on the existing matrix.** Keep the damage-type enum but pair it with **armor classes** (light/heavy/shielded/structure) so the existing multipliers become a readable **type×armor matrix** (spears beat heavy/cavalry, archers shred light, casters punish clumps, shields negate frontal arrows). This is the depth the flat stat model lacks — and it's authored as data, not code.
4. **Spell synergy & counterplay.** Evolve the 18 consumable spells *[§6]* into **combo-able effects**: freeze→shatter (bonus vs frozen), oil→ignite, wall→funnel-into-AoE. Every strong spell needs a **telegraph + counter** (dodge, dispel, terrain) so casting is a read, not a button.
5. **Readable-but-deep.** Hold the line on clarity: distinct silhouettes (Spine), color-blind-safe faction/damage colors, on-screen telegraphs. Depth comes from *interactions*, not from stat soup or hidden numbers.

**Net:** the same elegant modifier-chain engine, now fed by **position, facing, terrain, armor type, and synergy** — turning a 30-minute-deep system into a hundreds-of-hours one while keeping the original's instant readability.

---

## 5. AI evolution

**Recovered baseline** *[§5.1]*: per-MonoBehaviour `Update` with a timestamp-throttled stance FSM (`BalanceOfPowersRatio` vs 0.7/0.8/0.6) and **randomized 3-sample** unit targeting. Brilliant for performance, weak on adaptivity and predictable to experienced players.

**Three-layer model — modern but performant, deliberately not over-engineered:**

1. **Commander layer (strategic, ~2 s cadence):** replace the three hard thresholds with a small **utility system** scoring a handful of signals — army-value ratio (keep `BalanceOfPowersRatio`), economy delta, threat proximity, statue HP, spell readiness, terrain ownership — and picking a stance/build by weighted utility. Still O(1), still fully RemoteConfig-tunable (weights as data), but produces *varied, situational* decisions instead of a fixed ladder.
2. **Squad layer (objective/formation):** holds a lane/chokepoint, flanks, retreats, focuses casters — the tactical brain the original lacked entirely.
3. **Unit layer (cheap):** keep the O(1) philosophy but replace random 3-sampling with **influence-map lookups** — a coarse grid of threat/value updated on a slow cadence; each unit reads its cell to pick targets/retreat. Same per-unit cost, far better decisions, no per-unit scans.

**Build the central tick scheduler the original *didn't* have** (ironically the thing Phase-1 wrongly imagined *[§5.3, §12]*): a **budgeted AI scheduler** ticking N agents/frame keeps frame time stable on mid-range phones — the right place to spend the engineering the original saved.

**Encounter & difficulty:** an **adaptive director** for PvE (extends `GetWaveRating(day)` with a *player-skill* input) scales intensity/composition to keep flow without rubber-banding feeling cheap. Replace the single `DifficultyToModifier [1.0/1.1/1.2]` *[§5.1]* with a **multi-axis** difficulty (economy rate, AI aggression weights, unit stats, spell access) so "Hard" changes *behavior*, not just HP. **Avoid overengineering:** no full GOAP/behaviour-trees-everywhere, no ML inference on-device — utility + influence maps + a director is the proven sweet spot for mobile RTS.

---

## 6. Economy + meta evolution

**Keep (the original got these right)** *[§6–8]*: dual currency (soft mined in-battle, premium for cosmetics/convenience), **persistent capped upgrades**, the **diminishing-returns anti-grind**, the **3-tier RemoteConfig resolver** for live tuning, the **CRDT distributed ledger**, the **trusted-time anchor**, login-streak/daily cadence, and a **Play-Pass-style premium track** (the build already carries `PlayPassEconomyConfig`).

**Fix the trust model:** make the **economy server-authoritative**. The original is client-authoritative with telemetry-only tamper response *[§8]* — acceptable in 2014, a liability for ranked/seasonal stakes now. Keep the CRDT ledger and obscured values as a **client cache + deterrence** layer, but **validate currency, progression, and ranked outcomes server-side** (deterministic sim replays, §7, make this cheap for competitive modes).

**Modernize monetization — transparent, cosmetic-led, no pay-to-win:**

| Lever | Design | Why |
|---|---|---|
| **Cosmetics-first** | Skins/recolors/VFX/banners/statue designs via the existing Spine skin system (the build already has `VOLTAIC/PYROBLAZE/Golden/_Disabled_ROYAL/ABYSS` tiers *[§9]*) | Fair revenue that doesn't touch balance; high-margin, infinitely extensible |
| **Battle pass** | Seasonal dual-track (free + premium), earned-by-play, cosmetic + convenience rewards | Predictable revenue, respects time, proven retention shape; extends the Play-Pass concept already present |
| **Direct currency w/ shown value** | Buy gems with transparent pricing; **no loot-box opacity** | Avoids regulatory/loot-box-disclosure risk and the trust erosion of gacha |
| **Rewarded ads = opt-in only** | Player chooses to watch for a defined reward (extends `gems_per_ad_watch=10` *[§6]*); **no interstitials interrupting battles** | The original's interstitial-heavy menu flow is a retention drag; opt-in respects the player |
| **Upgrades capped & earnable** | Premium buys cosmetics/time-skip/battle-pass, never raw power; ranked uses normalized stats | Skill-expressive, fair; protects competitive integrity |

**Meta & social spine (the biggest retention gap to close):**
- **Ranked async seasons** — evolve the async ghost tournament *[§9]* into a real **seasonal ladder** with leagues, rewards, and resets; async keeps it cheap and lag-free.
- **Clans / co-op** — clan challenges, shared seasonal goals, async clan wars; social is the strongest retention multiplier the original entirely lacks.
- **Roguelite spell draft** — turn the consumable spell inventory into a **between-battle loadout draft** (pick 3 of N, with synergies), borrowing survivor-like build-craft dopamine while keeping RTS agency.
- **Events** — the build already ships seasonal remnants (`xmasjingle`, ChristmasTree reward) *[§9]*; make rotating events a first-class, data-driven cadence.

**What modern players tolerate (critical evaluation):** energy/stamina hard-gates → *avoid* (the original wisely has none — keep it); opaque gacha → *declining tolerance + regulatory risk* → lean cosmetic/battle-pass; ad interruptions → *only opt-in rewarded*; P2W power → *toxic to skill-based competition* → cap it. The discipline cost is real: **cosmetic + battle-pass revenue must carry the title**, which constrains ARPU and demands strong cosmetic content velocity (see §11 risk).

---

## 7. Technical architecture

| Concern | Original *[dossier]* | Modern successor | Rationale |
|---|---|---|---|
| Engine | Unity 6000.0.59f2, IL2CPP | Unity 6 LTS, IL2CPP | Proven; keep |
| Simulation | MonoBehaviour + manual pooling, main thread *[§4]* | **Unity ECS/DOTS deterministic sim** for the battle hot-path; MonoBehaviour for UI/meta | Scales unit count, enables **deterministic replays + server validation + future real-time PvP**; the key tech upgrade |
| Render | URP 2D, GLES3/Vulkan, Spine | URP 2D + Spine, GPU instancing for crowds | Keep readability; instancing for larger armies |
| Content | Addressables 2.7.6, **local-only** (PAD) *[§9]* | Addressables + **remote catalog/CDN** | Live events/cosmetics need remote content; original can't ship content without app updates |
| Balance data | ScriptableObjects (typetree-stripped) + RC overrides *[§6]* | **Data-as-config**: typed balance authored as data, exported to a config service; server-validated | Keeps the great 3-tier resolver but makes values typed, versioned, and server-trusted |
| Economy auth | **Client-authoritative** + CRDT + ServerTime *[§6–8]* | **Server-authoritative** economy/progression; CRDT ledger as client cache | Closes the trust gap for ranked/seasonal stakes |
| Anti-cheat | ACTk obscured values + telemetry-only *[§8]* | Client obscuration (deterrence) **+ server replay validation** for ranked | Detect-and-report on client, **decide-and-punish on server** |
| Live-ops | Firebase RemoteConfig (client-random A/B) *[§6]* | RemoteConfig-as-code + **server-side experiment assignment** | Proper bucketing vs the original's crude `Random<0.5` |
| Analytics | Triple stack (Firebase + Unity + Tenjin) *[§3]* | **One consolidated pipeline** + attribution | Reduce overhead/duplication |
| Cloud save | Play Games Snapshots, client merge *[§7]* | Server-owned profile; platform saves as backup; **don't log save JSON** | Authoritative + fixes the original's logcat save-state leak |
| Time | `ServerTimeManager` HTTP-Date *[§8]* | Keep + server time for authoritative events | Cheap, robust anti-rollback; keep |
| CI/CD | (hand-built pipeline) | Automated build/test, content-bundle CI, feature flags as first-class | Faster, safer live-ops cadence |
| Privacy | UMP consent, AD_ID *[§3]* | UMP/consent kept; minimize PII; data-deletion paths | Regulatory baseline |

**Principle:** adopt the original's *excellent* primitives (Spine, Addressables, 3-tier remote config, CRDT ledger, time anchor) and upgrade exactly the two things that limit it — **a deterministic ECS sim** (depth + scale + validation) and **server authority** (trust). Resist real-time-PvP netcode until the deterministic sim makes it cheap; it is the single biggest money pit in mobile RTS.

---

## 8. Content + world-building

Variety is the antidote to the original's "one battlefield forever" fatigue.

- **Asymmetric factions (3–4).** The original has Order/Chaos *[§9]*; a successor wants distinct *kits and playstyles*: e.g., a disciplined Legion (formations/shields), a Horde (swarm/expendable), an Arcane order (caster-centric, fragile), a Mechanized faction (siege/structures). Asymmetry = replayability + ranked diversity.
- **Biomes with mechanical teeth.** Desert/forest/snow/volcanic that *interact* with the §4 terrain layer (snow = freeze synergy, forest = cover/ambush, volcanic = burn synergy). Aesthetics *and* tactics.
- **Light narrative framing.** A faction-war arc that justifies campaign progression and gives the world identity — the original's campaign is mechanically fine but narratively thin.
- **Hero/commander identity.** Named commanders with signature abilities give a **collection loop and personality**; cap their power (cosmetic + light utility, not raw stats) to protect fairness.
- **Collectible cosmetics.** Skins/banners/emotes/statue designs via the Spine skin system; a fair, deep collection layer that funds the game without touching balance.

**Progression fantasy:** "rise from a single squad to commanding a faction army." Persistent upgrades (kept) + heroes + cosmetics + ranked rank make growth visible across battle, meta, and identity axes.

---

## 9. Mobile UX + accessibility

**Where older mobile RTS UX struggles (and the original's specifics):** single-lane direct control is fiddly under a thumb; onboarding is a tutorial-event funnel *[§6 tutorial_* events]*; no accessibility features observed.

- **Controls:** keep the signature direct control but make it *forgiving* — tap-to-command squads, drag-to-direct a push, long-press to possess a single unit; smart auto-target with manual override. One-handed portrait option for short sessions.
- **Readability:** distinct silhouettes, **color-blind-safe** faction/damage palettes (the game leans hard on color), clear AoE/telegraph indicators, damage-number toggles.
- **Onboarding:** playable tutorial woven into the first 2–3 battles (keep the funnel instrumentation, lose the hand-holding); teach the counter system through guided wins.
- **Session design:** 2–5 minute battles, instant suspend/resume (keep the original's frequent autosave + 5 s cloud cadence *[§7]*), no forced mid-battle ads.
- **Accessibility:** scalable UI, reduced-motion mode, haptics as readability cues, VO subtitles (Jason-style VO exists *[§9]*), and difficulty options that change *behavior* not just HP (§5).

---

## 10. Competitive landscape (analysis, not market fluff)

- **vs Stick War: Legacy** — differentiate on **tactical depth** (terrain/formations/counters), a **social/seasonal spine**, and **ethical monetization**, while keeping its agency hook. We are competing with the genre's own ceiling, not avoiding it.
- **vs Clash Royale** — CR is real-time card-deploy arena PvP; its strengths are collection + ladder + tight 3-minute duels, its costs are heavy netcode + matchmaking + a deck-collection power treadmill. We differentiate via **agency** (you *control* units, not just deploy cards) and **PvE depth** (campaign/survival/roguelite). **Do not** chase CR's real-time-PvP-first model; lead async + PvE and add real-time later if the deterministic sim earns it.
- **vs survivor-likes (Vampire Survivors / Survivor.io)** — their retention = run-based build-craft + short-session dopamine. Borrow the **roguelite spell-draft meta** (§6) and short sessions, but keep RTS agency and a persistent army instead of a single avatar.
- **vs SLG/4X (Rise of Kingdoms etc.)** — whale-driven, P2W, slow. That market is lucrative but predatory and crowded; deliberately **position opposite**: skill-expressive, fair, fast, tactically deep. The open niche is "Stick War's accessibility + real tactics + fairness."

**Strategic read:** the unoccupied space is a **fair, skill-expressive, direct-control tactical RTS-lite with PvE depth and async competition** — exactly what the original's strengths point toward and its limitations leave open.

---

## 11. Production reality

| | MVP (indie-feasible) | Full vision (studio-scale) |
|---|---|---|
| **Scope** | Direct control; one evolved battlefield (terrain + chokes); formations; type×armor counter matrix; ~12 units / 2 factions; utility AI + influence map + budgeted scheduler; campaign + endless + async ladder; roguelite spell draft; battle pass + cosmetics; **server-authoritative economy-lite** | 4 asymmetric factions; heroes; biomes; clans + clan wars; ranked seasons; deterministic-sim replays; optional real-time skirmish; full live-ops/events |
| **Team** | ~6–10 (2–3 eng, 2 art/anim w/ Spine, 1 design, 1 live-ops/backend, QA) | ~25–40 (ECS/sim, backend, client, 6–8 art, design, live-ops, data, QA) |
| **Timeline** | 9–15 months to soft-launch | 24–36 months to full vision |
| **Tech reuse** | Borrow the original's proven choices (Unity, Spine, Addressables, RC 3-tier, CRDT ledger, time anchor) to de-risk | Add ECS deterministic sim, server authority, remote content, experiment platform |

**Risks (ranked):**
1. **Balancing combinatorial depth** — terrain × formations × type/armor × spell synergy is a tuning explosion. *Mitigation:* the original's RemoteConfig + analytics model is exactly the live-tuning backbone needed; budget for telemetry-driven balance from day one.
2. **Cosmetic-only revenue discipline** — fair monetization caps ARPU; the game must produce **high cosmetic content velocity** to fund itself. *Mitigation:* Spine recolor/VFX pipeline is cheap; battle pass smooths revenue. Still the top commercial risk.
3. **Deterministic ECS sim complexity** — powerful but a real engineering lift. *Mitigation:* keep MonoBehaviour for meta/UI; ECS only for the battle hot-path; don't gate MVP on replays.
4. **Real-time PvP temptation** — netcode + matchmaking is a money pit. *Mitigation:* defer; async-first; only build it once the deterministic sim makes lockstep cheap.
5. **Content cost (factions × biomes × cosmetics)** — the long pole for the full vision. *Mitigation:* data-driven content pipeline; ship factions/biomes as live-ops over time.

**Indie vs studio line:** the **MVP is genuinely indie-feasible** by *borrowing the original's proven, low-cost architecture* and adding one combat layer + ethical monetization. The **social/seasonal/real-time/heroes/biomes vision is studio-scale** and should be earned with a successful MVP, not front-loaded.

---

## 12. Recommended direction (opinionated)

**What I would build:** a **direct-control tactical RTS-lite — PvE-and-async-first, cosmetic-monetized, with a roguelite spell-draft meta and a real terrain/formation/counter combat layer.**

- **Lead with single-player + async competition.** It's cheap, fair, lag-free, and differentiating; it plays directly to the original's proven async-tournament and PvE strengths *[§9]* while dodging the genre's most expensive trap (real-time netcode).
- **Keep the franchise DNA:** direct control, mine→train→push, persistent upgrades, the 3-tier live-ops resolver, the CRDT ledger, the time anchor, Spine readability — all recovered as *the* reasons it works *[§2]*.
- **Spend the engineering exactly where the original is thin:** a real **tactical combat layer** (terrain/formations/counters/synergy), **layered utility AI + influence maps + a budgeted scheduler**, and a **deterministic ECS sim** that future-proofs depth, scale, and validation.
- **Fix the two trust/UX liabilities decisively:** **server-authoritative economy** (not client + telemetry) and **transparent cosmetic/battle-pass monetization** (no loot-box opacity, no interstitial interruptions).

**Why this and not the alternatives:** a CR-style real-time-PvP-first product is costlier and crowded; an SLG/whale model is predatory and off-brand; a pure survivor-like throws away the agency that is the original's signature. The recommended direction **maximizes the recovered strengths, repairs the recovered weaknesses, avoids the expensive/predatory traps, and is buildable incrementally** — MVP first, vision earned. That is the highest-expected-value successor the dossier evidence supports.

---

## 13. Appendix — Original lesson → Modern successor response

| # | Original lesson (dossier §) | Modern successor response |
|---|---|---|
| 1 | Direct unit control is the agency hook *[§5.1]* | Keep; extend to squad command + possess; forgiving touch controls |
| 2 | Legible single-front loop *[§4]* | Keep the front, add terrain/chokes/elevation for depth |
| 3 | `dy×10` lane-readable targeting *[§5.1]* | Keep readability via influence maps; add formations/flanking |
| 4 | Flat stat counters + 5 damage types *[§5.2]* | Type × **armor-class** counter matrix (data-authored) |
| 5 | Clean centralized damage modifier chain *[§5.2]* | Keep the engine; feed it position/facing/terrain/synergy |
| 6 | 3-sample O(1) targeting; throttled stance FSM *[§5.1]* | Keep O(1); utility commander + influence-map units + **budgeted scheduler** |
| 7 | Single `DifficultyToModifier` *[§5.1]* | Multi-axis difficulty (eco/aggression/stats/spell access) + adaptive director |
| 8 | Persistent capped upgrades *[§6]* | Keep; add heroes (capped power) + cosmetic collection |
| 9 | Consumable spell inventory *[§6, §9]* | **Roguelite between-battle spell draft** with synergies |
| 10 | 3-tier RemoteConfig resolver + diminishing-returns *[§6]* | Keep; values **server-validated**; server-side A/B |
| 11 | CRDT distributed currency ledger *[§6]* | Keep as client cache; **server-authoritative** source of truth |
| 12 | `ServerTimeManager` HTTP-Date anchor *[§8]* | Keep for anti-rollback; server time for ranked events |
| 13 | Telemetry-only anti-cheat *[§8]* | Client deterrence **+ server replay validation** (ranked) |
| 14 | Interstitials + opaque loot boxes *[§6]* | Opt-in rewarded only; **transparent cosmetic + battle pass**, no gacha opacity |
| 15 | Async ghost tournament; no social *[§9]* | Ranked async **seasons + clans/co-op** |
| 16 | MonoBehaviour main-thread sim *[§4]* | **Deterministic ECS hot-path** (replays, scale, future real-time) |
| 17 | Local-only content (PAD) *[§9]* | Remote catalog/CDN for live events & cosmetics |
| 18 | Save JSON logged to logcat *[§7]* | Server-owned profile; never log save state |

---

*Synthesis only — no reverse engineering performed in this task. All design recommendations are derived from, and cited to, the reconstruction dossier; the successor is an original design that reuses **principles and techniques**, not assets, code, or IP.*
