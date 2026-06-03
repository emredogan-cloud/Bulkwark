# BULWARK — Phase 3 Final Report

**Authorization:** [ADR-2-001](../../docs/adr/ADR-2-001-conditional-phase3-authorization.md) —
Conditional, AUTHOR-ONLY authorization prior to GATE 2 runtime validation. No runtime claims;
all runtime-dependent validations DEFERRED; GATE 1 OPEN, GATE 2 DEFERRED; stop at GATE 3.
**Owner-required actions (all done):** ADR-2-002 (commander/spell stacking) opened **and
implemented**; the FormationMember wiring plan documented (`docs/design/FormationMember_wiring_plan.md`);
all prior deferred runtime validations preserved.

## 1. Phase Summary
Phase 3 (Meta & Economy Shell) was authored end-to-end, turning the Phase-2 vertical slice into
a feature-complete MVP shell: a **server-authoritative** 4-currency economy, **capped** progression
(unit upgrades + commander levels), the **Campaign Act 1 (20 levels)** + **Endless** (adaptive
director) + **async ghost ladder** (stat-sanity, no replays), and **RemoteConfig live-tuning +
a consolidated analytics taxonomy** — all wrapping the Phase-2 combat through a clean §12 meta
boundary (modes call an `IBattleLauncher` seam, never the ECS assembly directly).

Built by an orchestrated pipeline: 5 service/mode modules **authored against the Phase-0 seams →
adversarially canon-verified → repaired**; module 3.5 (RC/analytics) failed to return and was
**authored directly**; then a **cross-module integration pass** dedup'd the server-economy seam
(one canonical `IServerEconomy`), reconciled the grant API, wired the meta→battle bridge + cap-
clamped resolved upgrades into the bake, fixed asmdef references to be acyclic, and **verified the
inviolables**. Separately, **ADR-2-002 was implemented** in the Phase-2 ECS files (StatusEffect
gained a `Source`; commander buffs are now clamped ≤ the §6 budget; spell buffs stay separate).

**Honest status (ADR-2-001):** no Unity/BaaS — the code is **authored, not compiled or round-tripped**.
GATE 3 ("economy server-validated") and all runtime checks are **DEFERRED**; the economy is
server-authoritative **by construction** but cannot be runtime-demonstrated without a backend.

## 2. Work Completed   (Done = authored, integrated & canon-verified; runtime validation DEFERRED)
| Sub | Objective | Status | Evidence (§ref) |
|---|---|---|---|
| **3.1** | Server-authoritative wallet (4 MVP currencies) | **Done (authored)** | [OBS] `_Services/Economy/ServerEconomy.cs` (`IServerEconomy` seam + `StubServerEconomy` re-validating spend/grant/caps server-side), `Wallet.cs` (cache-only client; `SetBalance` THROWS — no local mutation), `CloudSync/DistributedLedgerCache.cs` (obscured CRDT cache, reconciled FROM server, NOT source of truth). Gold excluded (in-battle/non-persistent). §9, §12 |
| **3.2** | Capped progression (unit upgrades + commander levels) | **Done (authored)** | [OBS] `_Services/Economy/Upgrades.cs` (rising-Silver tracks, **hard cap re-enforced server-side**, spend via `IServerEconomy`), `Progression.cs` (`ICommanderEconomy`, commander levels capped, leveling stays within the §6 budget; ranked-normalization stub). No P2W. §3, §12, §15 |
| **3.3** | Campaign Act 1 (20) + Endless + adaptive director | **Done (authored)** | [OBS] `_Game/Modes/Campaign/CampaignController.cs` (loads `CampaignLevelDef`, launches via seam, **diminishing first-clear gems `max(floor, base−dec×playCount)`** server-granted), `Endless/{EndlessController,AdaptiveDirector}.cs` (wave scaling adapting within a band), 20 `Data/Campaign/Act1/*.asset`. §6, §7 |
| **3.4** | Async ghost ladder (stat-sanity, no replays) | **Done (authored)** | [OBS] `_Game/Modes/AsyncLadder/{GhostSnapshot,AsyncLadderController}.cs` — capture/serve snapshots, **server-side `StatSanityValidator`** rejects implausible DPS/army/over-cap upgrades (detect-client/decide-server), server-granted rewards. No deterministic replay. §3.4, decision-log §3 |
| **3.5** | RemoteConfig economy tuning + analytics taxonomy | **Done (authored)** | [OBS] `_Services/RemoteConfig/EconomyResolver.cs` (typed RC→SO→literal economy getters over the Phase-0 `ConfigResolver`; live retune), `_Services/Analytics/Events.cs` (ONE consolidated taxonomy; minimized non-PII params; **never a wallet/save payload**). §12 |

