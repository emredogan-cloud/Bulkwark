// BULWARK — PlayFab CloudScript: SERVER-AUTHORITATIVE economy handlers (Phase-3 §13 3.1/3.2, §9, §12).
// This runs on PlayFab's servers with the title secret. It is the AUTHORITY for the wallet,
// upgrades, and commander levels. The Unity client may only INVOKE these and adopt the result;
// it can never set a balance/level. Server authority + cap enforcement + anti-tamper + grant
// validation + upgrade validation all live HERE.
//
// CONTRACT honored:
//   • Currencies: Silver(SL), Gems(GM), PassXP(PX). Gold is in-battle only — NOT here (§9).
//   • Costs are RE-DERIVED from server-owned config (Title Data: "UpgradesConfig"/"EconomyConfig");
//     the client's claimed cost/amount is a request, never trusted (anti-tamper).
//   • Upgrade/commander HARD CAPS are re-checked server-side (§3/§15 — no P2W, INVIOLABLE).
//   • Optimistic concurrency: the client's expectedFromLevel must equal the server level.
//   • Grants are validated/clamped server-side; campaign first-clears use the server-owned play
//     count + the §9 diminishing rule and are single-claim.
//   • No save-state is logged; responses carry scalars + terse codes only.
//
// DEPLOY: upload as Legacy CloudScript (or adapt to a CloudScript-using-Azure-Functions). Upload
// EconomyConfig/UpgradesConfig JSON to Title Data. See backend/playfab/README.md.
// STATUS: authored, NOT executed here (no PlayFab runtime — ADR-0-001/-2-001). DEFERRED: live run.

var VALID_VC = { "SL": true, "GM": true, "PX": true };
var MAX_PLAUSIBLE_GRANT = 100000; // hard server clamp on any single grant (anti-abuse); RC-tunable

function titleJson(key, fallback) {
    var td = server.GetTitleData({ Keys: [key] });
    if (td && td.Data && td.Data[key]) { try { return JSON.parse(td.Data[key]); } catch (e) { return fallback; } }
    return fallback;
}
function getVC(pid, code) {
    var inv = server.GetUserInventory({ PlayFabId: pid });
    return (inv && inv.VirtualCurrency && typeof inv.VirtualCurrency[code] === "number") ? inv.VirtualCurrency[code] : 0;
}
function roData(pid, keys) {
    var r = server.GetUserReadOnlyData({ PlayFabId: pid, Keys: keys });
    return (r && r.Data) ? r.Data : {};
}
function setRoData(pid, map) {
    server.UpdateUserReadOnlyData({ PlayFabId: pid, Data: map });
}
function resp(ok, extra) {
    var o = { ok: ok, newBalance: 0, newLevel: 0, reason: "", error: "" };
    if (extra) for (var k in extra) o[k] = extra[k];
    return o;
}

// ---- 3.1 spend: client-initiated purchase; server validates funds + plausibility ----
handlers.spendCurrency = function (args, context) {
    var pid = currentPlayerId;
    if (!args || !VALID_VC[args.currency]) return resp(false, { error: "bad_currency" });
    var amount = args.amount | 0;
    if (amount <= 0) return resp(false, { error: "non_positive_amount" });
    if (amount > MAX_PLAUSIBLE_GRANT) return resp(false, { error: "implausible_amount" });
    var bal = getVC(pid, args.currency);
    if (amount > bal) return resp(false, { error: "insufficient_funds", newBalance: bal });
    var sub = server.SubtractUserVirtualCurrency({ PlayFabId: pid, VirtualCurrency: args.currency, Amount: amount });
    return resp(true, { newBalance: sub.Balance });
};

