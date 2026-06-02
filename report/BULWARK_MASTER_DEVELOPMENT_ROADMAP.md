# BULWARK — Master Development Roadmap & Production Constitution

**Status: CANONICAL.** This is the single source of truth for the BULWARK project: production constitution, execution manual, phase system, and Claude-agent operating guide. It reconciles and **overrides** casual ideation and any conflicting statement in earlier documents. Lineage: `STICK_WAR_MASTER_RECONSTRUCTION_DOSSIER.md` (evidence) → `NEXTGEN_RTS_SUCCESSOR_REPORT.md` (vision) → `NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md` + `PRODUCTION_DECISION_LOG.md` (plan) → **this roadmap** (law). Changelog: `ROADMAP_CHANGELOG.md`.

**Disposition tags used throughout:** **[PRESERVE]** keep a recovered Stick War strength · **[MODERNIZE]** evolve it · **[REPLACE]** swap a dated approach for a new one · **[CUT]** forbidden. Every disposition states *why*.

---

## 1. Executive overview

**What this is.** A phase-driven development system detailed enough that a Claude CLI production agent can build BULWARK end-to-end using **only** this document, without ambiguity, hallucination, or quality drift.

**Authority rules (binding):**
1. **This roadmap is law.** If anything (a prompt, an idea, an older doc) conflicts with it, the roadmap wins.
2. **Canon is closed** (Sections 2–12). New mechanics, units, currencies, or systems may **not** be invented mid-build. Changes require an **ADR** (Architecture/Design Decision Record) approved per the governance hierarchy (§16).
3. **Phases are gated** (§13). A phase may not start until its dependencies' **exit criteria** pass. Gates can **kill or rescope**.
4. **Inviolable constraints** (never traded away): **readability**, **fairness/no-P2W**, **session respect** (no dark patterns), and the **CUT list** (§15).
5. **When ambiguous or under-specified, the agent STOPS and asks** — it does not invent. (§15)

**Scope.** Mobile-first tactical RTS-lite (Android/iOS, Unity 6). MVP → soft launch → live. Post-launch features are explicitly deferred (§13 Phase 7, decision log).

**Production philosophy.** Preserve the decade-proven core that the reconstruction recovered; modernize exactly the two things that limited the original (shallow battlefield, fragile trust model); monetize ethically; build lean and gate hard. Quality > scope. Fun-gate before meta spend; LTV-gate before scale.

---

## 2. Project canon

| Field | Canonical value |
|---|---|
| **Title (codename)** | **BULWARK** |
| **Genre** | Direct-control tactical RTS-lite — single-front lane skirmisher; PvE + async competitive |
| **Audience** | 18–34 mobile strategy/action players; Stick War / lane-RTS / autobattler / survivor-like crossover seeking **agency + tactics + fairness**; secondary lapsed PC-RTS players |
| **Platform** | Android + iOS (landscape primary, portrait-capable); Unity 6 LTS. PC = full-vision option only |
| **Session** | 2–5 min battles; suspend/resume; one-handed-capable |
| **Pillars** | **P1 Agency** · **P2 Readable depth** · **P3 Fair mastery** · **P4 Respect the player** |
| **Visual philosophy** | Clean stylized 2D, bold readable silhouettes, Spine skeletal animation; **readability over detail** |
| **Monetization philosophy** | **Cosmetic + battle-pass led**; transparent; opt-in rewarded ads only; **never sells power** |
| **Fairness philosophy** | Skill > spend; upgrades capped; ranked normalized; cosmetics gameplay-safe |
| **Anti-identity (NON-GOALS)** | NOT real-time-PvP-first (Clash Royale); NOT whale SLG/4X; NOT passive autobattler; NOT gacha/loot-box; NOT P2W; NOT energy-gated |

**Non-goals are enforced as hard constraints**, not preferences (see §15 CUT list).

---

## 3. Legacy preservation layer (recovered Stick War strengths)

These are the recovered reasons the original retained for a decade *(dossier §2, §4–9)*. **[PRESERVE]** all; modern preservation strategy specified.

| System | Recovered origin (dossier) | Why it worked | Modern preservation strategy |
|---|---|---|---|
| **Mine → train → push → statue loop** | single `Game.unity`; Miner→Gold→units→Statue *[§4]* | legible, instantly understood, one objective | Kept verbatim as the core loop; terrain/counters add depth *around* it, never replace it |
| **Direct unit control** | per-unit `*Controls`/`*Ai`; possess-and-micro *[§5.1]* | the agency hook — "I made that play" | Kept + extended to squad command + commander ability (§4 modernize) |
| **Readable lane combat** | `AiDistance` `dy×10` row-lock, statue priority ×15 *[§5.1]* | coherent front line; predictable outcomes | Kept via influence-map targeting + same-row preference; readability is inviolable |
| **Persistent progression** | Armory upgrade IDs, 1 UpgradePoint/level, cross-battle *[§6]* | permanent growth; replay payoff | Kept as **capped** per-unit upgrade tracks (anti-P2W) |
| **Campaign** | 16 scenes incl. CampaignMenu/Map/Game *[§9]* | structured PvE progression fantasy | Kept; ~20-level MVP act; light narrative framing added |
| **Tournament (async)** | async ghost ladder, no real-time netcode *[§9]* | cheap, lag-free competition | Kept as async ladder → evolves to ranked seasons (post-launch) |
| **Endless (survival)** | EndlessDeads `GetWaveRating(day)` wave scaler *[§5.1]* | high-replay survival loop | Kept; adaptive director added (modernize) |
| **Spell persistence** | consumable spell inventory + distributed item ledger *[§6, §4.6b]* | meta-relevant tactical resource | Kept; evolved to a **draft-3 roguelite loadout** (§4 modernize) |
| **Performance-first systems** | 3-sample O(1) targeting; throttled FSM; hundreds of units on mid phones *[§5.1]* | huge device reach | Kept as a design constraint; ECS + influence maps preserve O(1)/unit |
| **Live-tunable economy** | 3-tier RemoteConfig resolver; diminishing-returns rewards *[§6]* | retune without app updates; anti-grind | Kept verbatim (server-validated values) |
| **Cheap integrity** | CRDT currency ledger + HTTP-Date time anchor *[§6, §8]* | defends F2P economy cheaply | Kept as client cache under server authority (modernize) |
| **Small, expressive content** | ~9 units, 18 spells, Spine 2D, 16 scenes *[§9]* | low content cost, high readability | Kept as a discipline: restrained roster, shared skeletons |