## 3. Files Created
**Schemas:** `Data/Schemas/{EconomyTypes.cs, CampaignLevelDef.cs(+.meta), UpgradesConfig.cs(+.meta), EconomyConfig.cs(+.meta)}`.
**Services:** `_Services/Economy/{ServerEconomy,Wallet,Upgrades,Progression}.cs`, `_Services/CloudSync/DistributedLedgerCache.cs`, `_Services/RemoteConfig/EconomyResolver.cs`, `_Services/Analytics/Events.cs`.
**Modes:** `_Game/Modes/Economy/IModeRewardWallet.cs`, `_Game/Modes/IBattleLauncher.cs`, `_Game/Modes/Campaign/CampaignController.cs`, `_Game/Modes/Endless/{EndlessController,AdaptiveDirector}.cs`, `_Game/Modes/AsyncLadder/{GhostSnapshot,AsyncLadderController}.cs`, `_Game/Modes/Bulwark.Game.Modes.asmdef`.
**Bootstrap:** `_Game/Bootstrap/BattleLauncher.cs` (`IBattleLauncher`/`IAsyncBattleLauncher` impl).
**Data (22):** 20 `Data/Campaign/Act1/*.asset`, `Data/Economy/{UpgradesConfig,EconomyConfig}.asset`.
**Governance:** `docs/adr/ADR-2-001-...md`, `docs/adr/ADR-2-002-...md`, `docs/design/FormationMember_wiring_plan.md`, this report.

## 4. Files Modified
- `_Game/Bootstrap/BattleBootstrap.cs` — `playerUpgradeDeltas` hook (cap-clamped upgrade deltas folded into the baked `UnitSpawnStats`).
- `_Game/Bootstrap/Bulwark.Bootstrap.asmdef` — references `Bulwark.Game.Modes` + `Bulwark.Services` (Bootstrap implements the launcher seam).
- **ADR-2-002 (Phase-2 ECS files):** `Sim/Components/Phase2Components.cs` (`StatusSource` + `StatusEffect.Source`), `Sim/Systems/Spell.cs` (`StatusQuery` split by source + commander clamp), `Sim/Systems/CommanderAbility.cs` (writes `Source=Commander`, single policy), `Sim/Systems/Combat.cs` (net = spell × clamped-commander, §4 chain unchanged).
- **No canon doc modified** (the five `report/*.md` are untouched; verified).

## 5. Validation Results
**Static/authoring validation (executed here):**
- **Server authority over currency (INVIOLABLE) — PASS.** Only the server stub commits the profile; `Wallet` mutates its cache solely from server-confirmed values (`SetBalance` throws); the reward facade + all controllers pass a delta+reason and adopt the server result; `DistributedLedgerCache` is reconciled FROM the server (server wins). No client read-modify-write of an authoritative balance.
- **No save-state logging (INVIOLABLE) — PASS.** No file logs a wallet/profile/save payload; analytics emit event name + non-PII numerics only.
- **Capped progression / no P2W (INVIOLABLE) — PASS.** Hard caps re-enforced server-side in the spend path; resolved-upgrade bake uses cap-clamped deltas; the ladder validator rejects over-cap ghosts.
- **Scope — PASS.** 4 currencies only (Gold/Silver/Gems/PassXP; no Honor/Event); no monetization/shop/IAP/ads/pass/chests (Phase 4); gems earned-only; ladder uses stat-sanity (no replays); no new units/spells/maps/factions/commanders; no canon edits.
- **Integration coherence — PASS.** One canonical economy seam; all callers resolve; one `Bulwark.Game.Modes` asmdef; **acyclic** assembly graph (Data→∅; Services→Data; Modes→{Services,Data}; Sim→Data; Bootstrap→{Sim,Data,Modes,Services}); zero duplicate types repo-wide; all touched files brace-balanced; ADR-2-002 changes intact.

