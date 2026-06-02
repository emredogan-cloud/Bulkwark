# BULWARK — Production Decision Log

Companion to `NEXTGEN_RTS_PRODUCTION_BLUEPRINT.md`. Records the **major cuts, MVP exclusions, and deferrals**, each with a rationale and an explicit **revisit-trigger** (the condition under which the decision is reopened). Decisions are binding until their trigger fires. Status: **CUT** (not planned), **DEFER** (scheduled later), **GATE** (conditional on a milestone).

---

## 1. Cut from the product entirely (CUT)

| Decision | Rationale | Revisit-trigger |
|---|---|---|
| **No loot boxes / gacha** | Opacity is a retention drag + regulatory liability (loot-box disclosure laws); contradicts the "fair mastery" pillar. The original's chest/gacha is exactly what we reject. | None (principled). Only revisit if law/market fundamentally changes *and* it can be made fully transparent. |
| **No interstitial ads** | Mid-flow interruption hurts retention; the original's interstitial-heavy menu loop is a known drag. | None. Rewarded **opt-in** ads only. |
| **No energy / stamina gates** | Time-gating is widely disliked; the original wisely has none — keep that. | None. |
| **No pay-to-win / sellable raw power** | Destroys skill-based competition and the core positioning. | None (principled). |
| **No real-time PvP in MVP/post-launch core** | Netcode + matchmaking is the genre's biggest money pit; latency-fragile on mobile; async ladder delivers competition cheaply. | Deterministic lockstep sim exists *and* a validated audience demands it (full-vision only). |

## 2. Excluded from MVP, deferred to post-launch (DEFER)

| Feature | Why excluded from MVP | Scheduled | Trigger to pull forward |
|---|---|---|---|
| **Hero/commander *collection* economy** | Identity is needed (so MVP ships 1 commander/faction), but a collectible roster + talent economy multiplies balance/monetization surface before pillars are proven. | S1–S2 | MVP retention validated; commander system bones stable |
| **Clans / clan wars** | Strong retention multiplier but **not core-loop-critical**; needs social backend + moderation. | S1–S2 | Soft-launch retention met; backend capacity ready |
| **Ranked seasons w/ cheat-proof validation** | Requires deterministic replays (hard); MVP ships an async *ghost* ladder with server stat-sanity checks instead. | S1 (with determinism) | Deterministic sim landed (see §3) |
| **3rd / 4th faction** | Two asymmetric factions are enough to prove counters + replayability; more factions explode balance cost. | S2 (3rd), full vision (4th) | Two-faction balance stable in telemetry |
| **Biomes with deep mechanics** | MVP ships **one battlefield archetype (3 maps)** with terrain features; biome systems are content-heavy. | S2+ | Terrain layer validated as fun/readable |
| **Full event engine** | MVP ships **basic weekend modifiers** only; a calendar/event-authoring system is tooling-heavy. | S1–S3 | Live-ops bandwidth + tooling ready |
| **Units 7–9 per faction** | MVP caps at 6/faction for balance tractability + content cost. | rolling, per season | Per-season content slot |
| **PC/Steam build** | Mobile-first focus; cross-platform input/UX is added scope. | full vision | Mobile success + demand |

## 3. Technically phase-gated (GATE)

| Decision | MVP posture | Why gated | Trigger to upgrade |
|---|---|---|---|
| **Deterministic simulation + replays** | **MVP: non-deterministic ECS sim + server stat-sanity checks** | Cross-platform deterministic math (fixed-point/lockstep) is a major lift; gating MVP on it risks the schedule for a feature only ranked-validation strictly needs. | Ranked season development begins (post-launch) |
| **Custom backend services** | **MVP: managed BaaS (PlayFab/Nakama)** for auth/profile/economy/config | Building custom backend pre-validation wastes runway; BaaS is server-authoritative enough for MVP integrity. | Scale/feature needs exceed BaaS (post-launch) |
| **ECS for everything** | **MVP: ECS for battle sim only**; MonoBehaviour for UI/meta | Full-ECS UI/meta is unnecessary complexity. | Never (intentional boundary) |
| **Adaptive AI director** | MVP ships utility-AI + influence maps; director is basic | Director tuning needs telemetry from real sessions. | Post-launch tuning pass |

## 4. Scope reductions (numeric)

| Item | Vision | MVP | Rationale |
|---|---|---|---|
| Campaign length | 60+ levels | **~20 levels (1 act)** | Enough to teach + retain through soft launch; content cost |
| Spell pool | deep synergy web | **~12, draft 3** | Tractable balance; proves the draft mechanic |
| Commanders | full roster | **2 (1/faction)** | Establish identity + system, defer collection |
| Maps | biome variety | **3 (one archetype)** | Prove terrain layer cheaply |
| Currencies | many | **4** (Gold/Silver/Gems/PassXP) | Simplicity; avoid currency soup (a common F2P sin) |
| Season cadence | aggressive | **8 weeks** | Sustainable for a 6–8 person team |

## 5. Decisions explicitly preserved from the original (KEEP)

These recovered mechanics are *kept* on purpose (not re-litigated): direct unit control; mine→train→push→statue loop; persistent capped upgrades; **3-tier RemoteConfig resolver**; **diminishing-returns** first-clear rewards; **distributed CRDT currency ledger** (now as client cache under server authority); **HTTP-Date trusted-time anchor**; Spine 2D readability; **async-first** competition; frequent autosave + short cloud cadence. Rationale: the dossier identifies these as *why the original retained for a decade*; discarding them would be throwing away proven value.

---

## 6. Summary of the cut line

**MVP = the smallest set that proves: (a) is agency + terrain/formation/counter combat fun, and (b) does fair monetization retain.** Everything that doesn't directly serve (a) or (b) is CUT (predatory/dated patterns), DEFERred (social/seasonal/collection depth), or GATEd (determinism/backend complexity). The two binding kill/rescope checkpoints are the **Prototype fun-gate** and the **Soft-launch LTV gate** (blueprint §11, §14). This log is the authority for "why isn't X in the MVP?" — every exclusion has a reason and a revisit-trigger.
