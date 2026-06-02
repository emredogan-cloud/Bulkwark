# Codename "BULWARK" — Production Blueprint

**A production-grade plan** translating `NEXTGEN_RTS_SUCCESSOR_REPORT.md` (vision) and `STICK_WAR_MASTER_RECONSTRUCTION_DOSSIER.md` (evidence) into a buildable game. Specification, not brainstorming. Decisions are final unless a stated revisit-trigger fires; cuts are logged in `PRODUCTION_DECISION_LOG.md`. Illustrative numbers are labelled *illustrative* (first-pass, telemetry-tuned later), not final balance.

---

## 1. Project identity

| Field | Definition |
|---|---|
| **Codename** | **BULWARK** (the statue/base you defend and destroy) |
| **Genre** | Direct-control tactical RTS-lite — single-front lane skirmisher, PvE + async competitive |
| **Target audience** | 18–34, mobile strategy/action players; Stick War / lane-RTS / autobattler / survivor-like crossover who want **agency + tactics + fairness**; secondary: lapsed PC-RTS players wanting mobile-friendly depth |
| **Platform** | Mobile-first Android + iOS (portrait-capable, landscape primary); Unity 6 LTS; PC/Steam a *full-vision* option, not MVP |
| **Session** | 2–5 min battles; suspend/resume; one-handed-capable |
| **Design pillars** | **1. Agency** (command squads, possess a unit) · **2. Readable depth** (terrain/formation/counters that read instantly) · **3. Fair mastery** (skill > spend; no P2W) · **4. Respect the player** (short sessions, no dark patterns) |
| **Elevator pitch** | *You command a stick-army on a living battlefield: mine gold, train counters, shape the front with terrain and formations, and personally seize the key unit at the decisive moment to break the enemy line and topple their statue. A fair, tactically deep, agency-first RTS-lite — Stick War's accessibility with real tactics and no pay-to-win.* |

**Anti-identity (what BULWARK is not):** not a real-time-PvP-first arena (Clash Royale), not a whale-driven SLG/4X, not a passive autobattler, not a gacha. These are explicit non-goals.

---

## 2. Product vision → MVP translation (ruthless)

The MVP exists to answer one question with real players: **is agency + terrain/formation/counter combat fun, and does fair monetization retain?** Everything not serving that is cut or phase-gated.

| Feature | **MVP (soft launch)** | Post-launch (S1–S3) | Full vision |
|---|---|---|---|
| Factions | **2** (asymmetric) | +1 (S2) | 4 |
| Units | **12** (6+6) | +2–3/faction | full rosters |
| Battlefield | **1 archetype, 3 maps**, terrain features | +biome variants | biomes w/ deep mechanics |
| Formations | **Line / Tight / Loose** | advanced (wedge/skirmish) | full |
| Control | **squad command + possess + commander ability** | refinements | gestures/macros |
| AI | **utility commander + influence-map units + budgeted scheduler** | adaptive director tuning | full director |
| Spells | **draft 3 of ~12** | +pool, synergies | deep synergy web |
| Commanders | **1 per faction (system bones)** | commander **collection** + talents | full roster |
| Modes | **Campaign (~20 lvls) + Endless + async ghost ladder** | Ranked seasons + leagues | + real-time skirmish (if earned) |
| Meta/economy | **server-authoritative, capped upgrades, Silver/Gems** | events economy | full |
| Monetization | **Battle Pass S0 + cosmetics + opt-in rewarded** | seasonal passes, shop rotation | full |
| Social | — | **clans + async clan goals** | clan wars |
| Tech | **server-auth economy; sim runs in ECS; replays OFF** | **deterministic replays + cheat-proof ranked** | + real-time netcode |
| Live-ops | basic weekend modifiers | event engine, calendar | full |

**Hard cut line for MVP:** real-time PvP, clans, deterministic-replay ranked validation, hero collection economy, biomes, 3rd+ faction, full event engine. (Rationale per item in the decision log.) **What survives is the smallest set that proves the pillars + a fair-monetization shell.**

