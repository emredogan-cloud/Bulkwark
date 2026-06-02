# BULWARK Roadmap — Changelog & Lineage Ledger

Companion to `BULWARK_MASTER_DEVELOPMENT_ROADMAP.md`. Records what was **inherited** from Stick War (recovered in the dossier), what was **modernized**, what was **removed**, and the **final canonical replacement** for each. Disposition tags match the roadmap: **[PRESERVE] / [MODERNIZE] / [REPLACE] / [CUT]**.

---

## 1. Inherited systems (PRESERVE) — recovered from the dossier, kept as canon

| System | Dossier origin | Why kept | Roadmap home |
|---|---|---|---|
| Mine → train → push → statue loop | §4 | the decade-proven legible core | §3, §7 Classic |
| Direct unit control / possess | §5.1 | the agency hook | §3, §4 control model |
| Readable lane combat (row preference, statue priority) | §5.1 | predictable, coherent fronts | §3, §4 targeting |
| Persistent unit progression | §6 | growth + replay payoff | §3, §13 P3.2 |
| Campaign / Endless / async Tournament | §9 | proven PvE + cheap competition | §7 modes |
| Spell persistence (consumables + item ledger) | §6, §4.6b | meta tactical resource | §5.3 (→ draft) |
| Performance-first AI (O(1) targeting, throttled FSM) | §5.1 | mass-unit reach on mid phones | §4, §12 perf rule |
| 3-tier RemoteConfig resolver + diminishing-returns rewards | §6 | live-tunable, anti-grind | §9, §12 |
| CRDT currency ledger + HTTP-Date time anchor | §6, §8 | cheap integrity primitives | §9 (client cache under server auth) |
| Small, readable, Spine-animated content set | §9 | low cost, high clarity | §5 restraint discipline |
| Login streak / daily cadence / Play-Pass concept | §6 | retention loops | §8, §10 battle pass |

## 2. Modernized systems (MODERNIZE) — evolved, original intent preserved

| Original | Modern replacement | Why |
|---|---|---|
| One static lane, no terrain | front + terrain (high ground/choke/cover/hazard) | depth without losing readability |
| Binary `backstab ×2.0` flag | positional flank/back multipliers (geometry) | skill expression |
| Flat stat counters + 5 damage types | type×armor 5×4 counter matrix + 6 roles | real counterplay |
| Reactive threshold FSM | layered utility AI + influence maps + budgeted scheduler + multi-axis difficulty | adaptive yet O(1) |
| Thin identity | commanders (1 active+passive, capped power) | identity without P2W |
| Point-effect spells (inventory) | draft-3 roguelite loadout w/ synergy + telegraph/counter | tactical depth |
| Static gold-tap mine; iconic statue | contestable miner-capped mines; statue with shield phase + damage states | spatial economy; readable climax |
| Main-thread MonoBehaviour sim | ECS/DOTS fixed-timestep sim (UI stays MonoBehaviour) | scale, replays, future netcode |
| Local-only content; triple analytics; client-random A/B | Addressables+CDN; one analytics pipeline; server-side A/B | live-ops + data hygiene |

## 3. Removed / replaced (REPLACE & CUT) — with final canonical replacement