---

## 4. Modernization layer

Two limitations justified every modernization *(dossier §2)*: **shallow battlefield** and **fragile client-trust economy**. Each change below states why it exists.

**Combat:**
| Change | Disposition | Why |
|---|---|---|
| Terrain (high ground / choke / cover / hazard) | [MODERNIZE] | original had none → low tactical depth |
| Positional flank/back multipliers (1.5/2.0) | [MODERNIZE] | replaces the original's binary `backstab` flag with skill geometry |
| Formations (Line/Tight/Loose) | [MODERNIZE] | adds a tactical lever the original lacked |
| Type×armor counter matrix (5×4) | [MODERNIZE] | original counters were flat stats + 5 damage types only |
| Layered AI (utility commander + influence-map units + budgeted scheduler) | [MODERNIZE] | original was reactive thresholds; this is adaptive yet O(1) |
| Commander (1 active + passive, capped power) | [MODERNIZE] | original identity was thin |
| Spell synergy + telegraph/counter | [MODERNIZE] | original spells were point effects |

**World:**
| Change | Disposition | Why |
|---|---|---|
| Redesigned **Statue** (objective + shield phase + readable damage states) | [MODERNIZE] | keep the iconic objective; make it a clearer climax |
| Redesigned **Mines** (fixed nodes, miner-cap, contestable) | [MODERNIZE] | economy node becomes a positional contest, not a static tap |
| Richer battlefield + cleaner VFX + modern UX | [MODERNIZE] | readability + feel; mobile UX upgrades |

**Technology:**
| Change | Disposition | Why |
|---|---|---|
| ECS/DOTS battle-sim hot-path | [MODERNIZE] | original main-thread MonoBehaviour sim caps scale; ECS enables scale + replays + future netcode |
| Server-authoritative economy | [REPLACE] | original was client-authoritative + telemetry-only — a trust gap for ranked/seasonal stakes |
| Remote content (Addressables + CDN) | [MODERNIZE] | original shipped content local-only — no live events |
| Consolidated analytics + server-side A/B | [MODERNIZE] | original triple-stacked analytics; A/B was client-random |
| Never log save state | [REPLACE] | original logged full save JSON to logcat (info leak) |

---

## 5. Content bible

**Discipline: modern but restrained — avoid fantasy bloat.** Counts are caps, not targets.

### 5.1 Factions
| | MVP | Launch | Full vision |
|---|---|---|---|
| Factions | **2** | 2 | **4** |
| Iron Pact | disciplined legion (formations, shields, attrition) | — | — |
| Ashen Horde | swarm aggro (speed, flank, expendable) | — | — |
| (Future) | — | — | Arcane order (caster-centric), Mechanized (siege/structures) |

Each faction = a **doctrine + a distinct kit + clear strengths/weaknesses** (blueprint §5). Asymmetry is required (replayability + ranked diversity). No faction may be strictly dominant.

### 5.2 Units (6 units per faction, drawn from a shared 7-archetype palette)
| Archetype | Combat role | Armor | Iron Pact | Ashen Horde |
|---|---|---|---|---|
| Miner | economy (mine Gold, weak combat) | Light | Miner | Miner |
| Frontline | tank / line-hold | Shielded | Shieldman | — (no dedicated Frontline; Razorbeast/Heavy holds the line) |
| Skirmisher | melee DPS | Heavy/Light | Legionary | Raider |
| Ranged | ranged DPS | Light | Crossbow (Pierce) | Slinger (Blunt) |
| Caster | AoE / utility | Light | Battlemage (Fire) | Hexcaster (poison) |
| Heavy | siege / anti-structure | Heavy | Ironclad | Razorbeast |
| (Flanker) | fast mobility | Light | — | Houndmaster |

**Archetype palette (7 roles):** Miner, Frontline, Skirmisher, Ranged, Caster, Heavy, Flanker. **Each faction fields exactly 6 units**; the asymmetry is *which* role it omits/adds — **Iron Pact** has a Frontline (Shieldman) and **no** Flanker; **Ashen Horde** has a Flanker (Houndmaster) and **no** dedicated Frontline (its Razorbeast/Heavy holds the line). "6 archetypes/roles" elsewhere in this document refers to a faction's **6-unit set**, not the 7-role shared palette.