---

## 3. Core gameplay loop

```
MINUTE-TO-MINUTE (one 2–5 min battle)
  ┌────────────────────────────────────────────────────────────────────┐
  │ PREPARE: pick faction + commander, DRAFT 3 spells from owned pool    │
  │ DEPLOY:  miners auto-mine fixed gold nodes → Gold income             │
  │ ECONOMY: spend Gold to TRAIN units (queue); choose composition vs    │
  │          read enemy army (counter system)                            │
  │ COMBAT:  command SQUADS (tap/drag) on a terrained front; set         │
  │          FORMATION; POSSESS a unit at key moments; CAST drafted       │
  │          spells (telegraph→counter); exploit terrain/flank            │
  │ OBJECTIVE: break the enemy line → damage/destroy enemy STATUE        │
  │ END:     Victory/Defeat → reward screen                              │
  └────────────────────────────────────────────────────────────────────┘
        (core loop preserved from dossier §4: mine→train→push→statue,
         now with terrain/formations/counters/spell-draft = depth)

SESSION-TO-SESSION (10–30 min)
  choose MODE → battle → earn Silver + PassXP (+first-clear Gem bonus w/
  diminishing returns) → spend Silver on capped UPGRADES, Gems on COSMETICS
  → daily/weekly quests → next battle

META-TO-META (weeks/season)
  persistent capped UPGRADES · BATTLE PASS track · async LADDER rank ·
  COSMETIC collection · CAMPAIGN progress · commander level
```

**Retention cadence:** daily (quests, login streak), weekly (quests, featured mode), seasonal (battle pass, ranked season, new unit/commander/map). Mirrors the original's proven daily/streak/pass loops *[dossier §6]* but adds a competitive + social spine.

---

## 4. Combat system specification

**Battlefield structure.** One horizontal front, **3 logical rows** over a continuous space; **statue** at each end; **2–4 gold-mine nodes** at fixed positions. Each map ships **2–3 terrain features**:
- **High ground** — +15% range, +10% damage to occupants (illustrative).
- **Chokepoint** — width-limited; AoE value spikes; formation discipline matters.
- **Cover/forest** — −30% ranged damage taken; blocks line of sight.
- **Hazard** (optional per map) — damage-over-time zone.

**Control model.** Tap-select a squad → tap to move/attack; **drag** to direct a push along the front; **long-press** to *possess* one unit (manual aim/dodge/ability) — the signature agency hook *[dossier §5.1]*; **commander ability** button (1 active). Smart auto-target with manual override. Set squad **formation** via a 3-state toggle.

**Targeting (performant, per dossier §5).** Influence-map grid (≈16 cols × 3 rows) updated at ~4 Hz with threat/value; each unit reads its local cell to choose target/retreat → **O(1)/unit, no scans**. Same-row preference retained for readability; **stickiness ×1.2** to current target to avoid jitter (kept from the original).

**Armor classes (4):** Light, Heavy, Shielded, Structure.
**Damage types (5):** Slash, Pierce, Blunt, Magic, Fire.
**Type × armor matrix (illustrative multipliers):**

| ↓type / →armor | Light | Heavy | Shielded | Structure |
|---|---|---|---|---|
| Slash | 1.3 | 0.8 | 0.7 | 0.5 |
| Pierce | 1.0 | **1.5** | 0.6 (frontal) | 0.7 |
| Blunt | 0.9 | 1.1 | **1.3** | **1.5** |
| Magic | 1.1 | 1.0 | **1.4** | 0.8 |
| Fire | 1.2 | 1.0 | 1.0 | 1.0 + burn |

