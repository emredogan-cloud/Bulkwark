# BULWARK — PlayFab backend setup

Server-authoritative economy (§9/§12). Title ID **101A1B**. The authoritative logic lives in
`cloudscript/economy.js` (runs on PlayFab); the Unity client uses the adapters in
`Assets/_Services/Integrations/PlayFab/` to invoke it and adopt server-confirmed results.

## 1. Install the PlayFab Unity SDK + activate the adapters
The PlayFab adapters are isolated in `Bulwark.Services.PlayFab.asmdef`, which is
`defineConstraints: ["PLAYFAB_SDK"]` — **excluded from the build until you opt in**, so they
cannot break the baseline CI compile.
1. Import the **PlayFab Unity SDK** (provides the `PlayFabSDK` assembly + `PlayFab.*` namespaces).
2. Add the scripting define **`PLAYFAB_SDK`** (Project Settings → Player → Scripting Define Symbols,
   or via CI `-define`). The adapter asmdef then compiles and references `PlayFabSDK`.
3. Set the Title ID to `101A1B` (PlayFabSharedSettings asset, or pass it to `PlayFabBackendClient`).

## 2. Virtual currencies (server-owned wallet)
Create these Virtual Currencies in Game Manager → Economy:
| Code | Currency | Notes |
|---|---|---|
| `SL` | Silver | meta soft currency |
| `GM` | Gems | premium; earned at MVP; **never buys power** (§9) |
| `PX` | PassXP | battle-pass track xp |
**Gold is NOT a virtual currency** — it is in-battle / non-persistent (the ECS `GoldStore`, §9).

## 3. Upload server-owned config to Title Data
Export the SO data to JSON and set as Title Data (the CloudScript re-derives costs from these):
- `UpgradesConfig` ← `Assets/_Game/Data/Economy/UpgradesConfig.asset` (tracks + caps + commander curve).
- `EconomyConfig`  ← `Assets/_Game/Data/Economy/EconomyConfig.asset` (diminishing rule, mode rates).
These are also the SO tier of the 3-tier resolver; RemoteConfig overrides them live (§12).

## 4. Upload CloudScript
Upload `cloudscript/economy.js` as **Legacy CloudScript** (Automation → CloudScript), or port the
handlers to a CloudScript-using-Azure-Functions deployment. Handlers:
`spendCurrency`, `grantCurrency`, `spendForUpgrade`, `spendForCommanderLevel`.

## 5. Server-authority guarantees (enforced in `economy.js`)
- Costs are **re-derived server-side** from Title Data; the client's claimed cost/amount is a
  request, never trusted (**anti-tamper**).
- Upgrade/commander **hard caps re-checked server-side** (INVIOLABLE, §3/§15 — no P2W).
- **Optimistic concurrency**: the client's `expectedFromLevel` must equal the server level.
- Grants are validated/clamped; **campaign first-clears** use the server-owned play count + the
  §9 diminishing rule and are **single-claim**.
- The client **never** writes a balance/level; `PlayFabBackendClient.WriteProfileAsync` returns
  `false` by design (authoritative writes go through CloudScript).

## 6. CI secrets (already provisioned by the owner)
`PLAYFAB_SECRET_KEY` (server-side calls / title admin), `UNITY_EMAIL`, `UNITY_PASSWORD`,
`UNITY_LICENSE` (game-ci activation). The Unity build itself does not need the PlayFab secret;
it is for server-side/integration steps. **Never** commit secrets to the repo.

## STATUS
Authored, **not executed** here (no PlayFab runtime / no Unity — ADR-0-001/-2-001). Live
auth/spend/grant/upgrade validation against Title 101A1B is **DEFERRED** to a CI/device run with
the SDK installed and the CloudScript + Title Data deployed.