**Counts:** MVP **12** (6/faction) · Launch: **+1 unit only if** a season spends its single content slot on a unit (§13 6.2) · Full vision full rosters (≤9/faction — hard cap to protect balance).

### 5.3 Spells (draft-3 roguelite)
- **Pool:** MVP ~**12** owned spells; draft **3** per battle.
- **Categories:** Offensive (Archidon-Rain-style barrage, LightningStorm), Control (Freeze→Shatter synergy, Stun), Economy (GoldRush, RaiseGold), Summon (Meric/Pouncer/Giant-type), Buff (Rage, Haste).
- **Every spell:** cooldown + charge, a **telegraph**, a **counter**, and **synergy tags**. No un-counterable spell ships.
- Derived from the recovered 18-spell roster *(dossier §9)* but rebuilt as draftable + synergistic.

### 5.4 Maps & modes (see §7 for full mode specs)
- **Maps:** MVP **3** (one battlefield archetype, 3 terrain layouts) · Launch: **+1 map only when** a season's single content slot is a map (§13 6.2 — one of {unit | commander | map} per 8-week season).
- **Modes:** Classic (Campaign), Endless, async Tournament/Ladder (MVP); Ranked seasons, seasonal/experimental (post-launch).

### 5.5 Commanders
- MVP: **1 per faction** (2 total). Collection roster post-launch. Full spec §6 / blueprint §6.

### 5.6 Cosmetics / chests / shops / events
- Cosmetics: §6. Chests: §8. Gem economy: §9. IAP/shop: §10. Events: §8 (basic at MVP) → event engine post-launch.

**Character count summary:** MVP = 12 units + 2 commanders · Launch/Season cadence = **one** new {unit | commander | map} per 8-week season (single content slot, §13 6.2) · Full vision ≈ 30+ units (≤9/faction × 4) + commander roster. Restraint is canonical: **no unit ships without a distinct role + counter**.

---

## 6. Character + cosmetic framework (outfit classes)

**Requirement (explicit):** every character/class supports multiple **outfit classes** — without ever breaking readability or fairness.

**Cosmetic-safety rule (INVIOLABLE):** a cosmetic may change **palette, material/texture, trim, particle/VFX color, and idle/victory flourishes**. A cosmetic may **NOT** change **silhouette, unit size, hitbox, animation timing, ability VFX *readability*, or faction-color identity**. Readability and fairness are gameplay-safe by construction.

| Cosmetic axis | Specification |
|---|---|
| **Base (locked)** | Each archetype has a canonical silhouette + faction base color (read-locked); this is what opponents parse |
| **Outfit classes** | Standard → Veteran → Elite → Legendary → Mythic (5 tiers). Each = recolor/material/trim/VFX-color over the locked base |
| **Variant types** | Armor variants (visual only), color schemes, weapon skins (same silhouette), VFX recolors, emotes/banners (out-of-battle) |
| **Rarity** | Common / Rare / Epic / Legendary / Mythic — purely cosmetic prestige + visual richness, **no stat or read advantage** |
| **Monetization** | Earned (battle pass, events, free chests) **and** purchasable (shop, gem-priced); higher tiers richer VFX, same silhouette |
| **Fairness limits** | Ranked may enforce a **"clarity mode"** (opponents render in standardized read-safe skins) so no cosmetic can ever obscure a competitive read |

**Why this design:** it gives deep collection/expression and is the **revenue engine** (cosmetic-led model, §10) while guaranteeing a paying player gains *prestige, not power, and not even a readability edge* — protecting P2-Readable-depth and P3-Fair-mastery.

---

## 7. Game mode system

Preserve the three recovered modes; modernize each. Production priority maps to phases: **P0 = MVP (Phases 1–5)**, **P1 = launch / Season 1 (Phase 6)**, **P2 = post-launch (Phase 7, deferred per the decision log)**.

| Mode | Disposition | Gameplay loop | Rewards | Replayability | Tech cost | Priority |
|---|---|---|---|---|---|---|
| **Classic / Campaign** | [PRESERVE]+[MODERNIZE] | scripted PvE battles, rising difficulty, light narrative; mine→train→push→statue with terrain/counters | first-clear Gem bonus (diminishing), Silver, PassXP, unlocks | medium (replay for stars/upgrades) | low | **P0** |
| **Endless (survival)** | [PRESERVE]+[MODERNIZE] | wave-survival vs adaptive director (modern `GetWaveRating`); run-based; spell-draft shines | scaling Silver/PassXP, leaderboard | high (run variety) | low-med | **P0** |
| **Tournament / Async Ladder** | [PRESERVE]+[MODERNIZE] | async vs opponent ghosts/snapshots; climb a ladder | Silver, cosmetics, seasonal | high | med (snapshot sim) | **P0 (ghost)** |
| **Ranked Seasons** | [REPLACE async→ranked] | async ladder with leagues + **deterministic-replay validation** + soft resets | ranked cosmetics via *Honor* (earned currency — §9) | very high | high (determinism) | **P2 (Phase 7.1)** |
| **Seasonal modes** | new | rotating limited rulesets per season (basic weekend modifiers ship at launch §13 4.5; full seasonal-mode/event engine is Phase 7) | event cosmetics via *Event tokens* (earned — §9) | seasonal | low (data-driven) | **P2 (Phase 7.6)** |
| **Experimental modes** | new | co-op / boss-rush / draft-only labs to test mechanics safely | cosmetics, fun | varies | varies | **P2** |