| Removed | Disposition | Final canonical replacement |
|---|---|---|
| Client-authoritative economy + telemetry-only anti-cheat | [REPLACE] | **server-authoritative** economy; client obscuration = deterrence only; ranked replay validation (P7) |
| Save-state JSON logged to logcat | [REPLACE] | server profile of record; **never log save state** |
| Opaque chests / gacha | [CUT] | **disclosed-odds, cosmetic-only earned chests**; paid = see-what-you-buy cosmetics |
| Interstitial ads | [CUT] | **opt-in rewarded ads only** |
| (Absent in original, rejected for successor) energy gates | [CUT] | none — session-respecting design |
| Pay-to-win / sellable power | [CUT] | **capped upgrades + ranked normalization + cosmetic-only spend** |
| Real-time netcode (not in original) | [CUT until P7] | **async-first**; real-time only if determinism + audience justify |
| ironSource/FMOD/Unity-Loc/Brain-scheduler (the dossier's *corrected* misreads) | n/a | not carried forward; correct understanding already in dossier §12 |

## 4. Net canonical replacements (summary)

- **Economy trust:** client-authoritative → **server-authoritative** (CRDT kept as cache).
- **Monetization:** interstitials + opaque chests → **cosmetic + battle pass + disclosed-odds earned chests + opt-in ads**, no P2W.
- **Combat:** flat single-lane → **terrain + formations + type×armor counters + positional flanking + spell synergy**.
- **AI:** reactive thresholds → **layered utility + influence maps + budgeted scheduler**, still O(1).
- **Tech:** main-thread sim → **ECS hot-path**; local content → **remote CDN**; triple analytics → **one pipeline**.
- **Identity:** thin → **factions (asymmetric) + capped commanders + outfit-class cosmetics**.

## 5. Process note

All PRESERVE/MODERNIZE/REPLACE/CUT dispositions are **binding canon** in the roadmap. Changing any one requires an **ADR** approved per the governance hierarchy (roadmap §16); **inviolable constraints** (readability, fairness/no-P2W, server authority, perf, disclosed odds) are **non-overridable**. This ledger is the authority for "what happened to the original system X?"

---

## 6. Verification & reconciliation pass

The roadmap was authored as a single coherent voice (to guarantee one consistent canon), then **adversarially verified** by a 6-critic pass (canon-consistency, internal-contradiction, hallucination/scope, phase-soundness, prompt-governance, spec-completeness). Result: **canon-consistency PASSED** (no recovered-fact misstatements, all PRESERVE/MODERNIZE/REPLACE/CUT dispositions consistent with this ledger and the decision log). The following **internal-consistency defects were found and fixed** (no design changes — wording/structure only):

| Fix | Defect | Resolution |
|---|---|---|
| Undefined currencies | §7 named *Honor* / *event currency* outside the canonical 4-currency wallet (tripping the §15 no-invented-currency rule) | Defined both in §9 as **post-launch, earned-only, cosmetic-only, server-authoritative** currencies; §7 now references §9; §13 3.1 marks the 4 as the **MVP** set |
| Wrong priority for deferred modes | §7 labelled Ranked Seasons + Seasonal modes **P1 (launch)** while the rest of the doc defers them to **Phase 7** | §7 priority key remapped to phases; both rows set to **P2 (Phase 7.1 / 7.6)**; basic weekend modifiers noted as the only launch-time seasonal content |
| Archetype count contradiction | §5.2 said "6 archetypes" but listed 7 rows (Flanker) with asymmetric faction coverage | Reframed as a **7-role shared palette, 6 units per faction**, with the Iron-Pact-Frontline / Ashen-Flanker asymmetry stated explicitly |
| Chest/currency name collision | chest tiers "Silver"/"Gold" collided with the Silver/Gold currencies | Added a naming note in §8 (tier names are cosmetic labels, distinct from currencies) |
| Per-season over-commit | §5 implied "+1 map/season" + "+0–1 unit/faction" + "3–4 commanders by launch" | Reconciled everything to the **single content slot per 8-week season** (one of {unit\|commander\|map}, §13 6.2) |
| Phase 5/6 schema + entry deps | Phase 5/6 tables dropped schema fields; GATE 5→launch dependency wasn't stated | §13 intro now scopes the full schema to Phases 0–5; **Entry: GATE 4 / GATE 5 must PASS** lines added; **GATE 6 launch-readiness** added |
| Vague gate metrics | GATE 2 had no testable bar; GATE 5 monetization half was "(illustrative)" | GATE 2 given a concrete playtest bar; GATE 5 given a **blended D30 LTV ≥ target CPI** floor (exact value via ADR, STOP-blocking) |
| Undefined "Gn" token | dependency cells used "G1/G2" with no legend | Added **legend "Gn = GATE n must PASS"** to §13 intro |
| Prompt authority gap | §14 preamble named only the roadmap, but Phase-7 triggers live in the decision log; Phase 6/7.x prompts missed Inputs/Deliverables | Preamble now also requires reading `PRODUCTION_DECISION_LOG.md`; Phase 6 + 7.x prompts given Inputs/Deliverables |

Post-fix state: the roadmap is internally consistent across §2–§16, all 17 required sections present and non-vague, no invented mechanics/currencies, phase dependencies acyclic and gate-checkable, and the agent prompts + governance close the drift paths the critics probed.