**Runtime validation (cannot execute here — DEFERRED):**
| Check | Result |
|---|---|
| server validates spend/grant (rejects tampered values) | **DEFERRED** (no backend) |
| upgrade caps enforced in a live profile | **DEFERRED** |
| campaign clearable + replayable; correct diminishing rewards | **DEFERRED** |
| ladder climbable; stat-checks reject anomalies | **DEFERRED** |
| RC retunes a value live; analytics events land | **DEFERRED** |
| **GATE 3: MVP feature-complete; economy server-validated** | **DEFERRED** — server-authoritative by construction, but not runtime-demonstrable without a BaaS; not asserted |

## 6. Known Issues
- Entire tree **authored, not compiled/round-tripped** — expect first-compile fixups + a real BaaS adapter behind `IBackendClient`/`IServerEconomy`.
- **Module 3.5 first-pass failed to return** (workflow `StructuredOutput` miss) → authored directly. **Module 3.2 declared a duplicate `IServerEconomy`** → dedup'd to the 3.1 canonical in integration.
- **Battle-launcher runtime is a documented stub** — `BattleBootstrapLauncher` maps the config + cap-clamped upgrade math, but the live battle Start, enemy-wave spawn (`BattleBootstrap` wave fields carried in the DTO), and `MatchState` poll are DEFERRED (no runtime). Campaign/Endless/Ladder are wired to the seam, not playable here.
- **FormationMember wiring** (Phase-2 carryover) — plan documented; implementation is a small pre-GATE-2 combat-layer follow-up.
- All economy/progression/level values are **PROVISIONAL** (LSD-owned, §16), RC-tunable; not asserted canon (§15.6).

## 7. Risks
- **Server-auth correctness unproven at runtime** — the design is server-authoritative, but tamper-rejection/caps/desync can only be proven against a real BaaS (DEFERRED). Highest Phase-3 risk.
- **Compounding validation debt** — GATE 1 OPEN, GATE 2 DEFERRED, now GATE 3 DEFERRED; all require a build + backend + playtest. If GATE 1/2 fail or pivot, meta layered on the core may need rework (accepted, ADR-1-001/-2-001).
- **Campaign/economy tuning** — 20-level curve + diminishing rewards + endless director are provisional; telemetry/RC tuning intended (not over-tuned now).
- Ladder snapshot fidelity (async, no replay) bounded by the stat-sanity validator; thresholds provisional.

## 8. ADRs Raised
- **ADR-2-001** (Accepted) — conditional author-only Phase-3 authorization + accepted risk; **linked at the top**.
- **ADR-2-002** (Accepted) — commander/spell buff-stacking resolution; **opened AND implemented** this phase (the §6 commander-attributable buff is now provably ≤ budget; spell buffs remain a separate counterable tactical layer). Closes the Phase-2 §6 open item.
- No other ADR required (no canon deviation; all mechanics trace to §3/§6/§7/§9/§12/§13).

## 9. Recommendations
1. **Stand up a BaaS** (PlayFab/Nakama) and implement the `IBackendClient`/`IServerEconomy` adapter + Cloud Script that re-derives every grant/spend/cap server-side; then run the deferred server-auth validations.
2. **Open `bulwark-clean` in Unity 6**, first-compile pass, wire the battle-launcher to a real `BattleBootstrap` Start + enemy-wave spawn, and assign inspector refs (campaign levels, UpgradesConfig, EconomyConfig, RC store).
3. **Burn down the deferred gates in order:** GATE 1 (fun) → GATE 2 (depth playtest) → GATE 3 (server-validated MVP). Do the FormationMember hook before the GATE 2 playtest.
4. **Do NOT begin Phase 4** (monetization) until GATE 3 passes — shop/IAP/ads/pass/chests stay forbidden (§J, §15).

## 10. Gate Status
- **GATE 3 (MVP feature-complete; economy server-validated):** **DEFERRED** — the meta/economy shell is authored, integrated, and canon-verified with server authority **structurally enforced**, but "server-validated" cannot be demonstrated without a backend, and the modes are not playable without a Unity build. **Not PASS, not FAIL.** No server-validation claimed.
- **Authorization to proceed to Phase 4:** **WITHHELD** — pending (a) a Unity build + BaaS, (b) the deferred GATE 1 + GATE 2 validations, and (c) GATE 3 runtime validation of the server-authoritative economy.