**Rule:** modes share the one combat core (§4) — no mode introduces bespoke mechanics that fork balance. Modifiers are data, not new systems.

---

## 8. Chest + reward architecture (ethical)

**Anti-pattern rejected:** the original's opaque chest/gacha *(dossier §6)* is **[CUT]**. BULWARK chests are an **earned free-track pacing loop with fully disclosed odds and cosmetic-only contents**.

*(Naming note: chest **tier names** below are cosmetic labels, distinct from the Gold/Silver **currencies** defined in §9 — e.g., the "Gold" chest awards Gems/shards, not Gold currency.)*

| Chest | Source | Open time | Slots | Rarity odds | Contents |
|---|---|---|---|---|---|
| **Wood** | per-battle (frequent) | 15 min | 4 (queue) | disclosed | Silver, small PassXP, common cosmetic shards |
| **Silver** | daily / win streak | 3 h | shared 4 | disclosed | Silver, cosmetic shards (common/rare), occasional Gems |
| **Gold** | quests / weekly | 8 h | shared 4 | disclosed | shards (rare/epic), Gems, cosmetic tokens |
| **Seasonal** | events / pass | event-timed | event | disclosed | seasonal cosmetics |

**Time-to-open logic:** a soft pacing mechanic for **free** rewards (not a paywall). A chest occupies a slot and unlocks after its timer; **Gems can skip the timer (convenience only)**. Slot cap (4) gently paces opening; chests never expire-and-vanish punitively.

**Fairness rules (binding):**
- **Disclosed odds** on every chest (regulatory + trust).
- **No power in any chest** — cosmetics + currency only.
- **No paid random boxes** — paid cosmetics are **direct, see-what-you-buy** purchases (§10). Chests are an *earned* loop; if a chest is ever purchasable, its odds are disclosed and contents are cosmetic-only with a pity/duplicate-protection rule.
- **Duplicate protection:** dupes convert to cosmetic-craft shards (no dead pulls).

**Free vs premium:** free track = Wood/Silver/Gold from play; premium = battle pass (which *contains* seasonal chests with disclosed cosmetic contents). The loop rewards play, not wallet.

---

## 9. Gem economy strategy

Gems are the premium currency. **Their power is deliberately bounded.**

| Aspect | Specification |
|---|---|
| **Earned** | first-clears (diminishing `max(5, base−5×replays)`), daily/weekly quests, achievements, events, battle-pass track, **opt-in rewarded ads** (`gems_per_ad_watch`-style, e.g., 10), login streak |
| **Stored** | **server-authoritative** balance; client holds an obscured CRDT cache (dossier §6/§8 pattern) reconciled on sync; trusted-time anchor guards time-based grants |
| **Spent** | cosmetics, battle-pass premium, **convenience only** (chest-skip, extra cosmetic slots), commander *skins* |
| **Protected** | server validates all grants/spends; client obscuration deters memory edits; ranked outcomes server-checked; no client-trusted currency mutation |
| **Gems CANNOT** | buy raw power, buy upgrades beyond the cap, buy units/commanders' *power*, buy ranked advantage, fuel any gacha-for-power. (These are hard prohibitions.) |
| **Post-launch currencies** | *Honor* (ranked) and *Event tokens* (seasonal) ship **with their Phase-7 modes** (§7). Both are **earned-only, server-authoritative, redeemable for cosmetics only, never power** — they expand the 4 MVP currencies (Gold/Silver/Gems/PassXP) without weakening any fairness rule. |

**Why:** gems must be *desirable* (cosmetics, time-saving, prestige) without being *coercive* (power). This sustains revenue while protecting P3-Fair-mastery — the discipline the original lacked.

---

## 10. IAP + revenue architecture

**Model: cosmetic + battle-pass led, transparent, ethical.** (Blueprint §7; integrity rules binding.)

| Product | Form | Price band (illustrative) | Notes |
|---|---|---|---|
| **Battle Pass** | seasonal dual-track (free+premium) | ~$9.99 / ~950 Gems | revenue anchor; earnable by play; cosmetics + currency |
| **Cosmetic skins** | direct, see-what-you-buy | $4.99–$9.99 / gems | no power; outfit-class tiers (§6) |
| **Banners/emotes** | direct | $0.99–$2.99 | cheap expression |
| **Gem packs** | direct currency | $0.99 → $99.99 ladder | transparent value ladder |
| **Starter/first-purchase offer** | one-time bundle | $2.99–$4.99 | conversion anchor; cosmetic + gems value |
| **Convenience** | chest-skip, slots | gems | time, never power |

- **Value ladder:** small→whale tiers, each with shown value; bonus % rises modestly with size (transparent, not predatory).
- **Shop rotation:** weekly featured cosmetics + bundles; seasonal lines; no FOMO-manipulation beyond honest limited-time seasonal items.
- **Conversion strategy:** first-purchase offer → battle pass habit → cosmetic collection; opt-in rewarded ads as a non-paying engagement + light revenue lever.
- **Ethical rules (binding):** no loot boxes/gacha, no interstitials (opt-in rewarded only), no energy gates, no P2W, disclosed odds where any randomness exists, honest pricing.
- **Commercial reality (acknowledged):** fair monetization caps ARPU → cosmetic content velocity + retention carry the title (top commercial risk, §16/blueprint §12).

---

## 11. World + environment design