**Damage formula (keeps the dossier's clean modifier chain §5.2, fed by position/terrain):**
```
final = round( base
   × (1 + upgradeLevel × perLevelPct)        // persistent meta (capped)
   × typeArmorMult[type][armor]              // counter matrix
   × positionalMult                          // frontal 1.0 / flank 1.5 / back 2.0
   × terrainMult                             // high-ground/cover/etc.
   × difficultyMult )                         // PvE only, multi-axis
```
Positional flank/back replaces the original's binary `backstab ×2.0` flag with **skill-expressive geometry**. Statue retains throttle behavior (small hits reduced) and a shield phase, per the recovered statue math.

**Formations (3):** **Line** (frontal block; strong head-on, weak to flank/AoE) · **Tight** (melee-dense, high DPS, AoE-vulnerable) · **Loose** (spread, AoE-resistant, ranged-friendly). Facing is tracked → flanking is real.

**Spell system.** Player owns a pool (~12 MVP) and **drafts 3** per battle. Each spell: cooldown + charge, a **telegraph** (counterable: dodge/dispel/terrain), and **synergy tags** (e.g., Freeze sets *Chilled* → next Blunt hit *Shatters* for bonus). Evolves the original's consumable spells *[dossier §6]* into a roguelite loadout.

**Unit roles (6 archetypes, shared across factions, reskinned):** Miner (econ) · Frontline (tank/Shielded) · Skirmisher (melee DPS) · Ranged (Pierce/Blunt) · Caster (AoE/utility) · Heavy (siege/anti-Structure). Counter web: Pierce-Ranged > Heavy; Frontline-Shielded > frontal Ranged; Skirmisher flanks Ranged; Caster punishes Tight clumps but is fragile; Heavy breaks Structure/Statue.

---

## 5. Faction + unit blueprint

Two **asymmetric** MVP factions (asymmetry = replayability + ranked diversity, addressing the original's flat roster *[dossier §9]*).

### Faction A — **The Iron Pact** (disciplined legion)
- **Fantasy:** an unbreakable shield-wall army; war as attrition and discipline.
- **Doctrine:** hold the line, out-armor, win the long fight.
- **Combat identity:** durability, formations, frontal denial; low burst, slow.
- **Core roster (6):** Miner · **Shieldman** (Frontline/Shielded) · **Legionary** (Skirmisher/Heavy armor) · **Crossbow** (Ranged/Pierce) · **Battlemage** (Caster/Fire) · **Ironclad** (Heavy/siege).
- **Strengths:** frontal defense, formation bonuses, sustain. **Weaknesses:** flanking, Magic, mobility, burst tempo.

### Faction B — **The Ashen Horde** (swarm aggro)
- **Fantasy:** a fast, expendable tide that overwhelms before you're ready.
- **Doctrine:** tempo, flank, swarm; end it early.
- **Combat identity:** speed, cheap mass, flanking burst; fragile, weak economy if stalled.
- **Core roster (6):** Miner · **Raider** (cheap Skirmisher) · **Houndmaster** (fast flanker/Light) · **Slinger** (Ranged/Blunt) · **Hexcaster** (Caster/poison-debuff) · **Razorbeast** (Heavy/mobile).
- **Strengths:** speed, flanking, swarm, burst. **Weaknesses:** sustained defense, AoE, attrition, stalled economy.

| | Starter (tutorial) | MVP roster | Expansion (post-launch) |
|---|---|---|---|
| Iron Pact | Miner, Shieldman, Crossbow | +Legionary, Battlemage, Ironclad | +Sentinel, Bannerman, Siege-Engine |
| Ashen Horde | Miner, Raider, Slinger | +Houndmaster, Hexcaster, Razorbeast | +Plaguebringer, Berserker, Warbeast |

---

## 6. Hero / commander system

**Why heroes exist:** identity + a *fair* collection/progression hook + a skill lever (ability timing) — fixing the original's thin identity *[dossier §8]* without P2W.

| Aspect | Specification |
|---|---|
| **Role** | One Commander per battle = faction avatar; **1 signature active** (e.g., Iron Pact *Shield Wall*: +armor to a formation for 6 s; Ashen *Blood Rush*: +speed/+flank dmg) + **1 passive aura** + cosmetic identity. A force-multiplier, **not a super-unit**. |
| **Power budget** | Commander contributes **≤10–15%** of battle outcome (design constraint, telemetry-enforced). Abilities are tempo/utility, never raw stat inflation. |
| **Progression** | Levels via play → unlock ability ranks + a **small talent tree** (utility choices, sidegrades not power creep). |
| **Monetization boundary** | Commanders are **earnable** (play / battle pass). Premium = **skins/VFX/voice only**. New commanders earnable; optional accelerate-purchase, **never exclusive power**. |
| **Fairness rules** | **Ranked normalizes** commander talents to a capped set; each commander has counters; no strictly-dominant pick (balance via RC + telemetry). |
| **MVP scope** | **1 commander per faction** (2 total) — full system bones, no collection economy yet (collection deferred to post-launch, per decision log). |

---

## 7. Progression + economy sheet (illustrative values)

**Currencies (4, deliberately few):**

| Currency | Source | Sink | Persist? | Premium? |
|---|---|---|---|---|
| **Gold** | mined in-battle | train units (in-battle only) | no | no |
| **Silver** | battles, quests | **capped unit upgrades**, commander talents | yes | no |
| **Gems** | first-clears (diminishing), quests, **opt-in ads**, purchase | cosmetics, battle-pass premium, convenience | yes | yes |
| **Pass XP** | battles, quests | battle-pass tiers | season | no |

(Honor/ranked currency added with Ranked post-launch; no Shards/loot-box currency — ever.)

**Rewards (illustrative):** win ≈ **40 Silver + 60 PassXP**; loss ≈ 15 Silver + 30 PassXP; campaign **first-clear** Gem bonus = `max(5, 20 − 5×replays)` (the recovered diminishing-returns curve *[dossier §6]*); daily quest ≈ 100 Silver / 20 Gems; login streak (kept).

**Unit upgrades (capped, Silver, rising cost — illustrative):**

| Level | Cost (Silver) | Effect (illustrative) |
|---|---|---|
| 1→2 | 100 | +6% stat |
| 2→3 | 250 | +6% |
| 3→4 | 500 | +6% |
| 4→5 | 1,000 | +6% + minor perk |
| **Cap 5** | — | total ≈ +24% (capped → no infinite power; **ranked uses normalized/capped**) |

**Battle Pass:** 50 tiers / **8-week season**; free + premium tracks; premium ≈ **$9.99 / 950 Gems**; rewards = cosmetics + Silver + Gems + commander-shard (post-launch). Earnable purely by play.

**Cosmetics:** unit skins (Spine recolor/VFX) ≈ $4.99–9.99 or gem-priced; banners/emotes cheaper. **No power.**

**Monetization integrity rules (hard):** no loot boxes/gacha; no interstitials (opt-in rewarded only); no energy gates; premium never sells raw power; ranked normalizes progression. *Commercial consequence acknowledged:* fair monetization caps ARPU and raises the retention/conversion bar (see §12 top risk).

**Retention loops:** daily quests + streak → weekly quests + featured mode → seasonal pass + ranked + new content.

---

## 8. LiveOps + seasonal architecture

| Layer | Cadence | Content |
|---|---|---|
| **Season** | **8 weeks** | new Battle Pass, soft ranked reset, **1** new unit *or* commander *or* map, cosmetic line, balance patch |
| **Event** | weekly | weekend modifiers / limited modes (data-driven); mid-season limited event |
| **Shop** | weekly rotation | featured cosmetics, value bundles (transparent pricing) |
| **Clans** *(post-launch)* | continuous | membership, async clan goals/wars, shared rewards, chat |
| **Ranked** *(post-launch)* | per season | async ghost ladder, leagues Bronze→Master, seasonal rewards, **deterministic-replay validation** |

**How live service survives:** (1) **data-driven balance** via the 3-tier RemoteConfig resolver kept from the original *[dossier §6]* — retune without app updates; (2) **content velocity** from shared skeletons + reskins (cheap cosmetics fund the game); (3) **seasonal + social hooks** for retention; (4) **realistic 8-week cadence** matched to team size (not a 2-week treadmill a small team can't sustain).

---

## 9. Technical architecture specification

```
 CLIENT (Unity 6 LTS, URP 2D)
 ┌───────────────────────────────────────────────┐        BACKEND (server-authoritative)
 │  MonoBehaviour / UGUI shell                    │        ┌──────────────────────────────┐
 │   menus · shop · meta · profile UI             │ HTTPS  │ Auth (platform + device)     │
 │  ───────────────────────────────────────────  │◀──────▶│ Profile/Economy/Inventory    │
 │  ECS / DOTS BATTLE SIM CORE (isolated)         │        │  (currency, upgrades, pass)  │
 │   components: Unit, Health, Team, Formation    │        │ Ranked + Replay validation*  │
 │   systems: Targeting(influence map), Combat    │        │ Config service (typed RC,    │
 │            (modifier chain), AI(util/squad),   │        │  server-side A/B assignment) │
 │            Spell, Spawn — fixed timestep       │        │ Content catalog → CDN        │
 │  Budgeted AI scheduler (N agents/frame)        │        │ Analytics ingest + attrib.   │
 └───────────────────────────────────────────────┘        └──────────────────────────────┘
   Addressables ◀── remote catalog/CDN            Platform saves (Play Games/iCloud) = backup
                                                   *replay validation = post-launch (ranked)
```

| Concern | Spec | Note (vs dossier) |
|---|---|---|
| Engine | Unity 6 LTS, IL2CPP, URP 2D | proven by the original |
| **Sim** | **ECS/DOTS, fixed-timestep**, isolated from UI | upgrades the original's main-thread MonoBehaviour sim *[§4]*; enables scale + future replays |
| **Determinism** | **MVP: non-deterministic sim, server stat-sanity checks** → **post-launch: fixed-point/lockstep deterministic + replay validation** | phase-gated; determinism is hard, don't gate MVP on it (risk §12) |
| Backend | **Managed BaaS (PlayFab or Nakama) for MVP**; custom services later | server-authoritative economy closes the original's client-auth gap *[§6–8]* |
| Saves | server profile primary; platform snapshots backup; **never log save JSON** | fixes the original's logcat save leak *[§7]* |
| Config | 3-tier resolver kept; **typed, server-owned, server-side A/B** | keeps the good pattern, fixes client-random A/B *[§6]* |
| Anti-cheat | client obscuration (deterrence) + **server authority on economy** + ranked replay validation (post-launch) | original was telemetry-only *[§8]* |
| Analytics | **single** product+attribution pipeline | consolidates the original's triple stack *[§3]* |
| Content | Addressables + **remote catalog/CDN**; units/spells/balance as ScriptableObject → exported config + designer tooling | original was local-only *[§9]* |
| CI/CD | Unity Cloud Build (or self-hosted) + test gates + content-bundle pipeline + feature flags | new |
| Time | HTTP-Date trusted-time anchor kept; server time for ranked events | keep *[§8]* |

---

## 10. Art + audio bible

| Domain | Direction |
|---|---|
| **Visual identity** | Clean stylized 2D; **bold readable silhouettes**; faction palettes (Iron Pact = steel/cobalt; Ashen = ember/oxblood). Readability over detail. |
| **Animation** | **Spine** skeletal; **shared archetype skeletons + faction reskins** (the content-velocity engine); attack/hit/death **telegraphs** animated for counterplay clarity. |
| **VFX** | Damage-type-coded (Slash steel, Pierce white, Blunt dust, Magic violet, Fire orange); AoE/telegraph rings; **strict mobile particle budgets**. |
| **UI** | Minimal, thumb-reachable, high-contrast, **scalable**; HUD = Gold, unit queue, 3 spell slots, commander ability, formation toggle. |
| **Audio** | Punchy combat SFX; per-faction musical motif; light VO barks (identity); adaptive music (calm→intense by army-value ratio). |
| **Readability rules (mandatory)** | color-blind-safe palettes; silhouette-distinctiveness sign-off per unit; clear health/telegraph indicators; damage-number toggle. |
| **Content strategy** | One skeleton per archetype → faction skins → cosmetic recolors/VFX = **high cosmetic output at low cost** (funds the cosmetic-led model). |

---

## 11. Production plan

| Milestone | Duration | Goals | Staffing | Key risk | Success criteria |
|---|---|---|---|---|---|
| **M0 Foundation** | 1–2 mo | Tech spike (ECS sim core, determinism decision), pipeline, design docs locked; 1 unit-vs-unit on 1 map | core eng + 1 designer | over-architecting | sim core runs 200 units @ stable frame on mid phone |
| **Prototype** | 2–3 mo | Core loop fun-check: mine/train/push/statue, 1 faction, 4 units, basic AI, 1 terrained map, **no meta** | 4–5 | **combat not fun** (hard gate) | internal "one more game"; combat feel approved |
| **Vertical slice** | 3–4 mo | Prove pillars: 2 factions (6+6), terrain/formations/counters, 2 commanders, spell draft, AI layers, 3 maps, basic meta + monetization shell, 1 campaign act | 6–8 | depth not readable | external playtest D1 + qualitative retention signal |
| **Alpha** | 3–4 mo | Feature-complete MVP: campaign ~20 lvls, endless, async ghost ladder, Battle Pass S0, cosmetics, **server-auth economy**, analytics, RC | 6–8 | scope creep | MVP feature-complete + stable |
| **Soft launch** | 2–3 mo (limited geos) | Tune retention/monetization on real data; telemetry balance; funnel | 6–8 + data | retention/LTV below gate | **D1 ≥ ~40%, D7 ≥ ~18%** + early monetization gate (illustrative) |
| **Live launch** | — | Global; S1 ready; live-ops cadence running | 8–10 | live-ops sustainability | scale gate met in soft launch |

**MVP → soft launch ≈ 12–16 months.** Each gate can **kill or rescope** (esp. Prototype fun-check and Soft-launch LTV).

---

## 12. Risk register (ranked, brutal)

| # | Risk | Class | Severity | Mitigation |
|---|---|---|---|---|
| 1 | **Fair monetization doesn't reach sustaining LTV** | Commercial | **Critical** | Battle pass + cosmetic velocity + soft-launch conversion tuning; **pre-defined kill/scale gate**; ethics-bounded fallback levers (more cosmetics/bundles, not P2W) |
| 2 | **Combat (direct-control + terrain/formation/counter) isn't fun** | Design | **Critical** | Prototype is a hard fun-gate; kill/pivot before any backend/meta spend |
| 3 | **Deterministic ECS sim complexity** | Technical | High | Phase-gate: MVP non-deterministic + server stat-checks; determinism+replays post-launch |
| 4 | **Cosmetic-led model demands constant content** | Content | High | Shared-skeleton reskin pipeline; budget ongoing art; realistic 8-week cadence |
| 5 | **Combinatorial balance (terrain×formation×counter×spell)** | Design | High | Telemetry-driven, RC live-tuning, normalized ranked, small MVP roster |
| 6 | **Scope creep (heroes/clans/real-time)** | Production | Med-High | Strict phase-gating + decision log + revisit-triggers |
| 7 | **Crowded market / differentiation fails to land** | Commercial | Med | Lead on agency + fairness positioning; soft-launch messaging tests |
| 8 | **Live-ops unsustainable for small team** | Production | Med | Automate (CI/CD, config), conservative cadence, data-driven content |
| 9 | **Real-time PvP temptation** | Technical/Commercial | Med | Deferred to full vision; only if deterministic sim makes lockstep cheap |

---

## 13. Budget + team reality

| | **Lean indie (MVP→soft launch)** | **Funded studio (full vision)** |
|---|---|---|
| Team | **6–8**: 2 gameplay/ECS eng, 1 backend/live-ops eng, 2 Spine artists/animators, 1 designer (doubles PM), part-time/contract QA + audio | **20–30**: dedicated sim/ECS, backend, client, 6–8 art/anim, 2 design, live-ops, data analyst, QA |
| Timeline | 12–16 mo | 24–36 mo |
| Cash cost | ≈ **$0.8–1.5M** (salary-dominated; far less on founder sweat-equity) | ≈ **$4–8M+** |
| Tooling | Unity (free→Pro), **BaaS (PlayFab/Nakama)**, Spine licenses, CI, analytics; server ≈ low-$K/mo at MVP scale | + custom backend infra, data warehouse, larger CI |
| Outsourcing | **cosmetic art, audio, localization, overflow animation** (core combat/ECS/backend in-house) | same, larger |
| Live-ops cost | servers + **1–2 ongoing artists** (cosmetic cadence) + part-time live-ops | dedicated live-ops + data + art pod |

**Reality check:** fair monetization means the **cosmetic + battle-pass funnel must carry the title** — this raises required retention/conversion vs a P2W peer, and is the dominant commercial uncertainty. Lean-indie is the right risk posture; **earn studio-scale from a proven MVP, don't front-load it.**

---

## 14. Final recommendation (opinionated)

**Greenlight — conditionally, as a lean-indie MVP with hard gates. Yes.**

- **Why:** the dossier proves the core loop + agency retain for a decade; the open market gap (fair, tactically-deep, agency-first RTS-lite) is real; the tech is **de-risked by reusing the original's proven choices** (Unity/Spine/Addressables/RC-resolver/CRDT/time-anchor) and upgrading only the two things that limit it (ECS sim, server authority); and the scope is incrementally buildable behind gates.
- **Conditions (all required):**
  1. **Prototype fun-gate is binding** — if direct-control + terrain/formation combat isn't fun, kill/pivot before meta/backend spend.
  2. **Commit to fair monetization but validate LTV in soft launch** against a pre-defined **scale-or-stop** gate.
  3. **Phase-gate ruthlessly** — no heroes-collection / clans / real-time / determinism in MVP.
  4. **Build the cosmetic content pipeline early** (it funds the game).
  5. **Keep cadence realistic** (8-week seasons) for team size.
- **Would NOT** greenlight as a day-one funded studio-scale project: the fair-monetization-at-scale commercial risk is unproven; scale must be earned. As a founder: I'd raise a small seed/self-fund the **6–8-person lean MVP**, gate hard at prototype and soft launch, and only scale on validated retention + conversion.

---

## 15. Appendix — successor recommendation → production implementation

| Successor report (§) | Production implementation (this blueprint §) |
|---|---|
| Keep agency (direct control) | §4 control model: squad command + possess + commander ability |
| Terrain/formations/counters | §4 battlefield + formations(3) + type×armor matrix + positional flanking |
| Roguelite spell draft | §4 draft 3 of ~12 with synergy tags; §7 owned pool |
| Layered utility AI + influence maps + budgeted scheduler | §4 targeting + §9 ECS AI systems + scheduler |
| Adaptive director / multi-axis difficulty | §2 post-launch tuning; §4 difficultyMult |
| Server-authoritative economy | §9 backend (PlayFab/Nakama), §7 currencies server-owned |
| Cosmetic + battle-pass, no P2W, opt-in ads | §7 monetization integrity rules; §8 battle pass |
| Ranked async seasons + clans | §8 (post-launch); §2 phase-gated |
| Deterministic ECS sim → replays | §9 ECS now, determinism/replays post-launch |
| Remote content + consolidated analytics | §9 Addressables CDN + single analytics pipeline |
| Shared-skeleton cosmetic velocity | §10 content strategy |
| Lead PvE+async, defer real-time PvP | §2 cut line; §11 milestones; §14 recommendation |

---

*Planning + specification only — no reverse engineering, no implementation. Choices trace to dossier evidence, successor philosophy, and stated production constraints. Cuts/deferrals recorded in `PRODUCTION_DECISION_LOG.md`.*