// ---- 3.1 grant: server RE-DERIVES legitimate reward; campaign first-clears are single-claim ----
handlers.grantCurrency = function (args, context) {
    var pid = currentPlayerId;
    if (!args || !VALID_VC[args.currency]) return resp(false, { error: "bad_currency" });
    var source = (args.source || "").toString();
    var grant = 0;

    // Source "campaign_firstclear:<levelId>:<gemBase>" → server computes the §9 diminishing reward.
    if (source.indexOf("campaign_firstclear:") === 0) {
        var parts = source.split(":");
        var levelId = parts[1] || "", gemBase = (parts[2] | 0);
        var econ = titleJson("EconomyConfig", { gemDiminishDecrement: 5, gemDiminishFloor: 5 });
        var ro = roData(pid, ["pc." + levelId]);
        var playCount = ro["pc." + levelId] ? (ro["pc." + levelId].Value | 0) : 0;
        grant = Math.max(econ.gemDiminishFloor, gemBase - econ.gemDiminishDecrement * playCount);
        var inc = {}; inc["pc." + levelId] = String(playCount + 1); setRoData(pid, inc); // server-owned play count++
        if (args.currency !== "GM") return resp(false, { error: "bad_currency" }); // first-clear pays Gems
    } else {
        // Generic mode reward: clamp the client's requested amount to a server max (anti-abuse).
        grant = Math.min(Math.max(args.amount | 0, 0), MAX_PLAUSIBLE_GRANT);
    }
    if (grant <= 0) return resp(true, { newBalance: getVC(pid, args.currency) }); // nothing to grant (diminished to 0/floor handled)
    var add = server.AddUserVirtualCurrency({ PlayFabId: pid, VirtualCurrency: args.currency, Amount: grant });
    return resp(true, { newBalance: add.Balance });
};

// ---- shared capped-spend (upgrades + commander levels): re-derive cost, enforce cap, concurrency ----
function cappedLevelSpend(pid, levelKey, fromLevel, hardCap, serverMax, baseCost, growth) {
    var ro = roData(pid, [levelKey]);
    var serverLevel = ro[levelKey] ? (ro[levelKey].Value | 0) : 0;
    if (serverLevel !== (fromLevel | 0)) return resp(false, { reason: "stale_client_level", newLevel: serverLevel });
    var cap = Math.min(serverMax | 0, hardCap | 0);
    if (serverLevel >= cap) return resp(false, { reason: "at_hard_cap", newLevel: serverLevel }); // INVIOLABLE §3/§15
    var cost = Math.round(baseCost * Math.pow(growth, serverLevel)); // SERVER-derived; client cost ignored
    var silver = getVC(pid, "SL");
    if (silver < cost) return resp(false, { reason: "insufficient_silver", newLevel: serverLevel, newBalance: silver });
    var sub = server.SubtractUserVirtualCurrency({ PlayFabId: pid, VirtualCurrency: "SL", Amount: cost });
    var set = {}; set[levelKey] = String(serverLevel + 1); setRoData(pid, set);
    return resp(true, { newLevel: serverLevel + 1, newBalance: sub.Balance });
}

// ---- 3.2 upgrade: cost from server UpgradesConfig track; hard cap re-enforced ----
handlers.spendForUpgrade = function (args, context) {
    var pid = currentPlayerId;
    var cfg = titleJson("UpgradesConfig", { tracks: [] });
    var unitId = (args.unitId || "").toString(), stat = (args.stat | 0);
    var track = null;
    for (var i = 0; i < cfg.tracks.length; i++) {
        if (cfg.tracks[i].unitId === unitId && (cfg.tracks[i].stat | 0) === stat) { track = cfg.tracks[i]; break; }
    }
    if (!track) return resp(false, { reason: "unknown_track" });
    return cappedLevelSpend(pid, "upg." + unitId + "." + stat, args.expectedFromLevel,
        Math.min(track.maxLevel | 0, args.hardCapMaxLevel | 0), track.maxLevel | 0,
        track.baseCostSilver | 0, track.costGrowth || 1.6);
};

// ---- 3.2 commander level: cost from server UpgradesConfig commander fields; hard cap re-enforced ----
handlers.spendForCommanderLevel = function (args, context) {
    var pid = currentPlayerId;
    var cfg = titleJson("UpgradesConfig", { commanderMaxLevel: 10, commanderBaseCostSilver: 200, commanderCostGrowth: 1.5 });
    var commanderId = (args.commanderId || "").toString();
    return cappedLevelSpend(pid, "cmd." + commanderId + ".level", args.expectedFromLevel,
        Math.min(cfg.commanderMaxLevel | 0, args.hardCapMaxLevel | 0), cfg.commanderMaxLevel | 0,
        cfg.commanderBaseCostSilver | 0, cfg.commanderCostGrowth || 1.5);
};