| Element | [Disposition] | Spec |
|---|---|---|
| **Statue** | [MODERNIZE] | the iconic win/lose objective; armored, with a **shield phase** and clear **damage states** (intact→cracked→breaking→destroyed) for a readable climax; throttles trickle damage (recovered statue math) |
| **Gold mines** | [MODERNIZE] | fixed nodes with a **miner cap**; contestable position → economy becomes a light spatial contest, not a static tap |
| **Battlefield** | [MODERNIZE] | one horizontal front, 3 rows, 2–4 terrain features (high ground/choke/cover/hazard); statues at ends; clean parallax depth |
| **Environment props** | new, restrained | readable, low-clutter; never obstruct unit reads |
| **Atmosphere** | new | faction-themed lighting/palette; restrained particles (mobile perf budget) |
| **Readability (INVIOLABLE)** | — | silhouettes, color-blind-safe faction/damage palettes, telegraphs, clear health/objective UI |

**Principle: modern but timeless** — depth from interactions, not visual clutter. Perf budget is a hard constraint (mid-range phone target, per recovered footprint).

---

## 12. Technical + production canon (authoritative rules)

| Domain | **Hard rule** |
|---|---|
| Engine | Unity 6 LTS, IL2CPP, URP 2D. No engine swaps. |
| Sim boundary | **Battle simulation = ECS/DOTS, fixed-timestep, isolated.** UI/meta/menus = MonoBehaviour/UGUI. No ECS in UI; no gameplay sim in MonoBehaviour. |
| Determinism | MVP: non-deterministic sim + **server stat-sanity checks**. Deterministic (fixed-point/lockstep) + replays = **Phase 7 gate** (ranked). Don't gate MVP on it. |
| Backend | **Server-authoritative** economy/profile/inventory/progression/ranked. MVP via managed BaaS (PlayFab/Nakama); custom services only when BaaS limits hit. |
| Saves | Server profile = source of truth; platform snapshots = backup. **NEVER log save state.** |
| RemoteConfig | 3-tier resolver (RC→typed SO→literal) kept; values **server-owned**; **server-side A/B assignment** (no client-random). |
| Analytics | **One** consolidated product+attribution pipeline. Consent (UMP) required; minimize PII; data-deletion path. |
| Anti-cheat | client obscuration (deterrence) + server authority on economy + ranked replay validation (Phase 7). Detect on client, decide on server. |
| CI/CD | automated build + test gates + content-bundle pipeline + feature flags. No manual release builds for store. |
| Content tools | units/spells/balance authored as **data (ScriptableObject) → exported config**; designer-editable; Addressables + remote catalog/CDN. |
| Perf | mid-range-phone frame budget is a hard gate every phase; budgeted AI scheduler; instancing for crowds; particle caps. |

**Hard constraints (never violated):** readability, fairness/no-P2W, server authority over currency, no save-state logging, perf budget, the §15 CUT list.

---

## 13. MASTER PHASE SYSTEM

Phase-driven and executable. Each sub-phase: **Objective · Systems · Dependencies · Deliverables · Validation gate · Exit criteria · Risk.** A phase starts only when its dependencies' exit criteria pass. **Bold gates** can kill/rescope. Phases 0–5 use the full 7-field schema; the post-soft-launch Phases 6–7 use a reduced schema with an explicit **Entry** dependency stated under the phase header. **Legend: "Gn" = GATE n must PASS.**

### Phase 0 — Foundation & Canon Lock
| Sub | Objective | Systems | Deps | Deliverables | Gate | Exit | Risk |
|---|---|---|---|---|---|---|---|
| 0.1 | Project scaffold | Unity6/URP2D project, repo, folder/asmdef structure, CI skeleton | — | building project, CI green | builds on CI | reproducible build | over-scaffolding |
| 0.2 | ECS sim spike | ECS components/systems; 1 unit moves+attacks another | 0.1 | sim core demo | 200 units @ stable frame on mid phone | perf budget met | ECS complexity |
| 0.3 | Data model + config resolver | ScriptableObject unit/spell/balance schema; 3-tier RC resolver | 0.1 | data pipeline + resolver | designer edits a unit stat via data, sim reflects it | data-driven sim | schema churn |
| 0.4 | Service stubs | BaaS auth/profile/config; analytics stub | 0.1 | login + profile read/write + event log | server round-trip works | backend reachable | BaaS lock-in |
| **Exit Phase 0** | — | — | — | sim core + data pipeline + backend stub + CI | **all sub-gates pass** | proceed to combat | — |

### Phase 1 — Core Combat Prototype (**FUN GATE**)
| Sub | Objective | Systems | Deps | Deliverables | Gate | Exit | Risk |
|---|---|---|---|---|---|---|---|
| 1.1 | Economy + objective | mines (capped), miner mining, Gold, statue (damage states) | 0.2–0.3 | mine→Gold→statue playable | loop runs | economy loop fun | — |
| 1.2 | One faction, 4 units | train/queue/deploy; 4 units (Miner+3 roles) | 1.1 | unit roster v0 | units fight | composition matters | — |
| 1.3 | Control model | squad select/command/drag + **possess** | 1.2 | touch control scheme | control feels good on device | agency confirmed | touch feel |
| 1.4 | Targeting + combat | influence-map targeting; modifier chain; basic counters | 1.2 | combat resolves readably | front line reads | combat legible | targeting jitter |
| 1.5 | Basic AI | utility commander + unit layer | 1.4 | AI opponent | AI provides a fair fight | playable vs AI | over/under-tuned |
| **GATE 1 (FUN)** | — | — | 1.1–1.5 | a fun, complete micro-battle | **binding: combat fun? if not → kill/pivot** | green-light meta | **combat not fun** |

