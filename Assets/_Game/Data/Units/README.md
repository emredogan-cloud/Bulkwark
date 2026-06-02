# Iron Pact unit data (Phase 1)

The 4 Phase-1 units are authored as `UnitDef` ScriptableObject instances here (Iron
Pact only — §5.2). Roles map 1:1 to the roadmap's authorized Phase-1 set:

| Asset | Role (§5.2) | DamageType | Armor | Notes |
|---|---|---|---|---|
| `IronPact_Miner` | Miner | Melee | Light | mines Gold, weak combat |
| `IronPact_Shieldman` | Frontline | Melee | Shielded | line-hold tank |
| `IronPact_Legionary` | Skirmisher | Melee | Heavy | melee DPS |
| `IronPact_Crossbow` | Ranged | Pierce | Light | ranged DPS |

**All numeric values are PROVISIONAL.** Per §16 the Lead Systems Designer owns balance
values *within canon*; they are first-pass placeholders to make a playable micro-battle
exist, to be tuned by the GATE-1 fun pass + telemetry. They are **not asserted canon**
(§15.6) and live in data so the sim never hardcodes them (§12).

The `..._Crossbow` Pierce vs `..._Shieldman` Shielded counter (and Melee-vs-Light /
Melee-vs-Shielded) are the only "basic counters" active at Phase 1 — see
`../Balance/IronPactBalance.asset`. The full type×armor matrix is **Phase 2.2**.

These `.asset` files reference `Schemas/UnitDef.cs` (guid `7c1f0a1eb0a14b6da0000000beef0001`)
and were hand-authored (no Unity in the authoring env, ADR-0-001/-002); open in Unity 6
to verify import / regenerate `.meta`.