### Phase 2 — Tactical Depth (Vertical Slice)
| Sub | Objective | Systems | Deps | Deliverables | Gate | Exit | Risk |
|---|---|---|---|---|---|---|---|
| 2.1 | Terrain + positioning | high ground/choke/cover/hazard; flank/back mults | G1 | terrain combat | terrain changes outcomes readably | depth+readable | clutter |
| 2.2 | Counters + formations | type×armor matrix; Line/Tight/Loose | 2.1 | counter system | counters teachable | counters land | balance |
| 2.3 | Second faction | Ashen Horde (6 units), asymmetry | 1.2 | 2 factions | asymmetry fair | replayable | balance |
| 2.4 | Spell draft | pool ~12; draft 3; synergy; telegraph/counter | 2.2 | spell system | spells counterable | tactical layer | power spikes |
| 2.5 | Commander bones | 1/faction: active+passive, capped | 2.3 | commanders | within power budget | identity | power creep |
| 2.6 | AI layering | squad layer; budgeted scheduler; difficulty axes | 1.5 | layered AI | stable frame + smart | scalable AI | perf |
| 2.7 | 3 maps | terrain layouts | 2.1 | map set | variety | content ready | — |
| **GATE 2** | — | — | 2.1–2.7 | vertical slice | **external playtest: ≥~40% session-2 return AND majority of testers rate combat "readable & fun" on a fixed rubric** | green-light meta build | depth unreadable |

### Phase 3 — Meta & Economy Shell
| Sub | Objective | Systems | Deps | Deliverables | Gate | Exit | Risk |
|---|---|---|---|---|---|---|---|
| 3.1 | Server economy | the **4 MVP currencies** (Gold/Silver/Gems/PassXP; post-launch cosmetic currencies per §9), server-auth balance | 0.4 | authoritative wallet | server validates spend/grant | trust model live | server bugs |
| 3.2 | Progression | capped unit upgrades; commander levels | 3.1 | upgrade system | caps enforced | progression loop | P2W leak |
| 3.3 | Campaign + Endless | ~20-level Act 1; Endless + director | G2 | PvE modes | clearable + replayable | PvE content | difficulty tuning |
| 3.4 | Async ghost ladder | snapshot opponents; stat-check validation | 3.1 | ladder | climbable | competition loop | snapshot fidelity |
| 3.5 | RC + analytics | live tuning; event taxonomy | 0.3,0.4 | tuned funnel | values retune live | live-ops ready | event gaps |
| **GATE 3** | — | — | 3.1–3.5 | MVP feature-complete | economy server-validated; modes complete | enter monetization | scope creep |

### Phase 4 — Monetization & Live-ops Shell
| Sub | Objective | Systems | Deps | Deliverables | Gate | Exit | Risk |
|---|---|---|---|---|---|---|---|
| 4.1 | Cosmetics | skin/outfit-class system; clarity-mode | G2 | cosmetic pipeline | readability rule enforced | gameplay-safe cosmetics | readability break |
| 4.2 | Battle Pass S0 | dual track, earn-by-play | 3.1 | pass | earnable + premium | revenue anchor | grindiness |
| 4.3 | Shop + IAP + ads | direct cosmetics, gem ladder, opt-in rewarded | 3.1,4.1 | store | transparent; no P2W | monetization live | dark-pattern creep |
| 4.4 | Chests + gems | ethical chests (disclosed odds), gem rules | 3.1 | reward loop | no power in chests | reward pacing | exploitative drift |
| 4.5 | Retention loops | daily/weekly quests, streak, weekend modifiers | 3.5 | live cadence | cadence runs | retention scaffolding | over-asking |
| **GATE 4** | — | — | 4.1–4.5 | monetization shell | **fairness audit: zero P2W; readability intact** | soft launch | fairness violation |

### Phase 5 — Soft Launch & Tuning (**SCALE-OR-STOP GATE**)
**Entry: GATE 4 must PASS.** Deps: 5.1 ← GATE 4; 5.2, 5.3 ← 5.1. Systems: telemetry/RC/analytics (§12). Validation: GATE 5 (below). Tuning only — **no new features**.
| Sub | Objective | Deliverables | Exit | Risk |
|---|---|---|---|---|
| 5.1 | Limited-geo release + telemetry | live build, funnel dashboards | data flowing | — |
| 5.2 | Retention/monetization tuning (RC) | tuned curves | thresholds approached | over-tuning |
| 5.3 | Balance + perf/stability hardening | stable, balanced build | crash-free + balanced | regressions |
| **GATE 5 (SCALE/STOP)** | — | — | **D1≥~40%, D7≥~18% AND monetization floor: blended D30 LTV ≥ target CPI (exact floor set by an ADR before P5; STOP-blocking) → scale; else stop/rescope** | LTV below gate |

### Phase 6 — Live Launch & Season 1
**Entry: GATE 5 (SCALE-OR-STOP) must PASS.** Deps: 6.1 ← GATE 5; 6.2 ← 6.1. **GATE 6 (launch-readiness):** crash-free ≥ ~99%, store-compliance + perf budget met, before global rollout (6.1). Risk: live-ops sustainability for team size; cadence over-commit.
| Sub | Objective | Deliverables | Exit |
|---|---|---|---|
| 6.1 | Global launch | global build, store presence | launched |
| 6.2 | S1 content + cadence | 1 new unit/commander/map; 8-week season live | live-ops operational |

### Phase 7+ — Post-launch (DEFERRED, decision-log-gated)
Ranked seasons + **deterministic replays** (7.1) · Clans/clan-wars (7.2) · 3rd faction (7.3) · Biomes (7.4) · Commander collection (7.5) · Event engine (7.6) · Real-time skirmish (7.7, only if determinism + audience justify). Each starts only when its decision-log revisit-trigger fires.

---

## 14. MASTER CLAUDE CLI EXECUTION PROMPTS

One master prompt per phase. **Every prompt is governed by §15.** Template fields are filled per phase; the agent must obey the **STOP conditions**.

**Universal preamble (prepended to every phase prompt):**
> You are a BULWARK production agent. Your governing authority is `report/BULWARK_MASTER_DEVELOPMENT_ROADMAP.md` (primary — law). Before acting, also read `ROADMAP_CHANGELOG.md` and `PRODUCTION_DECISION_LOG.md` (the latter holds the Phase-7 deferral **revisit-triggers** this roadmap references). Treat canon (§2–12) as law. Do NOT invent units, currencies, mechanics, or systems not in the roadmap. Do NOT implement features from later phases. Respect the inviolable constraints: readability, fairness/no-P2W, server authority over currency, no save-state logging, perf budget, and the §15 CUT list. If anything is ambiguous, under-specified, or would require a canon change, **STOP and surface an ADR request — do not guess.** Confirm prior-phase exit criteria are met before starting. Produce only this phase's deliverables; then stop at the validation gate for review.

| Phase | Master prompt (objective · inputs · deliverables · STOP) |
|---|---|
| **0** | *Objective:* scaffold project + ECS sim spike + data/config pipeline + backend stub per §13 Phase 0. *Inputs:* roadmap §0,§12. *Deliverables:* building Unity6/URP2D project, CI, ECS sim demo (200 units stable), SO data schema + 3-tier resolver, BaaS auth/profile/config + analytics stub. *STOP:* when all 0.x sub-gates pass; do not start combat content. Report perf numbers. |
| **1** | *Objective:* core combat prototype + FUN GATE per §13 Phase 1. *Inputs:* §3 (preserve loop), §4 combat, §11 world, §0 exit. *Deliverables:* mine→train→push→statue, 1 faction/4 units, squad+possess control, influence-map targeting + modifier chain, basic utility AI. *STOP:* at GATE 1 — present a playable micro-battle for the binding fun-check; do NOT build meta/economy. If combat isn't fun, STOP and request a pivot ADR. |
| **2** | *Objective:* tactical depth / vertical slice per §13 Phase 2. *Inputs:* §4 modernize, §5 content, §6 cosmetic-safety (for art hooks). *Deliverables:* terrain+positioning, type×armor matrix, formations, 2nd faction, spell-draft, 2 commanders (capped), layered AI + scheduler, 3 maps. *STOP:* at GATE 2 external playtest; no meta backend beyond stubs; no monetization. Keep all units within the §5 roster — no new units. |
| **3** | *Objective:* meta & economy shell per §13 Phase 3. *Inputs:* §9 gems, §12 backend rules. *Deliverables:* server-authoritative wallet (Gold/Silver/Gems/PassXP), capped upgrades, commander levels, Campaign Act 1 (~20), Endless+director, async ghost ladder (stat-checked), RC tuning + analytics. *STOP:* at GATE 3 feature-complete-MVP; do not add monetization yet. Currency MUST be server-authoritative — never client-trusted. |
| **4** | *Objective:* monetization & live-ops shell per §13 Phase 4. *Inputs:* §6 cosmetics, §8 chests, §9 gems, §10 IAP. *Deliverables:* outfit-class cosmetic system + clarity-mode, Battle Pass S0, shop + transparent IAP + opt-in rewarded ads, ethical chests (disclosed odds, no power), gem rules, retention loops. *STOP:* at GATE 4 — run the fairness audit (zero P2W, readability intact, disclosed odds). If any monetization touches power, STOP and reject. |
| **5** | *Objective:* soft launch + tuning per §13 Phase 5. *Inputs:* §10 KPIs, §12 analytics/RC. *Deliverables:* limited-geo build, telemetry dashboards, RC-tuned retention/monetization, balance + perf/stability hardening. *STOP:* at GATE 5 SCALE-OR-STOP — present retention/monetization vs the gate; do not scale globally without passing. No new features (tuning only). |
| **6** | *Objective:* global launch + S1 per §13 Phase 6. *Inputs:* GATE 5 PASS, §7 modes, §8/§10 live-ops config, §12 launch-readiness rules. *Deliverables:* global build, store presence, S1 content (**one** new unit *or* commander *or* map), 8-week live-ops cadence. *STOP:* once cadence operational; post-launch features remain §7-gated — confirm a §7 revisit-trigger in PRODUCTION_DECISION_LOG.md before any §7 work. |
| **7.x** | *Objective:* a single deferred feature (ranked/replays, clans, 3rd faction, biomes, commander collection, event engine, real-time) ONLY after its **revisit-trigger** (PRODUCTION_DECISION_LOG.md §1–3) fires. *Inputs:* roadmap §7/§12/§13 Phase 7 + the feature's decision-log entry. *Deliverables:* the one feature fully integrated under canon (e.g., 7.1 = deterministic sim + replay-validated ranked + *Honor* currency §9). *STOP:* one feature per prompt; confirm trigger met; no bundling; full §15 governance applies. |

---

## 15. Hallucination prevention layer (agent governance)

**The agent operates under these rules at all times. Violating any is a defect.**

1. **Canon is closed.** No new units, factions, currencies, spells, mechanics, modes, or systems beyond §2–12. Want one? → file an ADR; do not implement.
2. **No phase-jumping.** Implement only the current phase's deliverables; never pull in later-phase features (esp. §7 deferred).
3. **CUT list is a hard prohibition** (never implement, even if asked casually): loot boxes / gacha-for-power, interstitial ads, energy/stamina gates, pay-to-win or sellable power, paid random boxes without disclosed odds + dupe protection, save-state logging, client-authoritative currency, real-time PvP before Phase 7, biomes/clans/3rd-faction/commander-collection before their trigger.
4. **Inviolable constraints** (auto-reject any change that breaks them): readability (silhouette/clarity), fairness/no-P2W, server authority over currency, perf budget, disclosed odds.
5. **Trace-to-canon.** Every implemented feature must cite the roadmap section that authorizes it. If it can't, it doesn't ship.
6. **No silent assumptions.** Ambiguity/under-specification → STOP and ask (ADR request). Never invent values, names, or behavior to "fill gaps."
7. **No quality drift.** Match the established art/code/data conventions; reuse shared systems (one combat core, shared skeletons); no bespoke one-off mechanics per mode.
8. **Gates are binding.** Do not pass a validation gate without meeting its exit criteria; surface failures honestly (no faked success).
9. **Idea hygiene.** "Cool idea" ≠ canon. Park ideas in a backlog ADR; the roadmap decides, not the moment.
10. **Determinism/scope discipline.** Don't over-engineer (no full GOAP/ML, no premature determinism, no custom backend before BaaS limits) — the roadmap specifies the deliberate level.

---

## 16. Risk + governance layer (decision hierarchy)

**Roles & authority:**
| Role | Decides | Cannot unilaterally |
|---|---|---|
| **Game Director (GD)** | vision, pillars, scope cuts, mode/feature greenlight, pivots | violate inviolable constraints |
| **Lead Systems Designer (LSD)** | combat/economy/balance values *within canon* | add new systems/currencies (→ ADR) |
| **Technical Architect (TA)** | tech stack, ECS boundaries, determinism timing, backend | break perf/server-authority rules |
| **Live-ops/Product (LP)** | monetization config, shop/pass/events *within fairness rules* | introduce P2W/dark patterns |
| **Production agent (Claude CLI)** | implements the current phase per roadmap | anything in §15 |

**Decision mechanics:**
- **ADR (Architecture/Design Decision Record):** any change to closed canon, any new mechanic/currency/system, or any constraint exception requires a written ADR approved by the relevant owner (GD for design/scope, TA for tech, LP for monetization). Inviolable constraints are **non-overridable** — even by the GD.
- **Tie-breaker:** the roadmap; if the roadmap is silent, the GD decides and records an ADR.
- **Cuts/scope:** GD, informed by gate results. **Gate failures auto-escalate** to GD (and TA/LP as relevant).
- **Balance:** LSD within canon, telemetry-driven; ranked normalization is mandatory.
- **Monetization:** LP within §9/§10 fairness rules; any power-adjacent proposal is auto-rejected.
- **Pivots:** only GD, only at a gate, recorded as an ADR + reflected in `ROADMAP_CHANGELOG.md`.

**Top risks (ranked; full register in blueprint §12):** ① fair-monetization LTV (commercial, critical) → soft-launch scale-or-stop gate. ② combat fun (design, critical) → Phase-1 fun gate. ③ ECS/determinism complexity → phase-gated. ④ cosmetic content velocity → shared-skeleton pipeline. ⑤ combinatorial balance → telemetry + RC + normalized ranked. ⑥ scope creep → §15 + ADRs + decision log.

---

## 17. Final canon summary

**BULWARK is a fair, agency-first, tactically deep mobile RTS-lite.** It preserves the decade-proven Stick War core the reconstruction recovered — *mine → train → push → topple the statue*, direct unit control, persistent (capped) progression, async competition, and a small, readable, performant content set — and modernizes exactly where the original aged: a **real tactical battlefield** (terrain, formations, a type×armor counter system, positional flanking, a draftable synergistic spell loadout, layered-but-O(1) AI, capped commanders) and a **trustworthy technical spine** (ECS sim, server-authoritative economy, remote content, consolidated analytics). It monetizes **ethically and cosmetically** — battle pass + see-what-you-buy cosmetics with **outfit classes that never touch power or readability**, disclosed-odds earned chests, and gems that buy prestige and convenience but **never power**. It ships **lean and gated**: combat must be fun before meta is built; LTV must clear a bar before global scale; deferred features wait for their triggers. Quality, fairness, and readability are inviolable. This document is the law that keeps it so.

---

*Documentation + execution system only — no implementation, no coding, no reverse engineering. Canonical as of authoring; supersedes prior docs on conflict. Companion: `ROADMAP_CHANGELOG.md`.*
